using Microsoft.AspNetCore.Mvc;
using VibraUrbana.Services;
using VibraUrbana.ViewModels;

namespace VibraUrbana.Controllers;

public class CarritoController : Controller
{
    private readonly IPedidoServicio _pedidoServicio;
    private readonly IClienteServicio _clienteServicio;

    public CarritoController(
        IPedidoServicio pedidoServicio,
        IClienteServicio clienteServicio)
    {
        _pedidoServicio = pedidoServicio;
        _clienteServicio = clienteServicio;
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

    [HttpGet]
    public async Task<IActionResult> BuscarClientePorCedula(string cedula)
    {
        var cedulaNormalizada = cedula?.Trim() ?? string.Empty;

        if (!System.Text.RegularExpressions.Regex.IsMatch(cedulaNormalizada, "^[0-9]{9}$"))
        {
            return BadRequest(new { encontrado = false, mensaje = "Ingresa una cédula válida de 9 dígitos." });
        }

        var cliente = await _clienteServicio.ObtenerClientePorIdentificacionAsync(cedulaNormalizada);

        if (cliente is null)
        {
            return Ok(new { encontrado = false });
        }

        return Ok(new { encontrado = true, cliente });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegistrarPedido([FromBody] RegistrarPedidoEnLineaViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var errores = ModelState.Values
                .SelectMany(value => value.Errors)
                .Select(error => error.ErrorMessage)
                .Where(error => !string.IsNullOrWhiteSpace(error))
                .Distinct();

            return BadRequest(RegistrarPedidoEnLineaResult.Failure(string.Join(" ", errores)));
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
