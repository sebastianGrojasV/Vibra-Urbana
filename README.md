# Vibra Urbana

Vibra Urbana es un sistema web académico para la gestión comercial de una tienda de ropa. Su objetivo es apoyar procesos internos como administración de usuarios, control de roles y permisos, gestión de clientes, productos, inventario, ventas y reportes.

El proyecto está desarrollado con ASP.NET Core MVC sobre .NET 8, utilizando Razor Views, Bootstrap, JavaScript, Entity Framework Core y Azure SQL Database.

## Características principales

- Inicio y cierre de sesión.
- Control de acceso mediante roles y permisos.
- Administración de usuarios internos.
- Gestión de roles del sistema.
- Registro y consulta de clientes.
- Historial de compras por cliente.
- Gestión de categorías y productos.
- Consulta y ajuste de inventario.
- Registro de entradas de inventario.
- Visualización de stock bajo.
- Módulo base de ventas.
- Facturación y cierre de caja.
- Catálogo público y carrito de compras.
- Gestión administrativa de pedidos en línea.
- Trazabilidad de entregas mediante código QR.
- Reportes iniciales del sistema.

## Tecnologías utilizadas

- ASP.NET Core MVC .NET 8
- C#
- Razor Views
- Entity Framework Core
- Azure SQL Database
- SQL Server
- Bootstrap
- JavaScript
- GitHub

## Arquitectura general

El sistema utiliza una arquitectura MVC con separación entre controladores, modelos, vistas, servicios, repositorios y modelos de vista.

```text
VibraUrbana-main/
|-- Controllers/        Controladores MVC
|-- Data/               Contexto de base de datos e inicialización
|-- Filters/            Filtros de la aplicación
|-- Migrations/         Migraciones de Entity Framework Core
|-- Models/             Entidades del dominio
|-- Repositories/       Acceso a datos
|-- Scripts/            Scripts de apoyo para base de datos
|-- Services/           Lógica de aplicación
|-- ViewModels/         Modelos utilizados por las vistas
|-- Views/              Vistas Razor
|-- wwwroot/            Archivos estáticos
|-- Program.cs          Configuración principal
|-- appsettings.json    Configuración base
|-- VibraUrbana.csproj  Archivo del proyecto
```

## Base de datos

Vibra Urbana trabaja con Entity Framework Core y SQL Server. Para el ambiente de desarrollo compartido se utiliza Azure SQL Database, de manera que todos los integrantes puedan trabajar sobre una misma base de datos.

La cadena de conexión real no se almacena en el repositorio. Cada instalación debe configurarla de forma local mediante `dotnet user-secrets` o variables de entorno.

Ejemplo de configuración con `dotnet user-secrets`:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "CADENA_AZURE_SQL"
```

Ejemplo de formato de conexión con autenticación SQL:

```text
Server=tcp:servidor.database.windows.net,1433;Initial Catalog=VibraUrbanaDB;Persist Security Info=False;User ID=USUARIO;Password=CONTRASENA;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```

Para verificar la configuración local:

```powershell
dotnet user-secrets list
```

## Ejecución del proyecto

Requisitos recomendados:

- .NET SDK 8
- Visual Studio 2022 o Visual Studio Code
- Acceso a la base de datos configurada
- Herramientas de Entity Framework Core

Restaurar dependencias:

```powershell
dotnet restore
```

Compilar:

```powershell
dotnet build
```

Ejecutar:

```powershell
dotnet run
```

Al iniciar, la consola mostrará la URL local donde se encuentra disponible la aplicación.

### Ejecutar con recarga automática

Durante desarrollo se recomienda usar `dotnet watch`. Este modo detecta cambios en vistas Razor, CSS, JavaScript y C#, recompila cuando corresponde y refresca el navegador automáticamente.

Opción directa:

```powershell
dotnet watch --launch-profile http run
```

Opción con script del proyecto:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\Scripts\Start-DevWatch.ps1
```

