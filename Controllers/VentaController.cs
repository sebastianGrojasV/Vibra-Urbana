using System.Security.Claims;
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

    [HttpGet]
    public async Task<IActionResult> Detalle(int id)
    {
        var venta = await _context.Ventas
            .AsNoTracking()
            .Include(item => item.Cliente)
            .Include(item => item.Usuario)
            .Include(item => item.MetodoPago)
            .Include(item => item.Factura)
            .Include(item => item.DetalleVentas)
                .ThenInclude(detalle => detalle.Producto)
                    .ThenInclude(producto => producto.Categoria)
            .SingleOrDefaultAsync(item => item.Id == id);

        if (venta is null)
        {
            return NotFound();
        }

        return View(venta);
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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarEstado(CambiarEstadoVentaViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Debes indicar un estado y motivo válidos.";
            return RedirectToAction(nameof(Index));
        }

        if (!TryGetUsuarioId(out var usuarioId))
        {
            return Unauthorized();
        }

        var result = await _ventaServicio.CambiarEstadoVentaAsync(
            model.VentaId,
            model.NuevoEstado,
            model.Motivo,
            usuarioId);

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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Anular(AnularVentaViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Debes indicar un motivo de anulación válido.";
            return RedirectToAction(nameof(Index));
        }

        if (!TryGetUsuarioId(out var usuarioId))
        {
            return Unauthorized();
        }

        var result = await _ventaServicio.AnularVentaAsync(model.VentaId, model.Motivo, usuarioId);

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
