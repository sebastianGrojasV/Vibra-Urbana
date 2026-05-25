using Microsoft.EntityFrameworkCore;
using VibraUrbana.Models;

namespace VibraUrbana.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        await SeedRolesAsync(context);
        await SeedPermisosAsync(context);
        await SeedMetodosPagoAsync(context);
        await SeedRolPermisosAsync(context);
    }

    private static async Task SeedRolesAsync(ApplicationDbContext context)
    {
        var roles = new[]
        {
            new Rol { Nombre = "Administrador", Descripcion = "Acceso administrativo general" },
            new Rol { Nombre = "Cajero", Descripcion = "Gestion de ventas y facturacion" },
            new Rol { Nombre = "Inventario", Descripcion = "Gestion de productos e inventario" },
            new Rol { Nombre = "Consulta", Descripcion = "Acceso de solo consulta" }
        };

        foreach (var rol in roles)
        {
            var exists = await context.Roles.AnyAsync(existing => existing.Nombre == rol.Nombre);

            if (!exists)
            {
                context.Roles.Add(rol);
            }
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedPermisosAsync(ApplicationDbContext context)
    {
        var permisos = new[]
        {
            new Permiso { Nombre = "Usuarios.Gestionar", Descripcion = "Gestionar usuarios" },
            new Permiso { Nombre = "Roles.Gestionar", Descripcion = "Gestionar roles y permisos" },
            new Permiso { Nombre = "Clientes.Gestionar", Descripcion = "Gestionar clientes" },
            new Permiso { Nombre = "Productos.Gestionar", Descripcion = "Gestionar productos" },
            new Permiso { Nombre = "Inventario.Gestionar", Descripcion = "Gestionar inventario" },
            new Permiso { Nombre = "Ventas.Gestionar", Descripcion = "Gestionar ventas" },
            new Permiso { Nombre = "Facturacion.Gestionar", Descripcion = "Gestionar facturacion" },
            new Permiso { Nombre = "Reportes.Ver", Descripcion = "Ver reportes" }
        };

        foreach (var permiso in permisos)
        {
            var exists = await context.Permisos.AnyAsync(existing => existing.Nombre == permiso.Nombre);

            if (!exists)
            {
                context.Permisos.Add(permiso);
            }
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedMetodosPagoAsync(ApplicationDbContext context)
    {
        var metodosPago = new[]
        {
            new MetodoPago { Nombre = "Efectivo" },
            new MetodoPago { Nombre = "SINPE" },
            new MetodoPago { Nombre = "Tarjeta" }
        };

        foreach (var metodoPago in metodosPago)
        {
            var exists = await context.MetodosPago.AnyAsync(existing => existing.Nombre == metodoPago.Nombre);

            if (!exists)
            {
                context.MetodosPago.Add(metodoPago);
            }
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedRolPermisosAsync(ApplicationDbContext context)
    {
        var adminRole = await context.Roles.SingleAsync(rol => rol.Nombre == "Administrador");
        var adminPermissionIds = await context.Permisos.Select(permiso => permiso.Id).ToListAsync();

        foreach (var permisoId in adminPermissionIds)
        {
            var exists = await context.RolPermisos.AnyAsync(rolPermiso =>
                rolPermiso.RolId == adminRole.Id && rolPermiso.PermisoId == permisoId);

            if (!exists)
            {
                context.RolPermisos.Add(new RolPermiso
                {
                    RolId = adminRole.Id,
                    PermisoId = permisoId
                });
            }
        }

        await context.SaveChangesAsync();
    }
}
