namespace Application.Common
{
    public class Respuesta<T>
    {
        public bool Exito
        {
            get; init;
        }
        public string Mensaje { get; init; } = string.Empty;
        public IReadOnlyCollection<string> Errores { get; init; } = [];
        public T? Datos
        {
            get; init;
        }
        public int CodigoEstado
        {
            get; init;
        }

        public static Respuesta<T> Correcta(T datos, string mensaje = "Operacion completada", int codigo = 200) =>
            new()
            {
                Exito = true,
                Datos = datos,
                Mensaje = mensaje,
                CodigoEstado = codigo
            };

        public static Respuesta<T> Fallida(string mensaje, int codigo, params string[] errores) =>
            new()
            {
                Mensaje = mensaje,
                CodigoEstado = codigo,
                Errores = errores
            };
    }
}
