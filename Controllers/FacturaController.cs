using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VibraUrbana.Data;
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
        var fechaSiguiente = fechaSeleccionada.AddDays(1);

        var ventas = await _context.Ventas
            .AsNoTracking()
            .Include(venta => venta.Cliente)
            .Include(venta => venta.Usuario)
            .Include(venta => venta.MetodoPago)
            .Include(venta => venta.Factura)
            .Where(venta => venta.FechaVenta >= fechaSeleccionada && venta.FechaVenta < fechaSiguiente)
            .Where(venta => venta.Estado == "Aprobada")
            .OrderByDescending(venta => venta.FechaVenta)
            .ToListAsync();

        var totalesPorMetodo = ventas
            .GroupBy(venta => venta.MetodoPago.Nombre)
            .OrderBy(grupo => grupo.Key)
            .Select(grupo => new CierreCajaMetodoPagoViewModel
            {
                MetodoPago = grupo.Key,
                CantidadVentas = grupo.Count(),
                Total = grupo.Sum(venta => venta.Total)
            })
            .ToList();

        var model = new CierreCajaViewModel
        {
            Fecha = fechaSeleccionada,
            CantidadVentas = ventas.Count,
            TotalVentas = ventas.Sum(venta => venta.Total),
            TotalEfectivo = ventas.Where(venta => venta.MetodoPago.Nombre == "Efectivo").Sum(venta => venta.Total),
            TotalSinpe = ventas.Where(venta => venta.MetodoPago.Nombre == "SINPE").Sum(venta => venta.Total),
            TotalTarjeta = ventas.Where(venta => venta.MetodoPago.Nombre == "Tarjeta").Sum(venta => venta.Total),
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
                Estado = venta.Estado
            }).ToList()
        };

        return View(model);
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
}
