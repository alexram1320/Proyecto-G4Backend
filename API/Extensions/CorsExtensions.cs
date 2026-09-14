namespace API.Extensions
{
    public static class CorsExtensions
    {
        public static IServiceCollection AddCorsAngular(
        this IServiceCollection services,
        IConfiguration configuration)
        {
            var origenes = configuration
                .GetSection("CORS:Origenes")
                .Get<string[]>() ?? ["http://localhost:4200"];

            services.AddCors(opciones =>
            {
                opciones.AddPolicy("Angular", politica =>
                {
                    politica
                        .WithOrigins(origenes)
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            return services;
        }
    }
}
