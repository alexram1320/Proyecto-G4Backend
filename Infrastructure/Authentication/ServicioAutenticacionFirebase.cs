using System.Net.Http.Json;
using System.Text.Json;
using Application.Interfaces.Infrastructure;
using FirebaseAdmin.Auth;
using Infrastructure.Firebase;

namespace Infrastructure.Authentication
{
    public sealed class ServicioAutenticacionFirebase(HttpClient http, ConfiguracionFirebase configuracion, FirebaseAuth autenticacion) : IServicioAutenticacionFirebase
    {
        public async Task ActualizarEmailAsync(string uid, string email, CancellationToken ct)
        {
            try
            {
                await autenticacion.UpdateUserAsync(new UserRecordArgs
                {
                    Uid = uid,
                    Email = email,
                    EmailVerified = false
                }, ct);
            }
            catch (FirebaseAuthException ex) when (ex.AuthErrorCode == AuthErrorCode.EmailAlreadyExists)
            {
                throw new InvalidOperationException("El correo ya esta registrado en otra cuenta");
            }
        }

        public async Task<string> CrearUsuarioAsync(
            string email,
            string contrasena,
            string nombre,
            string rol,
            CancellationToken ct)
        {
            var argumentos = new UserRecordArgs
            {
                Email = email,
                Password = contrasena,
                DisplayName = nombre,
                EmailVerified = false,
                Disabled = false
            };

            var registro = await autenticacion.CreateUserAsync(argumentos, ct);
            var claims = new Dictionary<string, object> { ["role"] = rol };

            await autenticacion.SetCustomUserClaimsAsync(registro.Uid, claims, ct);
            return registro.Uid;
        }

        public async Task<UsuarioFirebase?> BuscarUsuarioPorEmailAsync(
            string email,
            CancellationToken ct)
        {
            try
            {
                var usuario = await autenticacion.GetUserByEmailAsync(email, ct);
                return new UsuarioFirebase(usuario.Uid, usuario.Email);
            }
            catch (FirebaseAuthException ex) when (ex.AuthErrorCode == AuthErrorCode.UserNotFound)
            {
                return null;
            }
        }

        public async Task<SesionFirebase> IniciarSesionAsync(
            string email,
            string contrasena,
            CancellationToken ct)
        {
            //Endpoint contiene la documentacion de esta forma de autenticacion de firebase
            //https://firebase.google.com/docs/reference/rest/auth?utm_source=chatgpt.com
            var url = $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={configuracion.Opciones.ApiKeyWeb}";
            var solicitud = new
            {
                email,
                password = contrasena,
                returnSecureToken = true
            };

            var respuesta = await http.PostAsJsonAsync(url, solicitud, ct);
            return await LeerSesionAsync(respuesta, ct);
        }

        public async Task<SesionFirebase> RenovarAsync(
            string tokenRenovacion,
            CancellationToken ct)
        {
            var campos = new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token",
                ["refresh_token"] = tokenRenovacion
            };
            var url = $"https://securetoken.googleapis.com/v1/token?key={configuracion.Opciones.ApiKeyWeb}";

            using var contenido = new FormUrlEncodedContent(campos);
            var respuesta = await http.PostAsync(url, contenido, ct);

            return await LeerSesionAsync(respuesta, ct);
        }

        public Task<string> CrearTokenPersonalizadoAsync(string uid, CancellationToken ct)
        {
            return autenticacion.CreateCustomTokenAsync(uid, cancellationToken: ct);
        }

        public async Task EnviarRecuperacionAsync(string email, CancellationToken ct)
        {
            var url = $"https://identitytoolkit.googleapis.com/v1/accounts:sendOobCode?key={configuracion.Opciones.ApiKeyWeb}";
            var solicitud = new
            {
                requestType = "PASSWORD_RESET",
                email
            };
            var respuesta = await http.PostAsJsonAsync(url, solicitud, ct);
            if (!respuesta.IsSuccessStatusCode)
            {
                var contenido = await respuesta.Content.ReadAsStringAsync(ct);
                if (contenido.Contains("EMAIL_NOT_FOUND", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                respuesta.EnsureSuccessStatusCode();
            }
        }

        public Task AsignarClaimsAsync(
            string uid,
            IReadOnlyDictionary<string, object> claims,
            CancellationToken ct)
        {
            //https://firebase.google.com/docs/auth/admin/custom-claims?hl=es-419    firebase lo maneja asi, el rol y la zona también están en el perfil users de Firestore,
            return autenticacion.SetCustomUserClaimsAsync(uid, claims, ct);
        }

        public Task RevocarTokensAsync(string uid, CancellationToken ct)
        {
            return autenticacion.RevokeRefreshTokensAsync(uid, ct);
        }

        public Task DeshabilitarAsync(string uid, bool deshabilitado, CancellationToken ct)
        {
            var argumentos = new UserRecordArgs
            {
                Uid = uid,
                Disabled = deshabilitado
            };

            return autenticacion.UpdateUserAsync(argumentos, ct);
        }

        private static async Task<SesionFirebase> LeerSesionAsync(
            HttpResponseMessage respuesta,
            CancellationToken ct)
        {
            var json = await respuesta.Content.ReadAsStringAsync(ct);

            if (!respuesta.IsSuccessStatusCode)
            {
                throw new UnauthorizedAccessException("Credenciales de Firebase invalidas");
            }

            using var documento = JsonDocument.Parse(json);
            var raiz = documento.RootElement;

            string Valor(params string[] nombres)
            {
                foreach (var nombre in nombres)
                {
                    if (raiz.TryGetProperty(nombre, out var valor))
                    {
                        return valor.GetString() ?? string.Empty;
                    }
                }

                return string.Empty;
            }

            _ = int.TryParse(Valor("expiresIn", "expires_in"), out var segundos);

            return new SesionFirebase(
                Valor("localId", "user_id"),
                Valor("idToken", "id_token"),
                Valor("refreshToken", "refresh_token"),
                segundos);
        }
    }
}
