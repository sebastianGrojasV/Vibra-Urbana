# Vibra Urbana

Vibra Urbana es un proyecto academico para la gestion comercial de una tienda de ropa. Esta version corresponde a la estructura base del sistema web en ASP.NET Core MVC.

## Estado actual

Estructura base ASP.NET Core MVC para Sprint 1. La aplicacion ya cuenta con navegacion inicial, layout compartido, controladores base, vistas Razor minimas y archivos estaticos organizados. Todavia no incluye autenticacion real, CRUD, inventario funcional, ventas, facturacion, carrito, reportes reales ni conexion a base de datos.

## Tecnologias utilizadas

- ASP.NET Core MVC .NET 8
- C#
- Razor Views
- Bootstrap
- JavaScript
- Entity Framework Core preparado para etapas posteriores
- SQL Server preparado para etapas posteriores

## Estructura general

```text
VibraUrbana-main/
├── Controllers/
├── Models/
├── ViewModels/
├── Views/
│   ├── Account/
│   ├── Admin/
│   ├── Home/
│   └── Shared/
├── Services/
├── Repositories/
├── Data/
├── wwwroot/
│   ├── assets/
│   ├── css/
│   ├── img/
│   ├── js/
│   └── lib/
├── Program.cs
├── appsettings.json
└── VibraUrbana.csproj
```

La beta visual estatica fue retirada de la raiz para evitar duplicidad. Los CSS, JavaScript y recursos visuales necesarios quedaron organizados en `wwwroot` para integrarlos con ASP.NET Core.

## Como ejecutar localmente

1. Verificar que el SDK o runtime de .NET compatible con `net8.0` este instalado.
2. Restaurar dependencias:

```bash
dotnet restore
```

3. Ejecutar la aplicacion:

```bash
dotnet run
```

4. Abrir la URL indicada por la consola. La ruta inicial carga `Home/Index`.

## Paquetes NuGet

No se agregaron paquetes NuGet adicionales para esta tarea. La base MVC usa las dependencias incluidas por `Microsoft.NET.Sdk.Web` y los recursos estaticos generados por la plantilla.
