using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VibraUrbana.Services;

namespace VibraUrbana.Controllers;

[Authorize(Policy = PermisosSistema.FacturacionGestionar)]
public class FacturaController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
