using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VibraUrbana.Data;
using VibraUrbana.Models;
using VibraUrbana.ViewModels;

namespace VibraUrbana.Services;

public class ProductoServicio : IProductoServicio
{
    private readonly ApplicationDbContext _context;

    public ProductoServicio(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ProductosIndexViewModel> ObtenerProductosAsync(int? categoriaId, string? talla, string? color, bool? activo)
    {
        var query = AplicarFiltros(QueryProductos(), categoriaId, talla, color, activo);

        return new ProductosIndexViewModel
        {
            CategoriaId = categoriaId,
            Talla = talla,
            Color = color,
            Activo = activo,
            Productos = await query
                .OrderBy(producto => producto.Nombre)
                .Select(producto => new ProductoListadoItemViewModel
                {
                    Id = producto.Id,
                    Nombre = producto.Nombre,
                    Descripcion = producto.Descripcion,
                    Precio = producto.Precio,
                    Talla = producto.Talla,
                    Color = producto.Color,
                    ImagenUrl = producto.ImagenUrl,
                    Activo = producto.Activo,
                    Categoria = producto.Categoria.Nombre,
                    CantidadDisponible = producto.Inventario == null ? 0 : producto.Inventario.CantidadDisponible,
                    StockMinimo = producto.Inventario == null ? 0 : producto.Inventario.StockMinimo
                })
                .ToListAsync(),
            Categorias = await ObtenerCategoriasSelectAsync(categoriaId),
            Tallas = await ObtenerTallasSelectAsync(talla),
            Colores = await ObtenerColoresSelectAsync(color)
        };
    }

    public async Task<ProductoFormViewModel> PrepararCrearAsync()
    {
        return new ProductoFormViewModel
        {
            Activo = true,
            Categorias = await ObtenerCategoriasSelectAsync()
        };
    }

    public async Task<ProductoFormViewModel?> ObtenerProductoParaEditarAsync(int id)
    {
        var model = await _context.Productos
            .AsNoTracking()
            .Include(producto => producto.Inventario)
            .Where(producto => producto.Id == id)
            .Select(producto => new ProductoFormViewModel
            {
                Id = producto.Id,
                Nombre = producto.Nombre,
                Descripcion = producto.Descripcion,
                Precio = producto.Precio,
                Talla = producto.Talla,
                Color = producto.Color,
                ImagenUrl = producto.ImagenUrl,
                CategoriaId = producto.CategoriaId,
                CantidadDisponible = producto.Inventario == null ? 0 : producto.Inventario.CantidadDisponible,
                StockMinimo = producto.Inventario == null ? 0 : producto.Inventario.StockMinimo,
                Activo = producto.Activo
            })
            .SingleOrDefaultAsync();

        if (model is null)
        {
            return null;
        }

        model.Categorias = await ObtenerCategoriasSelectAsync(model.CategoriaId);
        return model;
    }

    public async Task<ProductoOperacionResult> CrearAsync(ProductoFormViewModel model)
    {
        var validationResult = await ValidarProductoAsync(model);

        if (validationResult != ProductoOperacionResult.Success)
        {
            return validationResult;
        }

        var producto = new Producto
        {
            Nombre = model.Nombre.Trim(),
            Descripcion = model.Descripcion.Trim(),
            Precio = model.Precio!.Value,
            Talla = model.Talla.Trim(),
            Color = model.Color.Trim(),
            ImagenUrl = model.ImagenUrl.Trim(),
            CategoriaId = model.CategoriaId,
            Activo = true,
            FechaRegistro = DateTime.UtcNow,
            Inventario = new Inventario
            {
                CantidadDisponible = model.CantidadDisponible!.Value,
                StockMinimo = model.StockMinimo!.Value,
                FechaActualizacion = DateTime.UtcNow
            }
        };

        _context.Productos.Add(producto);
        await _context.SaveChangesAsync();

        return ProductoOperacionResult.Success;
    }

    public async Task<ProductoOperacionResult> ActualizarAsync(ProductoFormViewModel model)
    {
        var validationResult = await ValidarProductoAsync(model);

        if (validationResult != ProductoOperacionResult.Success)
        {
            return validationResult;
        }

        var producto = await _context.Productos
            .Include(item => item.Inventario)
            .SingleOrDefaultAsync(item => item.Id == model.Id);

        if (producto is null)
        {
            return ProductoOperacionResult.NotFound;
        }

        producto.Nombre = model.Nombre.Trim();
        producto.Descripcion = model.Descripcion.Trim();
        producto.Precio = model.Precio!.Value;
        producto.Talla = model.Talla.Trim();
        producto.Color = model.Color.Trim();
        producto.ImagenUrl = model.ImagenUrl.Trim();
        producto.CategoriaId = model.CategoriaId;
        producto.Activo = model.Activo;

        if (producto.Inventario is null)
        {
            producto.Inventario = new Inventario
            {
                ProductoId = producto.Id,
                CantidadDisponible = model.CantidadDisponible!.Value,
                StockMinimo = model.StockMinimo!.Value,
                FechaActualizacion = DateTime.UtcNow
            };
        }
        else
        {
            producto.Inventario.StockMinimo = model.StockMinimo!.Value;
            producto.Inventario.FechaActualizacion = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return ProductoOperacionResult.Success;
    }

    public async Task<InventarioIndexViewModel> ObtenerInventarioAsync(int? categoriaId, string? talla, string? color, bool? activo)
    {
        var query = AplicarFiltros(QueryProductos(), categoriaId, talla, color, activo);

        return new InventarioIndexViewModel
        {
            CategoriaId = categoriaId,
            Talla = talla,
            Color = color,
            Activo = activo,
            Inventario = await query
                .OrderBy(producto => producto.Nombre)
                .Select(producto => new InventarioListadoItemViewModel
                {
                    ProductoId = producto.Id,
                    Producto = producto.Nombre,
                    Categoria = producto.Categoria.Nombre,
                    Talla = producto.Talla,
                    Color = producto.Color,
                    Activo = producto.Activo,
                    CantidadDisponible = producto.Inventario == null ? 0 : producto.Inventario.CantidadDisponible,
                    StockMinimo = producto.Inventario == null ? 0 : producto.Inventario.StockMinimo,
                    FechaActualizacion = producto.Inventario == null ? producto.FechaRegistro : producto.Inventario.FechaActualizacion
                })
                .ToListAsync(),
            Categorias = await ObtenerCategoriasSelectAsync(categoriaId),
            Tallas = await ObtenerTallasSelectAsync(talla),
            Colores = await ObtenerColoresSelectAsync(color)
        };
    }

    public async Task<AjustarInventarioViewModel?> ObtenerAjusteInventarioAsync(int productoId)
    {
        var producto = await _context.Productos
            .AsNoTracking()
            .Include(item => item.Categoria)
            .Include(item => item.Inventario)
            .SingleOrDefaultAsync(item => item.Id == productoId);

        if (producto?.Inventario is null)
        {
            return null;
        }

        return new AjustarInventarioViewModel
        {
            ProductoId = producto.Id,
            Producto = producto.Nombre,
            Categoria = producto.Categoria.Nombre,
            Talla = producto.Talla,
            Color = producto.Color,
            CantidadAnterior = producto.Inventario.CantidadDisponible,
            NuevaCantidad = producto.Inventario.CantidadDisponible,
            Version = Convert.ToBase64String(producto.Inventario.Version)
        };
    }

    public async Task<AjustarInventarioResult> AjustarInventarioAsync(
        AjustarInventarioViewModel model,
        int usuarioId)
    {
        if (!model.NuevaCantidad.HasValue || model.NuevaCantidad.Value < 0)
        {
            return AjustarInventarioResult.InvalidQuantity;
        }

        var motivo = model.Motivo.Trim();

        if (motivo.Length < 5 || motivo.Length > 300)
        {
            return AjustarInventarioResult.InvalidReason;
        }

        byte[] version;

        try
        {
            version = Convert.FromBase64String(model.Version);
        }
        catch (FormatException)
        {
            return AjustarInventarioResult.ConcurrencyConflict;
        }

        var producto = await _context.Productos
            .Include(item => item.Inventario)
            .SingleOrDefaultAsync(item => item.Id == model.ProductoId);

        if (producto?.Inventario is null)
        {
            return AjustarInventarioResult.NotFound;
        }

        var inventario = producto.Inventario;
        var cantidadAnterior = inventario.CantidadDisponible;
        var cantidadNueva = model.NuevaCantidad.Value;

        if (cantidadAnterior == cantidadNueva)
        {
            return AjustarInventarioResult.NoChange;
        }

        _context.Entry(inventario)
            .Property(item => item.Version)
            .OriginalValue = version;

        inventario.CantidadDisponible = cantidadNueva;
        inventario.FechaActualizacion = DateTime.UtcNow;

        _context.MovimientosInventario.Add(new MovimientoInventario
        {
            ProductoId = producto.Id,
            TipoMovimiento = "Ajuste",
            Cantidad = Math.Abs(cantidadNueva - cantidadAnterior),
            CantidadAnterior = cantidadAnterior,
            CantidadNueva = cantidadNueva,
            Motivo = motivo,
            FechaMovimiento = DateTime.UtcNow,
            UsuarioId = usuarioId
        });

        try
        {
            await _context.SaveChangesAsync();
            return AjustarInventarioResult.Success;
        }
        catch (DbUpdateConcurrencyException)
        {
            return AjustarInventarioResult.ConcurrencyConflict;
        }
    }

    private IQueryable<Producto> QueryProductos()
    {
        return _context.Productos
            .AsNoTracking()
            .Include(producto => producto.Categoria)
            .Include(producto => producto.Inventario);
    }

    private static IQueryable<Producto> AplicarFiltros(IQueryable<Producto> query, int? categoriaId, string? talla, string? color, bool? activo)
    {
        if (categoriaId.HasValue)
        {
            query = query.Where(producto => producto.CategoriaId == categoriaId.Value);
        }

        if (!string.IsNullOrWhiteSpace(talla))
        {
            query = query.Where(producto => producto.Talla == talla);
        }

        if (!string.IsNullOrWhiteSpace(color))
        {
            query = query.Where(producto => producto.Color == color);
        }

        if (activo.HasValue)
        {
            query = query.Where(producto => producto.Activo == activo.Value);
        }

        return query;
    }

    private async Task<IEnumerable<SelectListItem>> ObtenerCategoriasSelectAsync(int? selectedId = null)
    {
        return await _context.Categorias
            .AsNoTracking()
            .Where(categoria => categoria.Activo)
            .OrderBy(categoria => categoria.Nombre)
            .Select(categoria => new SelectListItem
            {
                Value = categoria.Id.ToString(),
                Text = categoria.Nombre,
                Selected = selectedId == categoria.Id
            })
            .ToListAsync();
    }

    private async Task<IEnumerable<SelectListItem>> ObtenerTallasSelectAsync(string? selected = null)
    {
        return await _context.Productos
            .AsNoTracking()
            .Where(producto => producto.Talla != string.Empty)
            .Select(producto => producto.Talla)
            .Distinct()
            .OrderBy(talla => talla)
            .Select(talla => new SelectListItem
            {
                Value = talla,
                Text = talla,
                Selected = selected == talla
            })
            .ToListAsync();
    }

    private async Task<IEnumerable<SelectListItem>> ObtenerColoresSelectAsync(string? selected = null)
    {
        return await _context.Productos
            .AsNoTracking()
            .Where(producto => producto.Color != string.Empty)
            .Select(producto => producto.Color)
            .Distinct()
            .OrderBy(color => color)
            .Select(color => new SelectListItem
            {
                Value = color,
                Text = color,
                Selected = selected == color
            })
            .ToListAsync();
    }

    private async Task<ProductoOperacionResult> ValidarProductoAsync(ProductoFormViewModel model)
    {
        if (!model.Precio.HasValue || model.Precio.Value < 0)
        {
            return ProductoOperacionResult.InvalidPrice;
        }

        if (!model.CantidadDisponible.HasValue || model.CantidadDisponible.Value < 0)
        {
            return ProductoOperacionResult.InvalidInventoryQuantity;
        }

        if (!model.StockMinimo.HasValue || model.StockMinimo.Value < 0)
        {
            return ProductoOperacionResult.InvalidMinimumStock;
        }

        var categoriaExiste = await _context.Categorias.AnyAsync(categoria =>
            categoria.Id == model.CategoriaId && categoria.Activo);

        return categoriaExiste
            ? ProductoOperacionResult.Success
            : ProductoOperacionResult.CategoriaNotFound;
    }
}
