namespace SistemadeRenta.Domain.Entities
{
    public class Usuario
    {
        public int IdUsuario { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string ContrasenaHash { get; set; } = string.Empty;
        public string Rol { get; set; } = "Cliente"; // Admin / Cliente
        public bool Activo { get; set; } = true;
    }
}
