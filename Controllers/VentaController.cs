using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VibraUrbana.Data;
using VibraUrbana.Services;
using VibraUrbana.ViewModels;

namespace VibraUrbana.Controllers;

[Authorize(Policy = PermisosSistema.VentasGestionar)]
public class VentaController : Controller
{
    private readonly IVentaServicio _ventaServicio;
    private readonly IClienteServicio _clienteServicio;
    private readonly IProductoServicio _productoServicio;
    private readonly ApplicationDbContext _context;

    public VentaController(
        IVentaServicio ventaServicio,
        IClienteServicio clienteServicio,
        IProductoServicio productoServicio,
        ApplicationDbContext context)
    {
        _ventaServicio = ventaServicio;
        _clienteServicio = clienteServicio;
        _productoServicio = productoServicio;
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int? clienteId)
    {
        var ventas = await _ventaServicio.ObtenerVentasAsync(clienteId);

        if (clienteId.HasValue)
        {
            var cliente = await _context.Clientes.FindAsync(clienteId.Value);
            if (cliente != null)
            {
                ViewBag.ClienteFiltrado = cliente.NombreCompleto;
                ViewBag.ClienteIdentificacion = cliente.Identificacion;
            }
            ViewBag.ClienteId = clienteId.Value;
        }

        return View(ventas);
    }

    [HttpGet]
    public async Task<IActionResult> Crear()
    {
        var clientes = await _clienteServicio.ObtenerClientesAsync(null);
        var productosResult = await _productoServicio.ObtenerProductosAsync(null, null, null, true);
        var metodosPago = await _context.MetodosPago.Where(m => m.Activo).ToListAsync();

        var model = new CrearVentaViewModel
        {
            Clientes = clientes
                .Where(c => c.Activo && c.Identificacion != "000000000")
                .ToList(),
            Productos = productosResult.Productos.Where(p => p.Activo && p.CantidadDisponible > 0).ToList(),
            MetodosPago = metodosPago
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear([FromBody] RegistrarVentaViewModel model)
    {
        if (model == null)
        {
            return BadRequest(VentaOperacionResult.Failure("Los datos de la venta no son válidos."));
        }

        if (!ModelState.IsValid)
        {
            var errors = string.Join(" | ", ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage));
            return BadRequest(VentaOperacionResult.Failure($"Datos inválidos: {errors}"));
        }

        // Obtener ID del usuario autenticado
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var usuarioId))
        {
            return Unauthorized(VentaOperacionResult.Failure("El usuario no está autenticado o su sesión ha expirado."));
        }

        var result = await _ventaServicio.RegistrarVentaAsync(model, usuarioId);

        if (result.EsExitoso)
        {
            TempData["SuccessMessage"] = "Venta registrada correctamente.";
            return Ok(result);
        }

        return BadRequest(result);
    }
}
