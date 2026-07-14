namespace VibraUrbana.ViewModels;

public class ReporteVentasItemViewModel
{
    public int VentaId { get; set; }

    public string NumeroFactura { get; set; } = string.Empty;

    public DateTime FechaVenta { get; set; }

    public string Cliente { get; set; } = string.Empty;

    public string Cajero { get; set; } = string.Empty;

    public string MetodoPago { get; set; } = string.Empty;

    public decimal Subtotal { get; set; }

    public decimal Descuento { get; set; }

    public decimal Impuesto { get; set; }

    public decimal Total { get; set; }

    public string Estado { get; set; } = string.Empty;
}
