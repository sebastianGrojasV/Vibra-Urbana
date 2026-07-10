namespace VibraUrbana.ViewModels;

public class CierreCajaViewModel
{
    public DateTime Fecha { get; set; }

    public int CantidadVentas { get; set; }

    public int CantidadVentasAprobadas { get; set; }

    public int CantidadVentasAnuladas { get; set; }

    public int CantidadVentasPendientes { get; set; }

    public int VentasDisponiblesParaCierre { get; set; }

    public decimal TotalVentas { get; set; }

    public decimal TotalEfectivo { get; set; }

    public decimal TotalSinpe { get; set; }

    public decimal TotalTarjeta { get; set; }

    public bool PuedeRegistrarCierre => VentasDisponiblesParaCierre > 0;

    public IReadOnlyList<CierreCajaMetodoPagoViewModel> TotalesPorMetodoPago { get; set; } = new List<CierreCajaMetodoPagoViewModel>();

    public IReadOnlyList<CierreCajaVentaItemViewModel> Ventas { get; set; } = new List<CierreCajaVentaItemViewModel>();
}
