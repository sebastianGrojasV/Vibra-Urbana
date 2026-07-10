using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using VibraUrbana.Data;
using VibraUrbana.Models;
using VibraUrbana.Services;
using VibraUrbana.ViewModels;

namespace VibraUrbana.Controllers;

[Authorize(Policy = PermisosSistema.FacturacionGestionar)]
public class FacturaController : Controller
{
    private readonly ApplicationDbContext _context;

    public FacturaController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var facturas = await _context.Facturas
            .AsNoTracking()
            .Include(factura => factura.Venta)
                .ThenInclude(venta => venta.Cliente)
            .Include(factura => factura.Venta)
                .ThenInclude(venta => venta.Usuario)
            .Include(factura => factura.Venta)
                .ThenInclude(venta => venta.MetodoPago)
            .OrderByDescending(factura => factura.FechaEmision)
            .Select(factura => new FacturaListadoItemViewModel
            {
                Id = factura.Id,
                VentaId = factura.VentaId,
                NumeroFactura = factura.NumeroFactura,
                FechaEmision = factura.FechaEmision,
                Cliente = factura.Venta.Cliente.NombreCompleto,
                IdentificacionCliente = factura.Venta.Cliente.Identificacion,
                Cajero = factura.Venta.Usuario.NombreCompleto,
                MetodoPago = factura.Venta.MetodoPago.Nombre,
                Total = factura.Total,
                Estado = factura.Estado
            })
            .ToListAsync();

