using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VibraUrbana.Services;
using VibraUrbana.ViewModels;

namespace VibraUrbana.Controllers;

[Authorize(Policy = PermisosSistema.ClientesGestionar)]
public class ClienteController : Controller
{
    private readonly IClienteServicio _clienteServicio;

    public ClienteController(IClienteServicio clienteServicio)
    {
        _clienteServicio = clienteServicio;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? busqueda)
    {
        var model = new ClientesIndexViewModel
        {
            Busqueda = busqueda,
            Clientes = await _clienteServicio.ObtenerClientesAsync(busqueda)
        };

        return View(model);
    }

    [HttpGet]
    public IActionResult Crear()
    {
        return View(new CrearClienteViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(CrearClienteViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ModelState.AddModelError(string.Empty, "Completa los campos obligatorios para registrar el cliente.");
            return View(model);
        }

        var result = await _clienteServicio.CrearAsync(model);

        if (result == CrearClienteResult.Success)
        {
            TempData["SuccessMessage"] = "Cliente registrado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError(string.Empty, "La identificación ya se encuentra registrada.");
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        var model = await _clienteServicio.ObtenerClienteParaEditarAsync(id);

        if (model is null)
        {
            return NotFound();
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(EditarClienteViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ModelState.AddModelError(string.Empty, "Completa los campos obligatorios para actualizar el cliente.");
            return View(model);
        }

        var result = await _clienteServicio.ActualizarAsync(model);

        if (result == ActualizarClienteResult.Success)
        {
            TempData["SuccessMessage"] = "Cliente actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        if (result == ActualizarClienteResult.NotFound)
        {
            return NotFound();
        }

        ModelState.AddModelError(string.Empty, "La identificación ya se encuentra registrada por otro cliente.");
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmarCambiarEstado(int id, bool active)
    {
        var result = await _clienteServicio.CambiarEstadoAsync(id, active);

        if (result == CambiarEstadoClienteResult.NotFound)
        {
            return NotFound();
        }

        TempData["SuccessMessage"] = active
            ? "Cliente activado correctamente."
            : "Cliente desactivado correctamente.";

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> HistorialCompras(int id)
    {
        var model = await _clienteServicio.ObtenerHistorialComprasAsync(id);

        if (model is null)
        {
            return NotFound();
        }

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> DetalleCompra(int clienteId, int ventaId)
    {
        var model = await _clienteServicio.ObtenerDetalleCompraAsync(clienteId, ventaId);

        if (model is null)
        {
            return NotFound();
        }

        return View(model);
    }
}
