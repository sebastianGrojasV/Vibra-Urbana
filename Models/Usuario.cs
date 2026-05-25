namespace VibraUrbana.Models;

public class Usuario
{
    public int Id { get; set; }

    public string Cedula { get; set; } = string.Empty;

    public string NombreCompleto { get; set; } = string.Empty;

    public string Correo { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public bool Activo { get; set; } = true;

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public int RolId { get; set; }

    public Rol Rol { get; set; } = null!;

    public ICollection<MovimientoInventario> MovimientosInventario { get; set; } = new List<MovimientoInventario>();

    public ICollection<Venta> Ventas { get; set; } = new List<Venta>();

    public ICollection<CierreCaja> CierresCaja { get; set; } = new List<CierreCaja>();

    public ICollection<Bitacora> Bitacoras { get; set; } = new List<Bitacora>();
}
