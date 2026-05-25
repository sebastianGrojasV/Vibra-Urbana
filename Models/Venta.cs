namespace VibraUrbana.Models;

public class Venta
{
    public int Id { get; set; }

    public int ClienteId { get; set; }

    public Cliente Cliente { get; set; } = null!;

    public int UsuarioId { get; set; }

    public Usuario Usuario { get; set; } = null!;

    public int MetodoPagoId { get; set; }

    public MetodoPago MetodoPago { get; set; } = null!;

    public DateTime FechaVenta { get; set; } = DateTime.UtcNow;

    public decimal Subtotal { get; set; }

    public decimal Descuento { get; set; }

    public decimal Impuesto { get; set; }

    public decimal Total { get; set; }

    public string Estado { get; set; } = "Registrada";

    public string Observacion { get; set; } = string.Empty;

    public int? CierreCajaId { get; set; }

    public CierreCaja? CierreCaja { get; set; }

    public ICollection<DetalleVenta> DetalleVentas { get; set; } = new List<DetalleVenta>();

    public Factura? Factura { get; set; }
}
