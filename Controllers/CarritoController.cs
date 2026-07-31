using Microsoft.AspNetCore.Mvc;
using VibraUrbana.Services;
using VibraUrbana.ViewModels;

namespace VibraUrbana.Controllers;

public class CarritoController : Controller
{
    private readonly IPedidoServicio _pedidoServicio;

    public CarritoController(IPedidoServicio pedidoServicio)
    {
        _pedidoServicio = pedidoServicio;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Pedido()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegistrarPedido([FromBody] RegistrarPedidoEnLineaViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(RegistrarPedidoEnLineaResult.Failure("Completa los datos obligatorios del pedido."));
        }

        var result = await _pedidoServicio.RegistrarPedidoEnLineaAsync(model);

        if (!result.EsExitoso)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpGet]
    public IActionResult Confirmacion()
    {
        return View();
    }
}
