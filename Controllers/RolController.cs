using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VibraUrbana.Services;
using VibraUrbana.ViewModels;

namespace VibraUrbana.Controllers;

[Authorize(Policy = PermisosSistema.RolesGestionar)]
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

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var model = await _rolServicio.ObtenerRolParaEditarAsync(id);

        if (model is null)
        {
            return NotFound();
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditarRolViewModel model)
    {
        if (!model.PermisosSeleccionados.Any())
        {
            ModelState.AddModelError(string.Empty, "Selecciona al menos un permiso para el rol.");
        }

        if (!ModelState.IsValid)
        {
            ModelState.AddModelError(string.Empty, "Completa los campos obligatorios para actualizar el rol.");
            await RepoblarPermisosAsync(model);
            return View(model);
        }

        var result = await _rolServicio.ActualizarRolAsync(model);

        if (result == ActualizarRolResult.Success)
        {
            TempData["SuccessMessage"] = "Rol actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        if (result == ActualizarRolResult.NotFound)
        {
            return NotFound();
        }

        AddUpdateError(result);
        await RepoblarPermisosAsync(model);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> CambiarEstado(int id)
    {
        var model = await _rolServicio.ObtenerRolParaEditarAsync(id);

        if (model is null)
        {
            return NotFound();
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmarCambiarEstado(int id, bool active)
    {
        var result = await _rolServicio.CambiarEstadoAsync(id, active);

        if (result == CambiarEstadoRolResult.NotFound)
        {
            return NotFound();
        }

        if (result == CambiarEstadoRolResult.LastActiveAdministrator)
        {
            TempData["SuccessMessage"] = "No se puede dejar el sistema sin al menos un rol Administrador activo.";
            return RedirectToAction(nameof(Index));
        }

        TempData["SuccessMessage"] = active
            ? "Rol activado correctamente."
            : "Rol desactivado correctamente.";

        return RedirectToAction(nameof(Index));
    }

    private async Task RepoblarPermisosAsync(EditarRolViewModel model)
    {
        var persistedModel = await _rolServicio.ObtenerRolParaEditarAsync(model.Id);

        if (persistedModel is null)
        {
            return;
        }

        var permisosSeleccionados = model.PermisosSeleccionados.ToHashSet();
        model.Permisos = persistedModel.Permisos.Select(permiso => new PermisoSeleccionViewModel
        {
            Id = permiso.Id,
            Nombre = permiso.Nombre,
            Descripcion = permiso.Descripcion,
            Seleccionado = permisosSeleccionados.Contains(permiso.Id)
        }).ToList();
    }

    private void AddUpdateError(ActualizarRolResult result)
    {
        var message = result switch
        {
            ActualizarRolResult.DuplicateName => "Ya existe un rol con ese nombre.",
            ActualizarRolResult.LastActiveAdministrator => "No se puede dejar el sistema sin al menos un rol Administrador activo.",
            ActualizarRolResult.InvalidPermissions => "Uno o más permisos seleccionados no están disponibles.",
            _ => "No fue posible actualizar el rol."
        };

        ModelState.AddModelError(string.Empty, message);
    }
}
