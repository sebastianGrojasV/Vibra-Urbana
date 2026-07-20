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

La base del sistema contempla datos iniciales para:

- Roles: Administrador, Cajero, Inventario y Consulta.
- Métodos de pago: Efectivo, SINPE y Tarjeta.
- Permisos base para usuarios, roles, clientes, productos, inventario, ventas, facturación y reportes.

## Estado del proyecto

El proyecto se encuentra en desarrollo académico. Actualmente cuenta con la base funcional de seguridad, usuarios, roles, clientes, productos e inventario, y continúa evolucionando por medio de historias de usuario y tareas técnicas por sprint.
