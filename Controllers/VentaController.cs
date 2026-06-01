using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VibraUrbana.Services;

namespace VibraUrbana.Controllers;

[Authorize(Policy = PermisosSistema.VentasGestionar)]
public class VentaController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
