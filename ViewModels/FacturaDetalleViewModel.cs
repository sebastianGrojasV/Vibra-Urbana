namespace VibraUrbana.ViewModels;

public class FacturaDetalleViewModel
{
    public int Id { get; set; }

    public int VentaId { get; set; }

    public string NumeroFactura { get; set; } = string.Empty;

    public DateTime FechaEmision { get; set; }

    public DateTime FechaVenta { get; set; }

    public string EstadoFactura { get; set; } = string.Empty;

    public string EstadoVenta { get; set; } = string.Empty;

    public string Cliente { get; set; } = string.Empty;

    public string IdentificacionCliente { get; set; } = string.Empty;

    public string TelefonoCliente { get; set; } = string.Empty;

    public string CorreoCliente { get; set; } = string.Empty;

    public string Cajero { get; set; } = string.Empty;

    public string MetodoPago { get; set; } = string.Empty;

    public string Observacion { get; set; } = string.Empty;

    public decimal Subtotal { get; set; }

    public decimal Descuento { get; set; }

    public decimal Impuesto { get; set; }

    public decimal Total { get; set; }

    public IReadOnlyList<FacturaDetalleItemViewModel> Productos { get; set; } = new List<FacturaDetalleItemViewModel>();
}
