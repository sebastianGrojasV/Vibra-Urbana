namespace VibraUrbana.ViewModels;

public class CierreCajaVentaItemViewModel
{
    public int VentaId { get; set; }

    public string NumeroFactura { get; set; } = string.Empty;

    public DateTime FechaVenta { get; set; }

    public string Cliente { get; set; } = string.Empty;

    public string Cajero { get; set; } = string.Empty;

    public string MetodoPago { get; set; } = string.Empty;

    public decimal Total { get; set; }

    public string Estado { get; set; } = string.Empty;
}
