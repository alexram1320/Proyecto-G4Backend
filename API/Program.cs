using ApagonYa.API;
using ApagonYa.Application;
using ApagonYa.Infrastructure;
using API.Middleware;
using Application.Interfaces.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("correo.Local.json", optional: true, reloadOnChange: false);
builder.Configuration.AddEnvironmentVariables();
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApi();


var app = builder.Build();

if (args.Contains("--provision-admin", StringComparer.OrdinalIgnoreCase))
{
    await using var scope = app.Services.CreateAsyncScope();
    var provisionador = scope.ServiceProvider.GetRequiredService<IServicioProvisionamientoAdministrador>();
    var administrador = await provisionador.ProvisionarAsync(
        builder.Configuration["AdminInicial:Nombre"] ?? string.Empty,
        builder.Configuration["AdminInicial:Email"] ?? string.Empty,
        builder.Configuration["AdminInicial:Contrasena"] ?? string.Empty,
        CancellationToken.None);
    Console.WriteLine($"Administrador provisionado: {administrador.Email} ({administrador.Id})");
    return;
}

app.UseMiddleware<MiddlewareExcepciones>();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseCors("Angular");
app.UseAuthentication();
app.UseMiddleware<MiddlewareUsuarioActivo>();
app.UseAuthorization();
app.MapControllers();
app.Run();

public partial class Program;
