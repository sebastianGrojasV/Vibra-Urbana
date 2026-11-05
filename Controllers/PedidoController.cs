using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QRCoder;
using VibraUrbana.Services;
using VibraUrbana.ViewModels;

namespace VibraUrbana.Controllers;

[Authorize(Policy = PermisosSistema.VentasGestionar)]
public class PedidoController : Controller
{
    private readonly IPedidoServicio _pedidoServicio;

    public PedidoController(IPedidoServicio pedidoServicio)
    {
        _pedidoServicio = pedidoServicio;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? estado, DateTime? fechaInicio, DateTime? fechaFin, string? cliente)
    {
        var model = await _pedidoServicio.ObtenerPedidosAsync(estado, fechaInicio, fechaFin, cliente);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Detalle(int id)
    {
        var model = await _pedidoServicio.ObtenerDetalleAsync(id);

        if (model is null)
        {
            return NotFound();
        }

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> QrEntrega(int id)
    {
        var model = await _pedidoServicio.ObtenerDetalleAsync(id);

        if (model is null || string.IsNullOrWhiteSpace(model.TokenEntrega))
        {
            return NotFound();
        }

        var seguimientoUrl = Url.Action(
            nameof(Seguimiento),
            "Pedido",
            new { token = model.TokenEntrega },
            Request.Scheme) ?? string.Empty;

        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(seguimientoUrl, QRCodeGenerator.ECCLevel.Q);
        var qr = new PngByteQRCode(data);
        var bytes = qr.GetGraphic(18);

        return File(bytes, "image/png");
    }

    [HttpGet]
    public async Task<IActionResult> EtiquetaEntrega(int id)
    {
        var model = await _pedidoServicio.ObtenerDetalleAsync(id);

        if (model is null || string.IsNullOrWhiteSpace(model.TokenEntrega))
        {
            return NotFound();
        }

        return View(model);
    }

    [AllowAnonymous]
    [HttpGet("Pedido/Seguimiento/{token}")]
    public async Task<IActionResult> Seguimiento(string token)
    {
        var model = await _pedidoServicio.ObtenerSeguimientoAsync(token);

        if (model is null)
        {
            return NotFound();
        }

        return View(model);
    }

    [AllowAnonymous]
    [HttpPost("Pedido/ConfirmarEntrega/{token}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmarEntrega(string token)
    {
        var result = await _pedidoServicio.ConfirmarEntregaPorTokenAsync(token);

        if (result.EsExitoso)
        {
            TempData["SuccessMessage"] = result.Mensaje;
        }
        else
        {
            TempData["ErrorMessage"] = result.Mensaje;
        }

        return RedirectToAction(nameof(Seguimiento), new { token });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarEstado(CambiarEstadoPedidoViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "No se pudo cambiar el estado del pedido.";
            return RedirectToAction(nameof(Index));
        }

        if (!TryGetUsuarioId(out var usuarioId))
        {
            return Unauthorized();
        }

        var result = await _pedidoServicio.CambiarEstadoAsync(model.PedidoId, model.NuevoEstado, usuarioId, model.Observacion);

        if (result.EsExitoso)
        {
            TempData["SuccessMessage"] = result.Mensaje;
        }
        else
        {
            TempData["ErrorMessage"] = result.Mensaje;
        }

        return RedirectToAction(nameof(Index));
    }

    private bool TryGetUsuarioId(out int usuarioId)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(userIdString, out usuarioId);
    }
}