Si el equipo nota que los cambios descargados con `git pull` no se detectan en alguna computadora, pueden activar el watcher por sondeo:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\Scripts\Start-DevWatch.ps1 -UsePollingWatcher
```

Notas de uso:

- Cada integrante debe tener configurada su cadena de Azure SQL en `user-secrets` o variable de entorno.
- Si un compañero sube cambios al repositorio, primero se debe ejecutar `git pull`.
- Con `dotnet watch` activo, después del `git pull` la aplicación se recompila y el navegador se actualiza sin presionar F5.

## Migraciones

Las migraciones de Entity Framework Core permiten mantener sincronizada la estructura de la base de datos con las entidades del proyecto.

Aplicar migraciones pendientes:

```powershell
dotnet ef database update
```

Crear una nueva migración:

```powershell
dotnet ef migrations add NombreDeLaMigracion
dotnet ef database update
```

Migraciones actuales:

- `InitialIdentityStructure`
- `CrearBaseDatosCompleta`
- `AgregarControlConcurrenciaInventario`
- `AgregarAnulacionVenta`
- `AgregarTrazabilidadEntregaPedidos`
- `VincularPedidosEnLineaConVentas`

## Trazabilidad de entregas

Cada pedido cuenta con un código de seguimiento interno. Desde el detalle administrativo del pedido se puede visualizar un código QR para pegar en la caja de entrega.

Flujo esperado:

1. El pedido se registra como `Pendiente de verificación`.
2. El personal autorizado lo confirma y el sistema descuenta inventario.
3. El pedido puede avanzar a `Enviado` y luego a `En tránsito`.
4. El cliente escanea el QR y confirma la recepción.
5. El sistema marca el pedido como `Entregado` y registra la fecha de entrega.

El QR utiliza un token único por pedido para no exponer el identificador simple de la base de datos.

## Pedidos en línea y reportes

Los pedidos en línea se reflejan en el reporte de ventas y en el panel administrativo. Mientras están pendientes se muestran como operaciones pendientes del canal web. Cuando el personal autorizado confirma un pedido, el sistema lo vincula con una venta aprobada, genera su factura y permite que entre al flujo normal de facturación, cierre de caja y reportes comerciales.

## Seguridad de configuración

Este repositorio no debe contener credenciales reales, contraseñas, cadenas de conexión privadas ni scripts con datos sensibles.

Archivos y carpetas ignoradas por seguridad:

- `.database/`
- `bin/`
- `obj/`
- `.vs/`
- `Credenciales.txt`
- `appsettings.Development.json`
- `appsettings.*.local.json`

## Scripts disponibles

El directorio `Scripts/` contiene herramientas de apoyo para administración de base de datos.

`Update-AzureDatabase.ps1` permite aplicar migraciones y, opcionalmente, cargar datos mediante un script externo:

```powershell
$env:VIBRA_AZURE_CONNECTION = "CADENA_AZURE_SQL"
powershell -NoProfile -ExecutionPolicy Bypass -File .\Scripts\Update-AzureDatabase.ps1 -SkipData
Remove-Item Env:\VIBRA_AZURE_CONNECTION
```

`Export-LocalData.ps1` permite exportar datos desde una base local hacia un archivo SQL. Esta herramienta debe utilizarse con precaución, ya que el archivo generado puede contener información sensible.

## Datos iniciales

Los formularios de roles y clientes muestran contadores y limitan los caracteres según sus validaciones. Los correos de clientes son únicos, incluso al variar mayúsculas o espacios al inicio o al final; también se valida el registro desde pedidos. La migración `CorreoUnicoClientes` agrega la restricción en SQL Server. Si ya existen correos duplicados, debe corregirse esa información antes de aplicar la migración; no se eliminan ni modifican datos automáticamente.

La base del sistema contempla datos iniciales para:

- Roles: Administrador, Cajero, Inventario y Consulta.
- Métodos de pago: Efectivo, SINPE y Tarjeta.
- Permisos base para usuarios, roles, clientes, productos, inventario, ventas, facturación y reportes.

## Estado del proyecto

La interfaz adapta la navegación y los listados a teléfonos y tabletas. En pantallas pequeñas, los módulos se abren en un menú lateral desplegable, los registros se presentan como fichas con búsqueda y ordenación, y el catálogo incluye filtros plegables y navegación inferior al carrito. Los formularios tienen controles táctiles, foco visible y campos adecuados para el teclado móvil. Los estilos de impresión se conservan para comprobantes y etiquetas.

Las nuevas ventas en efectivo redondean el total final al múltiplo de ₡10 más cercano; los empates se redondean hacia arriba. Esta es la política de cobro del sistema. El ajuste se muestra por separado en venta, factura y comprobante, sin modificar el descuento ni el IVA. SINPE y tarjeta conservan el importe a dos decimales. Reportes y cierre de caja usan el total guardado. Las ventas históricas no se recalculan. Este cambio no requiere migración.

El proyecto se encuentra en desarrollo académico. Actualmente cuenta con la base funcional de seguridad, usuarios, roles, clientes, productos e inventario, y continúa evolucionando por medio de historias de usuario y tareas técnicas por sprint.
