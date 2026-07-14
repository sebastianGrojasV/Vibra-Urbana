using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VibraUrbana.Services;
using VibraUrbana.ViewModels;

namespace VibraUrbana.Controllers;

[Authorize(Policy = PermisosSistema.UsuariosGestionar)]
public class AdminController : Controller
{
    private readonly IAdminDashboardServicio _adminDashboardServicio;
    private readonly IUsuarioRegistrationService _usuarioRegistrationService;

    public AdminController(
        IAdminDashboardServicio adminDashboardServicio,
        IUsuarioRegistrationService usuarioRegistrationService)
    {
        _adminDashboardServicio = adminDashboardServicio;
        _usuarioRegistrationService = usuarioRegistrationService;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _adminDashboardServicio.ObtenerDashboardAsync());
    }

    [HttpGet]
    public async Task<IActionResult> Usuarios()
    {
        return View(await _usuarioRegistrationService.GetUsuariosAsync());
    }

    [HttpGet]
    public async Task<IActionResult> VerUsuario(int id)
    {
        var usuario = await _usuarioRegistrationService.GetUsuarioDetalleAsync(id);

        if (usuario is null)
        {
            return NotFound();
        }

        return View(usuario);
    }

    [HttpGet]
    public async Task<IActionResult> CrearUsuario()
    {
        return View(await BuildCrearUsuarioViewModelAsync());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CrearUsuario(CrearUsuarioViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ModelState.AddModelError(string.Empty, "Completa los campos obligatorios para registrar el usuario.");
            model.Roles = await BuildRoleSelectListAsync(model.RolId);
            return View(model);
        }

        var result = await _usuarioRegistrationService.CreateAsync(model);

        if (result == RegistroUsuarioResult.Success)
        {
            TempData["SuccessMessage"] = "Usuario registrado correctamente.";
            return RedirectToAction(nameof(Usuarios));
        }

        AddRegistrationError(result);
        model.Roles = await BuildRoleSelectListAsync(model.RolId);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> ModificarUsuario(int id)
    {
        var model = await _usuarioRegistrationService.GetUsuarioParaEditarAsync(id);

        if (model is null)
        {
            return NotFound();
        }

        model.Roles = await BuildRoleSelectListAsync(model.RolId);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ModificarUsuario(EditarUsuarioViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ModelState.AddModelError(string.Empty, "Completa los campos obligatorios para actualizar el usuario.");
            model.Roles = await BuildRoleSelectListAsync(model.RolId);
            return View(model);
        }

        var result = await _usuarioRegistrationService.UpdateAsync(model);

        if (result == ActualizarUsuarioResult.Success)
        {
            TempData["SuccessMessage"] = "Usuario actualizado correctamente.";
            return RedirectToAction(nameof(Usuarios));
        }

        if (result == ActualizarUsuarioResult.NotFound)
        {
            return NotFound();
        }

        AddUpdateError(result);
        model.Roles = await BuildRoleSelectListAsync(model.RolId);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> RestablecerPasswordUsuario(int id)
    {
        var model = await _usuarioRegistrationService.GetUsuarioParaRestablecerPasswordAsync(id);

        if (model is null)
        {
            return NotFound();
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RestablecerPasswordUsuario(RestablecerPasswordUsuarioViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ModelState.AddModelError(string.Empty, "Completa los campos obligatorios para restablecer la contraseña.");
            await RepoblarUsuarioParaRestablecerAsync(model);
            return View(model);
        }

        var result = await _usuarioRegistrationService.ResetPasswordAsync(model);

        if (result == RestablecerPasswordUsuarioResult.NotFound)
        {
            return NotFound();
        }

        TempData["SuccessMessage"] = "Contraseña restablecida correctamente.";
        return RedirectToAction(nameof(VerUsuario), new { id = model.Id });
    }

    [HttpGet]
    public async Task<IActionResult> CambiarEstadoUsuario(int id)
    {
        var usuario = await _usuarioRegistrationService.GetUsuarioDetalleAsync(id);

        if (usuario is null)
        {
            return NotFound();
        }

        return View(usuario);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmarCambiarEstadoUsuario(int id, bool active)
    {
        var result = await _usuarioRegistrationService.ChangeStatusAsync(id, active);

        if (result == CambiarEstadoUsuarioResult.NotFound)
        {
            return NotFound();
        }

        TempData["SuccessMessage"] = active
            ? "Usuario activado correctamente."
            : "Usuario desactivado correctamente.";

        return RedirectToAction(nameof(Usuarios));
    }

    private async Task<CrearUsuarioViewModel> BuildCrearUsuarioViewModelAsync()
    {
        return new CrearUsuarioViewModel
        {
            Roles = await BuildRoleSelectListAsync()
        };
    }

    private async Task<IEnumerable<SelectListItem>> BuildRoleSelectListAsync(int? selectedRolId = null)
    {
        var roles = await _usuarioRegistrationService.GetActiveRolesAsync();

        return roles.Select(rol => new SelectListItem
        {
            Value = rol.Id.ToString(),
            Text = rol.Nombre,
            Selected = selectedRolId == rol.Id
        });
    }

    private void AddRegistrationError(RegistroUsuarioResult result)
    {
        var message = result switch
        {
            RegistroUsuarioResult.DuplicateCedula => "La cédula ya se encuentra registrada.",
            RegistroUsuarioResult.DuplicateCorreo => "El correo electrónico ya se encuentra registrado.",
            RegistroUsuarioResult.RolNotFound => "El rol seleccionado no está disponible.",
            _ => "No fue posible registrar el usuario."
        };

        ModelState.AddModelError(string.Empty, message);
    }

    private void AddUpdateError(ActualizarUsuarioResult result)
    {
        var message = result switch
        {
            ActualizarUsuarioResult.DuplicateCedula => "La cédula ya se encuentra registrada por otro usuario.",
            ActualizarUsuarioResult.DuplicateCorreo => "El correo electrónico ya se encuentra registrado por otro usuario.",
            ActualizarUsuarioResult.RolNotFound => "El rol seleccionado no está disponible.",
            _ => "No fue posible actualizar el usuario."
        };

        ModelState.AddModelError(string.Empty, message);
    }

    private async Task RepoblarUsuarioParaRestablecerAsync(RestablecerPasswordUsuarioViewModel model)
    {
        var persistedModel = await _usuarioRegistrationService.GetUsuarioParaRestablecerPasswordAsync(model.Id);

        if (persistedModel is null)
        {
            return;
        }

        model.NombreCompleto = persistedModel.NombreCompleto;
        model.Correo = persistedModel.Correo;
    }
}
