using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VibraUrbana.Services;

namespace VibraUrbana.Controllers;

[Authorize(Policy = PermisosSistema.ReportesVer)]
public class ReporteController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
