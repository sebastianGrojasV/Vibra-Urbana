# Vibra Urbana

Vibra Urbana es un proyecto academico para la gestion comercial de una tienda de ropa. Esta version corresponde a la base del sistema web en ASP.NET Core MVC con configuracion inicial de Entity Framework Core.

## Estado actual

Estructura base ASP.NET Core MVC con navegacion inicial, layout compartido, controladores base, vistas Razor minimas, archivos estaticos organizados y configuracion inicial de base de datos para usuarios, roles y permisos.

Todavia no incluye login funcional, CRUD, inventario funcional, ventas, facturacion, carrito ni reportes reales.

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

Tambien existe `DbInitializer` para preparar roles y permisos iniciales cuando se conecte el flujo de migraciones o inicializacion controlada.

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

Cuando se quiera crear la primera migracion:

```bash
dotnet ef migrations add InitialIdentitySchema
dotnet ef database update
```

## Paquetes NuGet agregados

- `Microsoft.EntityFrameworkCore.SqlServer`: proveedor de SQL Server para EF Core.
- `Microsoft.EntityFrameworkCore.Tools`: herramientas para migraciones y comandos EF.
- `Microsoft.EntityFrameworkCore.Design`: soporte de diseno requerido por las herramientas EF.
