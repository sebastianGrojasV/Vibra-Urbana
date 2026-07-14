namespace VibraUrbana.ViewModels;

public class ClienteHistorialCompraItemViewModel
{
    public int VentaId { get; set; }

    public DateTime FechaVenta { get; set; }

    public decimal Total { get; set; }

    public string Estado { get; set; } = string.Empty;

    public string? NumeroComprobante { get; set; }
}
