namespace VibraUrbana.ViewModels;

public class ReporteVentasViewModel
{
    public const string PeriodoDia = "dia";
    public const string PeriodoSemana = "semana";
    public const string PeriodoMes = "mes";
    public const string PeriodoRango = "rango";

    public string Periodo { get; set; } = PeriodoDia;

    public DateTime FechaInicio { get; set; }

    public DateTime FechaFin { get; set; }

    public IReadOnlyList<ReporteVentasItemViewModel> Ventas { get; set; } = [];

    public IReadOnlyList<ReporteVentasMetodoPagoViewModel> TotalesPorMetodoPago { get; set; } = [];

    public int CantidadVentas => Ventas.Count;

    public int CantidadVentasAprobadas => Ventas.Count(venta => venta.Estado == "Aprobada");

    public int CantidadVentasAnuladas => Ventas.Count(venta => venta.Estado == "Anulada");

    public int CantidadVentasPendientes => Ventas.Count(venta => venta.Estado == "Pendiente");

    public decimal MontoTotalAprobado => Ventas
        .Where(venta => venta.Estado == "Aprobada")
        .Sum(venta => venta.Total);
}
