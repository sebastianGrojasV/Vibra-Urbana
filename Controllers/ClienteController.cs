using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VibraUrbana.Services;

namespace VibraUrbana.Controllers;

[Authorize(Policy = PermisosSistema.ClientesGestionar)]
public class ClienteController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
