namespace VibraUrbana.ViewModels;

public class FacturaListadoItemViewModel
{
    public int Id { get; set; }

    public int VentaId { get; set; }

    public string NumeroFactura { get; set; } = string.Empty;

    public DateTime FechaEmision { get; set; }

    public string Cliente { get; set; } = string.Empty;

    public string IdentificacionCliente { get; set; } = string.Empty;

    public string Cajero { get; set; } = string.Empty;

    public string MetodoPago { get; set; } = string.Empty;

    public decimal Total { get; set; }

    public string Estado { get; set; } = string.Empty;
}
