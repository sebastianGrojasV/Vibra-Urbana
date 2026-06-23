namespace VibraUrbana.ViewModels;

public class ClienteCompraDetalleViewModel
{
    public int ClienteId { get; set; }

    public string Cliente { get; set; } = string.Empty;

    public string Identificacion { get; set; } = string.Empty;

    public int VentaId { get; set; }

    public DateTime FechaVenta { get; set; }

    public string Cajero { get; set; } = string.Empty;

    public string MetodoPago { get; set; } = string.Empty;

    public string Estado { get; set; } = string.Empty;

    public string Observacion { get; set; } = string.Empty;

    public string? NumeroComprobante { get; set; }

    public decimal Subtotal { get; set; }

    public decimal Descuento { get; set; }

    public decimal Impuesto { get; set; }

    public decimal Total { get; set; }

    public IReadOnlyList<ClienteCompraDetalleItemViewModel> Productos { get; set; } = [];
}
