using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VibraUrbana.Services;

namespace VibraUrbana.Controllers;

[Authorize(Policy = PermisosSistema.ReportesVer)]
public class ReporteController : Controller
{
    private readonly IReporteServicio _reporteServicio;

    public ReporteController(IReporteServicio reporteServicio)
    {
        _reporteServicio = reporteServicio;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int? categoriaId, string? estadoStock)
    {
        return View(await _reporteServicio.ObtenerInventarioAsync(categoriaId, estadoStock));
    }
}
