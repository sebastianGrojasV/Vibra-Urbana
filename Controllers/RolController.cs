using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using VibraUrbana.Models;
using VibraUrbana.Services;

namespace VibraUrbana.Controllers;

public class RolController : Controller
{
    private readonly IRolServicio _rolServicio;

    public RolController(IRolServicio rolServicio)
    {
        _rolServicio = rolServicio;
    }

    // GET: Rol
    public async Task<IActionResult> Index()
    {
        var roles = await _rolServicio.ObtenerRolesAsync();
        return View(roles); // Vista Index.cshtml con la lista de roles
    }

    // GET: Rol/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var rol = await _rolServicio.ObtenerRolPorIdAsync(id);
        if (rol == null)
            return NotFound();

        return View(rol); // Vista Details.cshtml
    }

    // GET: Rol/Create
    public IActionResult Create()
    {
        return View(); // Vista Create.cshtml con formulario
    }

    // POST: Rol/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Rol rol)
    {
        if (ModelState.IsValid)
        {
            var creado = await _rolServicio.AgregarRolAsync(rol);
            if (creado)
                return RedirectToAction(nameof(Index));

            ModelState.AddModelError(string.Empty, "Ya existe un rol con ese nombre");
        }
        return View(rol);
    }


    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
