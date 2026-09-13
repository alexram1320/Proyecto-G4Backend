namespace Application.Common
{
    public sealed class RespuestaPaginada<T> : Respuesta<IReadOnlyCollection<T>>
    {
        public int Pagina
        {
            get; init;
        }
        public int TamanoPagina
        {
            get; init;
        }
        public int Total
        {
            get; init;
        }
        public int TotalPaginas => TamanoPagina == 0 ? 0 : (int)Math.Ceiling((double)Total / TamanoPagina);

        public static RespuestaPaginada<T> Crear(IReadOnlyCollection<T> datos, int pagina, int tamano, int total) =>
            new()
            {
                Exito = true,
                Datos = datos,
                CodigoEstado = 200,
                Mensaje = "Consulta completada",
                Pagina = pagina,
                TamanoPagina = tamano,
                Total = total
            };
    }
}
