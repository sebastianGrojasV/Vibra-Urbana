using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VibraUrbana.Services;

namespace VibraUrbana.Controllers;

[Authorize(Policy = PermisosSistema.ProductosGestionar)]
public class ProductoController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
