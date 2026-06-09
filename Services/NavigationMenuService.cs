using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using VibraUrbana.Data;

namespace VibraUrbana.Services;

public class NavigationMenuService : INavigationMenuService
{
    private readonly ApplicationDbContext _context;

    public NavigationMenuService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<MenuItemViewModel>> GetItemsAsync(ClaimsPrincipal user)
    {
        var items = new List<MenuItemViewModel>
        {
            new() { Texto = "Inicio", Controller = "Home" }
        };

        if (user.Identity?.IsAuthenticated != true)
        {
            return items;
        }

        var permisos = await GetPermisosAsync(user);

        if (permisos.Contains(PermisosSistema.UsuariosGestionar))
        {
            items.Add(new MenuItemViewModel { Texto = "Admin", Controller = "Admin" });
            items.Add(new MenuItemViewModel { Texto = "Usuarios", Controller = "Admin", Action = "Usuarios" });
        }

        if (permisos.Contains(PermisosSistema.RolesGestionar))
        {
            items.Add(new MenuItemViewModel { Texto = "Roles", Controller = "Rol" });
        }

        if (permisos.Contains(PermisosSistema.ClientesGestionar))
        {
            items.Add(new MenuItemViewModel { Texto = "Clientes", Controller = "Cliente" });
        }

        if (permisos.Contains(PermisosSistema.ProductosGestionar))
        {
            items.Add(new MenuItemViewModel { Texto = "Productos", Controller = "Producto" });
        }

        if (permisos.Contains(PermisosSistema.InventarioGestionar))
        {
            items.Add(new MenuItemViewModel { Texto = "Inventario", Controller = "Inventario" });
        }

        if (permisos.Contains(PermisosSistema.VentasGestionar))
        {
            items.Add(new MenuItemViewModel { Texto = "Ventas", Controller = "Venta" });
            items.Add(new MenuItemViewModel { Texto = "Pedidos", Controller = "Pedido" });
        }

        if (permisos.Contains(PermisosSistema.FacturacionGestionar))
        {
            items.Add(new MenuItemViewModel { Texto = "Facturación", Controller = "Factura" });
        }

        if (permisos.Contains(PermisosSistema.ReportesVer))
        {
            items.Add(new MenuItemViewModel { Texto = "Reportes", Controller = "Reporte" });
        }

        return items;
    }

    private async Task<HashSet<string>> GetPermisosAsync(ClaimsPrincipal user)
    {
        var roleName = user.FindFirstValue(ClaimTypes.Role);

        if (string.IsNullOrWhiteSpace(roleName))
        {
            return new HashSet<string>();
        }

        var permisos = await _context.Roles
            .Where(rol => rol.Activo && rol.Nombre == roleName)
            .SelectMany(rol => rol.RolPermisos)
            .Where(rolPermiso => rolPermiso.Permiso.Activo)
            .Select(rolPermiso => rolPermiso.Permiso.Nombre)
            .ToListAsync();

        return permisos.ToHashSet();
    }
}
