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

### Base compartida en Azure durante desarrollo

No guardar la cadena de conexion de Azure en `appsettings.json`, porque contiene usuario y contrasena. Este proyecto tiene habilitado `User Secrets` para que cada integrante configure su conexion localmente:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=tcp:TU_SERVIDOR.database.windows.net,1433;Initial Catalog=TU_BASE;Persist Security Info=False;User ID=TU_USUARIO;Password=TU_PASSWORD;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
```

Con esa configuracion, `Program.cs` seguira usando `DefaultConnection`, pero la cadena de Azure se leera desde los secretos locales y no desde Git.

Para crear o actualizar las tablas en Azure:

```bash
dotnet ef database update
```

Para copiar los datos actuales desde SQL Server Express local hacia Azure:

```powershell
$env:VIBRA_AZURE_CONNECTION = "Server=tcp:TU_SERVIDOR.database.windows.net,1433;Initial Catalog=TU_BASE;Persist Security Info=False;User ID=TU_USUARIO;Password=TU_PASSWORD;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
powershell -NoProfile -ExecutionPolicy Bypass -File .\Scripts\Export-LocalData.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\Scripts\Update-AzureDatabase.ps1
Remove-Item Env:\VIBRA_AZURE_CONNECTION
```

El archivo generado `.database/local-data.sql` queda ignorado por Git porque puede contener datos de usuarios, clientes, ventas y hashes de contrasenas.

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
- `AgregarControlConcurrenciaInventario`

## Paquetes NuGet agregados

- `Microsoft.EntityFrameworkCore.SqlServer`: proveedor de SQL Server para EF Core.
- `Microsoft.EntityFrameworkCore.Tools`: herramientas para migraciones y comandos EF.
- `Microsoft.EntityFrameworkCore.Design`: soporte de diseno requerido por las herramientas EF.
