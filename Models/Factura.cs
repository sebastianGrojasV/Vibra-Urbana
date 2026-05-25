namespace VibraUrbana.Models;

public class Factura
{
    public int Id { get; set; }

    public int VentaId { get; set; }

    public Venta Venta { get; set; } = null!;

    public string NumeroFactura { get; set; } = string.Empty;

    public DateTime FechaEmision { get; set; } = DateTime.UtcNow;

    public decimal Subtotal { get; set; }

    public decimal Descuento { get; set; }

    public decimal Impuesto { get; set; }

    public decimal Total { get; set; }

    public string Estado { get; set; } = "Emitida";
}
