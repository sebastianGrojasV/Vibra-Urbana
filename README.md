# Vibra Urbana

Vibra Urbana es un proyecto academico para la gestion comercial de una tienda de ropa. Esta version corresponde a la base del sistema web en ASP.NET Core MVC con configuracion inicial de Entity Framework Core.

## Estado actual

Estructura ASP.NET Core MVC con navegacion inicial, layout compartido, controladores base, vistas Razor minimas, archivos estaticos organizados y base de datos relacional completa para los modulos principales del sistema.

Todavia no incluye login funcional, CRUD, inventario funcional, ventas, facturacion, carrito ni reportes reales en la interfaz.

## Tecnologias utilizadas

- ASP.NET Core MVC .NET 8
- C#
- Razor Views
- Bootstrap
- JavaScript
- Entity Framework Core
- SQL Server

## Estructura general

```text
VibraUrbana-main/
|-- Controllers/
|-- Models/
|-- ViewModels/
|-- Views/
|   |-- Account/
|   |-- Admin/
|   |-- Home/
|   |-- Shared/
|-- Services/
|-- Repositories/
|-- Data/
|   |-- ApplicationDbContext.cs
|   |-- DbInitializer.cs
|-- wwwroot/
|   |-- assets/
|   |-- css/
|   |-- img/
|   |-- js/
|   |-- lib/
|-- Program.cs
|-- appsettings.json
|-- VibraUrbana.csproj
```

## Base de datos

La aplicacion esta configurada para usar SQL Server Express con la base local `VibraUrbanaDb`.

Cadena de conexion de desarrollo:

```text
Server=localhost\SQLEXPRESS;Database=VibraUrbanaDb;Trusted_Connection=True;TrustServerCertificate=True;
```

Entidades iniciales:

- `Usuario`
- `Rol`
- `Permiso`
- `RolPermiso`
- `Bitacora`
- `Cliente`
- `Categoria`
- `Producto`
- `Inventario`
- `MovimientoInventario`
- `Venta`
- `DetalleVenta`
- `Factura`
- `MetodoPago`
- `CierreCaja`
- `Pedido`
- `DetallePedido`
- `ComprobantePago`

La migracion `CrearBaseDatosCompleta` crea la estructura completa y carga datos iniciales para roles, permisos, asignaciones rol-permiso y metodos de pago.

Datos iniciales:

- Roles: Administrador, Cajero, Inventario, Consulta.
- Metodos de pago: Efectivo, SINPE, Tarjeta.
- Permisos base: usuarios, roles, clientes, productos, inventario, ventas, facturacion y reportes.

## Como ejecutar localmente

1. Verificar que SQL Server Express este activo y que exista la base `VibraUrbanaDb`.
2. Restaurar dependencias:

```bash
dotnet restore
```

3. Compilar:

```bash
dotnet build
```

4. Ejecutar la aplicacion:

```bash
dotnet run
```

5. Abrir la URL indicada por la consola. La ruta inicial carga `Home/Index`.

## Migraciones EF Core

Para aplicar la base de datos en un entorno local:

```bash
dotnet ef database update
```

Migraciones actuales:

- `InitialIdentityStructure`
- `CrearBaseDatosCompleta`

## Paquetes NuGet agregados

- `Microsoft.EntityFrameworkCore.SqlServer`: proveedor de SQL Server para EF Core.
- `Microsoft.EntityFrameworkCore.Tools`: herramientas para migraciones y comandos EF.
- `Microsoft.EntityFrameworkCore.Design`: soporte de diseno requerido por las herramientas EF.
