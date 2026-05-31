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
            {
                // Guardamos mensaje de confirmación en TempData
                TempData["RolCreado"] = " El rol se creó exitosamente";
                return RedirectToAction(nameof(Index));
            }

            // Si ya existe, mostramos error en la vista
            ModelState.AddModelError(string.Empty, "Ya existe un rol con ese nombre");
        }
        return View(rol);
    }




    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var rol = await _rolServicio.ObtenerRolPorIdAsync(id);
        if (rol == null)
            return NotFound();

        return View(rol); // Vista Delete.cshtml con los datos del rol
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var eliminado = await _rolServicio.EliminarRolAsync(id);

        if (eliminado)
        {
            // Mensaje de confirmación para el Index
            TempData["RolEliminado"] = " El rol fue desactivado correctamente";
            return RedirectToAction(nameof(Index));
        }

        // Si no se pudo eliminar (no existe o ya estaba inactivo)
        TempData["RolError"] = " No se pudo desactivar el rol";
        return RedirectToAction(nameof(Index));
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Activar(int id)
    {
        var activado = await _rolServicio.ActivarRolAsync(id);

        if (activado)
        {
            TempData["RolActivado"] = " El rol fue activado correctamente";
        }
        else
        {
            TempData["RolError"] = " No se pudo activar el rol";
        }

        return RedirectToAction(nameof(Index));
    }





    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
