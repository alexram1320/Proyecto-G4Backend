namespace Infrastructure.Authentication
{
    public sealed class ConfiguracionJwt
    {
        public string Emisor { get; set; } = string.Empty;
        public string Audiencia { get; set; } = string.Empty;
        public string Clave { get; set; } = string.Empty;
        public int MinutosExpiracion { get; set; } = 60;
    }
}
