using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VibraUrbana.Services;
using VibraUrbana.ViewModels;

namespace VibraUrbana.Controllers;

[Authorize(Roles = "Administrador")]
public class RolController : Controller
{
    private readonly IRolServicio _rolServicio;

    public RolController(IRolServicio rolServicio)
    {
        _rolServicio = rolServicio;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var roles = await _rolServicio.ObtenerRolesAsync();
        var model = roles.Select(rol => new RolListadoItemViewModel
        {
            Id = rol.Id,
            Nombre = rol.Nombre,
            Descripcion = rol.Descripcion,
            Activo = rol.Activo
        }).ToList();

        return View(model);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new CrearRolViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CrearRolViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ModelState.AddModelError(string.Empty, "Completa los campos obligatorios para registrar el rol.");
            return View(model);
        }

        var creado = await _rolServicio.AgregarRolAsync(model);

        if (!creado)
        {
            ModelState.AddModelError(string.Empty, "Ya existe un rol con ese nombre.");
            return View(model);
        }

        TempData["SuccessMessage"] = "Rol registrado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarEstado(int id, bool active)
    {
        var actualizado = active
            ? await _rolServicio.ActivarRolAsync(id)
            : await _rolServicio.EliminarRolAsync(id);

        TempData["SuccessMessage"] = actualizado
            ? active ? "Rol activado correctamente." : "Rol desactivado correctamente."
            : "No fue posible cambiar el estado del rol.";

        return RedirectToAction(nameof(Index));
    }
}