using System.Text.Json.Serialization;

namespace ApagonYa.API;

public static class DependencyInjection
{
    public static IServiceCollection AddApi(this IServiceCollection services)
    {
        services
            .AddControllers()
            .AddJsonOptions(opciones =>
            {
                opciones.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            })
            .ConfigureApiBehaviorOptions(opciones =>
            {
               
            });

        return services;
    }
}
