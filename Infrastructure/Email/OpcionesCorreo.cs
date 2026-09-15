namespace Infrastructure.Email
{
    public sealed class OpcionesCorreo
    {
        public bool Habilitado { get; set; }
        public string Servidor { get; set; } = "smtp.gmail.com";
        public int Puerto { get; set; } = 587;
        public string Usuario { get; set; } = string.Empty;
        public string ContrasenaAplicacion { get; set; } = string.Empty;
        public string NombreRemitente { get; set; } = "ApagónYa";
    }
}
