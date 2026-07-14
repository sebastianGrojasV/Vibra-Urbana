namespace VibraUrbana.ViewModels;

public class AdminDashboardViewModel
{
    public DateTime Fecha { get; set; } = DateTime.Today;

    public int VentasDelDia { get; set; }

    public decimal TotalVendidoDia { get; set; }

    public int ProductosStockBajo { get; set; }

    public int ClientesRegistrados { get; set; }

    public int VentasAprobadasDia { get; set; }

    public int VentasPendientesDia { get; set; }

    public int VentasAnuladasDia { get; set; }

    public IReadOnlyList<AdminDashboardMetodoPagoViewModel> VentasPorMetodoPago { get; set; } =
        Array.Empty<AdminDashboardMetodoPagoViewModel>();

    public IReadOnlyList<AdminDashboardStockItemViewModel> ProductosCriticos { get; set; } =
        Array.Empty<AdminDashboardStockItemViewModel>();

    public int TotalEstados => VentasAprobadasDia + VentasPendientesDia + VentasAnuladasDia;

    public decimal PorcentajeAprobadas => GetPercentage(VentasAprobadasDia);

    public decimal PorcentajePendientesAcumulado => GetPercentage(VentasAprobadasDia + VentasPendientesDia);

    public decimal TotalPorMetodoPago => VentasPorMetodoPago.Sum(item => item.Total);

    private decimal GetPercentage(int value)
    {
        return TotalEstados == 0 ? 0 : Math.Round(value * 100m / TotalEstados, 2);
    }
}
