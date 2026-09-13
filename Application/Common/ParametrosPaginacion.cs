using System.ComponentModel.DataAnnotations;

namespace Application.Common
{
    public class ParametrosPaginacion
    {
        [Range(1, int.MaxValue)]
        public int Pagina { get; set; } = 1;

        [Range(1, 100)]
        public int TamanoPagina { get; set; } = 20;
    }
}
