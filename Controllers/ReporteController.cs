using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VibraUrbana.Services;
using VibraUrbana.ViewModels;

namespace VibraUrbana.Controllers;

[Authorize(Policy = PermisosSistema.ReportesVer)]
public class ReporteController : Controller
{
    private readonly IVentaServicio _ventaServicio;

    public ReporteController(IVentaServicio ventaServicio)
    {
        _ventaServicio = ventaServicio;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string filtro = "dia", DateTime? fechaInicio = null, DateTime? fechaFin = null)
    {
        var model = new ReporteVentasViewModel
        {
            Filtro = filtro,
            FechaInicio = fechaInicio,
            FechaFin = fechaFin
        };

        DateTime localStart;
        DateTime localEnd;
        var hoy = DateTime.Today;

        switch (filtro.ToLower())
        {
            case "semana":
                int diff = (7 + (hoy.DayOfWeek - DayOfWeek.Monday)) % 7;
                localStart = hoy.AddDays(-1 * diff);
                localEnd = localStart.AddDays(7).AddTicks(-1); // Domingo 23:59:59.999
                break;
            case "mes":
                localStart = new DateTime(hoy.Year, hoy.Month, 1);
                localEnd = localStart.AddMonths(1).AddTicks(-1);
                break;
            case "personalizado":
                localStart = fechaInicio ?? hoy;
                localEnd = (fechaFin ?? hoy).Date.AddDays(1).AddTicks(-1);
                break;
            case "dia":
            default:
                localStart = hoy;
                localEnd = hoy.Date.AddDays(1).AddTicks(-1);
                break;
        }

        // Guardar las fechas locales calculadas para mostrarlas en los inputs
        model.FechaInicio = localStart;
        model.FechaFin = localEnd;

        // Convertir a UTC (Costa Rica UTC-6, por lo que la hora UTC está 6 horas adelante)
        var costaRicaOffset = TimeSpan.FromHours(-6);
        DateTime utcStart = new DateTimeOffset(localStart, costaRicaOffset).UtcDateTime;
        DateTime utcEnd = new DateTimeOffset(localEnd, costaRicaOffset).UtcDateTime;

        var ventas = await _ventaServicio.ObtenerVentasPorRangoFechaAsync(utcStart, utcEnd);

        model.Ventas = ventas;
        model.TotalVentas = ventas.Count;

        var aprobadas = ventas.Where(v => v.Estado != "Anulada").ToList();
        model.VentasAprobadas = aprobadas.Count;
        model.MontoTotal = aprobadas.Sum(v => v.Total);
        model.TotalDescuentos = aprobadas.Sum(v => v.Descuento);

        model.VentasAnuladas = ventas.Count(v => v.Estado == "Anulada");

        // Inicializar métodos de pago comunes con 0
        var metodosComunes = new[] { "Efectivo", "SINPE", "Tarjeta" };
        foreach (var m in metodosComunes)
        {
            model.MontoPorMetodoPago[m] = 0;
            model.CantidadPorMetodoPago[m] = 0;
        }

        // Agrupar y sobreescribir con valores reales
        foreach (var g in aprobadas.GroupBy(v => v.MetodoPago.Nombre))
        {
            model.MontoPorMetodoPago[g.Key] = g.Sum(v => v.Total);
            model.CantidadPorMetodoPago[g.Key] = g.Count();
        }

        return View(model);
    }
}
