using System.Text.Json;
using System.Text.Json.Serialization;
using Google.Cloud.Firestore;

namespace Infrastructure.Firebase
{
    internal class MapeoFirestore
    {
        private static readonly JsonSerializerOptions Opciones = new()
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };

        public static Dictionary<string, object> Diccionario<T>(T entidad)
        {
            var elemento = JsonSerializer.SerializeToElement(entidad, Opciones);
            return (Dictionary<string, object>)ConvertirValor(elemento)!;
        }

        public static T Entidad<T>(DocumentSnapshot documento)
        {
            var normalizado = documento
                .ToDictionary()
                .ToDictionary(
                    campo => campo.Key,
                    campo => NormalizarValor(campo.Value));
            var json = JsonSerializer.Serialize(normalizado, Opciones);

            return JsonSerializer.Deserialize<T>(json, Opciones) ??
                throw new InvalidOperationException($"No se pudo convertir {typeof(T).Name}");
        }

        private static object? NormalizarValor(object? valor)
        {
            return valor switch
            {
                Timestamp timestamp => timestamp.ToDateTime(),
                IDictionary<string, object> diccionario => diccionario.ToDictionary(
                    campo => campo.Key,
                    campo => NormalizarValor(campo.Value)),
                IEnumerable<object> arreglo => arreglo.Select(NormalizarValor).ToArray(),
                _ => valor
            };
        }

        private static object? ConvertirValor(JsonElement elemento)
        {
            return elemento.ValueKind switch
            {
                JsonValueKind.Object => elemento
                    .EnumerateObject()
                    .ToDictionary(
                        propiedad => propiedad.Name,
                        propiedad => ConvertirValor(propiedad.Value)!),
                JsonValueKind.Array => elemento
                    .EnumerateArray()
                    .Select(ConvertirValor)
                    .ToArray(),
                JsonValueKind.String when elemento.TryGetDateTime(out var fecha) =>
                    fecha.ToUniversalTime(),
                JsonValueKind.String => elemento.GetString(),
                JsonValueKind.Number when elemento.TryGetInt64(out var entero) => entero,
                JsonValueKind.Number => elemento.GetDouble(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                _ => null
            };
        }
    }
}
