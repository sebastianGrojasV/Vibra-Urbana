using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace VibraUrbana.Controllers;

[Authorize(Roles = "Administrador,Inventario")]
public class InventarioController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
