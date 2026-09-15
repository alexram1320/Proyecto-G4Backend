using Domain.Constants;
using Google.Cloud.Firestore;
using Infrastructure.Firebase;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Infrastructure.Email
{

    //Una sola instancia viva para todo el ciclo de vida del proyecto
    public sealed class EmisorCorreo(IServiceProvider servicios, IOptions<OpcionesCorreo> opciones, ILogger<EmisorCorreo> logger) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var config = opciones.Value;
            if (!config.Habilitado) return;

            using var smtp = new SmtpClient { Timeout = 30_000 };
            var ultimoUso = DateTime.UtcNow;
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var contexto = servicios.GetRequiredService<ContextoFirestore>();
                        var cola = contexto.Coleccion("emailOutbox");
                        var pendientes = await cola.WhereEqualTo("Pendiente", true).GetSnapshotAsync(stoppingToken);
                        foreach (var entrada in pendientes.Documents
                            .Where(d => d.GetValue<DateTime>("ProximoIntento") <= DateTime.UtcNow)
                            .OrderBy(d => d.GetValue<DateTime>("ProximoIntento")).Take(20))
                        {
                            var adquirido = await contexto.BaseDatos.RunTransactionAsync(async t =>
                            {
                                var actual = await t.GetSnapshotAsync(entrada.Reference, stoppingToken);
                                if (!actual.GetValue<bool>("Pendiente") ||
                                    actual.GetValue<DateTime>("ProximoIntento") > DateTime.UtcNow) return false;
                                t.Update(entrada.Reference, "ProximoIntento", DateTime.UtcNow.AddMinutes(5));
                                return true;
                            }, cancellationToken: stoppingToken);
                            if (!adquirido) continue;

                            var intentos = entrada.GetValue<int>("Intentos") + 1;
                            using var limiteEnvio = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
                            limiteEnvio.CancelAfter(TimeSpan.FromMinutes(1));
                            var envioToken = limiteEnvio.Token;
                            try
                            {
                                var usuario = await contexto.Coleccion(ColeccionesFirestore.Usuarios)
                                    .Document(entrada.GetValue<string>("UsuarioId")).GetSnapshotAsync(envioToken);
                                if (!usuario.Exists || !usuario.GetValue<bool>("Activo") ||
                                    !MailboxAddress.TryParse(usuario.GetValue<string>("Email"), out var destinatario))
                                {
                                    await FinalizarAsync(entrada.Reference, "DESTINATARIO_NO_DISPONIBLE", envioToken);
                                    continue;
                                }

                                // Una reasignación pendiente también revoca los avisos de correo del técnico anterior.
                                if (usuario.GetValue<string>("Rol") == "TECHNICIAN")
                                {
                                    var reporteId = entrada.GetValue<string>("ReporteId");
                                    var reporte = string.IsNullOrEmpty(reporteId) ? null : await contexto
                                        .Coleccion(ColeccionesFirestore.Reportes).Document(reporteId).GetSnapshotAsync(envioToken);
                                    var tecnicoId = reporte is { Exists: true } && reporte.TryGetValue<string>("TecnicoId", out var id) ? id : null;
                                    var tecnico = string.IsNullOrEmpty(tecnicoId) ? null : await contexto
                                        .Coleccion(ColeccionesFirestore.Tecnicos).Document(tecnicoId).GetSnapshotAsync(envioToken);
                                    if (tecnico is not { Exists: true } || !tecnico.GetValue<bool>("Activo") ||
                                        tecnico.GetValue<string>("UsuarioId") != usuario.Id)
                                    {
                                        await FinalizarAsync(entrada.Reference, "ASIGNACION_CAMBIADA", envioToken);
                                        continue;
                                    }
                                }

                                if (!smtp.IsConnected)
                                    await smtp.ConnectAsync(config.Servidor, config.Puerto, SecureSocketOptions.StartTls, envioToken);
                                if (!smtp.IsAuthenticated)
                                    await smtp.AuthenticateAsync(config.Usuario, config.ContrasenaAplicacion.Replace(" ", ""), envioToken);

                                var mensaje = new MimeMessage();
                                mensaje.From.Add(new MailboxAddress(config.NombreRemitente, config.Usuario));
                                mensaje.To.Add(destinatario);
                                mensaje.Subject = "ApagónYa: " + entrada.GetValue<string>("Titulo");
                                mensaje.MessageId = entrada.Id + "@apagonya.local";
                                mensaje.Body = new TextPart("plain")
                                {
                                    Text = entrada.GetValue<string>("Mensaje") +
                                        "\n\nEntra a ApagónYa para consultar tus notificaciones y el detalle del reporte."
                                };
                                await smtp.SendAsync(mensaje, envioToken);
                                ultimoUso = DateTime.UtcNow;
                                await FinalizarAsync(entrada.Reference, "ENVIADO", envioToken);
                            }
                            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { throw; }
                            catch (Exception error)
                            {
                                // No registrar mensajes del servidor, destinatarios ni credenciales.
                                logger.LogWarning("Correo {Id}: fallo {Tipo}, intento {Intento}.", entrada.Id, error.GetType().Name, intentos);
                                await entrada.Reference.UpdateAsync(new Dictionary<string, object>
                                {
                                    ["Intentos"] = intentos,
                                    ["Pendiente"] = intentos < 8,
                                    ["Estado"] = intentos < 8 ? "REINTENTO" : "FALLIDO",
                                    ["ProximoIntento"] = DateTime.UtcNow.AddMinutes(Math.Min(60, Math.Pow(2, intentos)))
                                }, cancellationToken: stoppingToken);
                                // Pausa global ante fallos: evita repetir autenticaciones o superar cuotas en ráfaga.
                                await Task.Delay(TimeSpan.FromMinutes(Math.Min(60, Math.Pow(2, intentos))), stoppingToken);
                                break;
                            }
                            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
                        }

                        if (smtp.IsConnected && DateTime.UtcNow - ultimoUso > TimeSpan.FromMinutes(2))
                        {
                            using var pulso = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
                            pulso.CancelAfter(TimeSpan.FromSeconds(30));
                            await smtp.NoOpAsync(pulso.Token);
                            ultimoUso = DateTime.UtcNow;
                        }
                    }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { throw; }
                    catch (Exception error)
                    {
                        logger.LogWarning("Servicio de correo: fallo {Tipo}. Se reintentará en un minuto.", error.GetType().Name);
                        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                    }
                    await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
            finally
            {
                if (smtp.IsConnected)
                {
                    using var cierre = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                    try { await smtp.DisconnectAsync(true, cierre.Token); }
                    catch (Exception) { /* Dispose libera la conexión aunque Gmail no responda al cierre. */ }
                }
            }
        }

        private static Task FinalizarAsync(DocumentReference documento, string estado, CancellationToken ct) =>
            documento.UpdateAsync(new Dictionary<string, object>
            {
                ["Pendiente"] = false,
                ["Estado"] = estado,
                ["FechaFinalizacion"] = DateTime.UtcNow
            }, cancellationToken: ct);
    }