        return View(facturas);
    }

    public async Task<IActionResult> Detalle(int id)
    {
        var model = await ObtenerFacturaDetalleAsync(id);

        if (model is null)
        {
            return NotFound();
        }

        return View(model);
    }

    public async Task<IActionResult> Comprobante(int id)
    {
        var model = await ObtenerFacturaDetalleAsync(id);

        if (model is null)
        {
            return NotFound();
        }

        return View(model);
    }

    public async Task<IActionResult> CierreCaja(DateTime? fecha)
    {
        var fechaSeleccionada = fecha?.Date ?? DateTime.Today;
        var model = await ConstruirCierreCajaViewModelAsync(fechaSeleccionada);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegistrarCierreCaja(DateTime fecha)
    {
        if (!TryGetUsuarioId(out var usuarioId))
        {
            return Unauthorized();
        }

        var fechaSeleccionada = fecha.Date;
        var fechaSiguiente = fechaSeleccionada.AddDays(1);

        await using var transaction = await _context.Database.BeginTransactionAsync();

        var ventasParaCierre = await _context.Ventas
            .Include(venta => venta.MetodoPago)
            .Where(venta => venta.FechaVenta >= fechaSeleccionada && venta.FechaVenta < fechaSiguiente)
            .Where(venta => venta.CierreCajaId == null)
            .Where(venta => venta.Estado == "Aprobada" || venta.Estado == "Anulada")
            .ToListAsync();

        if (!ventasParaCierre.Any())
        {
            TempData["ErrorMessage"] = "No hay ventas aprobadas o anuladas pendientes para cerrar en la fecha seleccionada.";
            return RedirectToAction(nameof(CierreCaja), new { fecha = fechaSeleccionada.ToString("yyyy-MM-dd") });
        }

        var ventasAprobadas = ventasParaCierre
            .Where(venta => venta.Estado == "Aprobada")
            .ToList();
        var cantidadAnuladas = ventasParaCierre.Count(venta => venta.Estado == "Anulada");
        var totalEfectivo = ventasAprobadas.Where(venta => venta.MetodoPago.Nombre == "Efectivo").Sum(venta => venta.Total);
        var totalSinpe = ventasAprobadas.Where(venta => venta.MetodoPago.Nombre == "SINPE").Sum(venta => venta.Total);
        var totalTarjeta = ventasAprobadas.Where(venta => venta.MetodoPago.Nombre == "Tarjeta").Sum(venta => venta.Total);

        var cierre = new CierreCaja
        {
            UsuarioId = usuarioId,
            FechaCierre = DateTime.UtcNow,
            TotalVentas = ventasAprobadas.Sum(venta => venta.Total),
            TotalEfectivo = totalEfectivo,
            TotalSinpe = totalSinpe,
            TotalTarjeta = totalTarjeta,
            CantidadVentas = ventasParaCierre.Count,
            Observacion = $"Cierre del {fechaSeleccionada:dd/MM/yyyy}. Aprobadas: {ventasAprobadas.Count}. Anuladas: {cantidadAnuladas}."
        };

        _context.CierresCaja.Add(cierre);
        await _context.SaveChangesAsync();

        foreach (var venta in ventasParaCierre)
        {
            venta.CierreCajaId = cierre.Id;
        }

        _context.Bitacora.Add(new Bitacora
        {
            UsuarioId = usuarioId,
            Modulo = "Caja",
            Accion = "Cierre de caja",
            Descripcion = $"Cierre #{cierre.Id} registrado para {fechaSeleccionada:dd/MM/yyyy}. Ventas: {ventasParaCierre.Count}. Total: {cierre.TotalVentas:C0}.",
            FechaRegistro = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        TempData["SuccessMessage"] = $"Cierre de caja registrado correctamente. Total: {cierre.TotalVentas:C0}.";

        return RedirectToAction(nameof(CierreCaja), new { fecha = fechaSeleccionada.ToString("yyyy-MM-dd") });
    }

    private async Task<CierreCajaViewModel> ConstruirCierreCajaViewModelAsync(DateTime fechaSeleccionada)
    {
        var fechaSiguiente = fechaSeleccionada.AddDays(1);

        var ventas = await _context.Ventas
            .AsNoTracking()
            .Include(venta => venta.Cliente)
            .Include(venta => venta.Usuario)
            .Include(venta => venta.MetodoPago)
            .Include(venta => venta.Factura)
            .Where(venta => venta.FechaVenta >= fechaSeleccionada && venta.FechaVenta < fechaSiguiente)
            .OrderByDescending(venta => venta.FechaVenta)
            .ToListAsync();

        var ventasAprobadas = ventas.Where(venta => venta.Estado == "Aprobada").ToList();
        var ventasCerrables = ventas
            .Where(venta => venta.CierreCajaId == null)
            .Where(venta => venta.Estado == "Aprobada" || venta.Estado == "Anulada")
            .ToList();

        var totalesPorMetodo = ventasAprobadas
            .GroupBy(venta => venta.MetodoPago.Nombre)
            .OrderBy(grupo => grupo.Key)
            .Select(grupo => new CierreCajaMetodoPagoViewModel
            {
                MetodoPago = grupo.Key,
                CantidadVentas = grupo.Count(),
                Total = grupo.Sum(venta => venta.Total)
            })
            .ToList();

        return new CierreCajaViewModel
        {
            Fecha = fechaSeleccionada,
            CantidadVentas = ventas.Count,
            CantidadVentasAprobadas = ventasAprobadas.Count,
            CantidadVentasAnuladas = ventas.Count(venta => venta.Estado == "Anulada"),
            CantidadVentasPendientes = ventas.Count(venta => venta.Estado == "Pendiente"),
            VentasDisponiblesParaCierre = ventasCerrables.Count,
            TotalVentas = ventasAprobadas.Sum(venta => venta.Total),
            TotalEfectivo = ventasAprobadas.Where(venta => venta.MetodoPago.Nombre == "Efectivo").Sum(venta => venta.Total),
            TotalSinpe = ventasAprobadas.Where(venta => venta.MetodoPago.Nombre == "SINPE").Sum(venta => venta.Total),
            TotalTarjeta = ventasAprobadas.Where(venta => venta.MetodoPago.Nombre == "Tarjeta").Sum(venta => venta.Total),
            TotalesPorMetodoPago = totalesPorMetodo,
            Ventas = ventas.Select(venta => new CierreCajaVentaItemViewModel
            {
                VentaId = venta.Id,
                NumeroFactura = venta.Factura?.NumeroFactura ?? "Sin comprobante",
                FechaVenta = venta.FechaVenta,
                Cliente = venta.Cliente.NombreCompleto,
                Cajero = venta.Usuario.NombreCompleto,
                MetodoPago = venta.MetodoPago.Nombre,
                Total = venta.Total,
                Estado = venta.Estado,
                IncluidaEnCierre = venta.CierreCajaId.HasValue
            }).ToList()
        };
    }

    private async Task<FacturaDetalleViewModel?> ObtenerFacturaDetalleAsync(int id)
    {
        return await _context.Facturas
            .AsNoTracking()
            .Include(factura => factura.Venta)
                .ThenInclude(venta => venta.Cliente)
            .Include(factura => factura.Venta)
                .ThenInclude(venta => venta.Usuario)
            .Include(factura => factura.Venta)
                .ThenInclude(venta => venta.MetodoPago)
            .Include(factura => factura.Venta)
                .ThenInclude(venta => venta.DetalleVentas)
                    .ThenInclude(detalle => detalle.Producto)
                        .ThenInclude(producto => producto.Categoria)
            .Where(factura => factura.Id == id)
            .Select(factura => new FacturaDetalleViewModel
            {
                Id = factura.Id,
                VentaId = factura.VentaId,
                NumeroFactura = factura.NumeroFactura,
                FechaEmision = factura.FechaEmision,
                FechaVenta = factura.Venta.FechaVenta,
                EstadoFactura = factura.Estado,
                EstadoVenta = factura.Venta.Estado,
                Cliente = factura.Venta.Cliente.NombreCompleto,
                IdentificacionCliente = factura.Venta.Cliente.Identificacion,
                TelefonoCliente = factura.Venta.Cliente.Telefono,
                CorreoCliente = factura.Venta.Cliente.Correo,
                Cajero = factura.Venta.Usuario.NombreCompleto,
                MetodoPago = factura.Venta.MetodoPago.Nombre,
                Observacion = factura.Venta.Observacion,
                Subtotal = factura.Subtotal,
                Descuento = factura.Descuento,
                Impuesto = factura.Impuesto,
                Total = factura.Total,
                Productos = factura.Venta.DetalleVentas
                    .OrderBy(detalle => detalle.Producto.Nombre)
                    .Select(detalle => new FacturaDetalleItemViewModel
                    {
                        Producto = detalle.Producto.Nombre,
                        Categoria = detalle.Producto.Categoria.Nombre,
                        Talla = detalle.Producto.Talla,
                        Color = detalle.Producto.Color,
                        Cantidad = detalle.Cantidad,
                        PrecioUnitario = detalle.PrecioUnitario,
                        Subtotal = detalle.Subtotal
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();
    }

    private bool TryGetUsuarioId(out int usuarioId)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(userIdString, out usuarioId);
    }
}
