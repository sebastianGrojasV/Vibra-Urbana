namespace VibraUrbana.Models;

public class CierreCaja
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }

    public Usuario Usuario { get; set; } = null!;

    public DateTime FechaCierre { get; set; } = DateTime.UtcNow;

    public decimal TotalVentas { get; set; }

    public decimal TotalEfectivo { get; set; }

    public decimal TotalSinpe { get; set; }

    public decimal TotalTarjeta { get; set; }

    public int CantidadVentas { get; set; }

    public string Observacion { get; set; } = string.Empty;

    public ICollection<Venta> Ventas { get; set; } = new List<Venta>();
}
