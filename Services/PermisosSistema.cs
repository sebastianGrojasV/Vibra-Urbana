namespace VibraUrbana.Services;

public static class PermisosSistema
{
    public const string UsuariosGestionar = "Usuarios.Gestionar";
    public const string RolesGestionar = "Roles.Gestionar";
    public const string ClientesGestionar = "Clientes.Gestionar";
    public const string ProductosGestionar = "Productos.Gestionar";
    public const string InventarioGestionar = "Inventario.Gestionar";
    public const string VentasGestionar = "Ventas.Gestionar";
    public const string FacturacionGestionar = "Facturacion.Gestionar";
    public const string ReportesVer = "Reportes.Ver";

    public static readonly IReadOnlyList<string> Todos = new[]
    {
        UsuariosGestionar,
        RolesGestionar,
        ClientesGestionar,
        ProductosGestionar,
        InventarioGestionar,
        VentasGestionar,
        FacturacionGestionar,
        ReportesVer
    };
}
