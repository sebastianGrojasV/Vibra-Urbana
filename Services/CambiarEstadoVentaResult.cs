namespace VibraUrbana.Services;

public class CambiarEstadoVentaResult
{
    public bool EsExitoso { get; private set; }

    public string Mensaje { get; private set; } = string.Empty;

    public static CambiarEstadoVentaResult Success(string mensaje = "Estado de venta actualizado correctamente.") =>
        new() { EsExitoso = true, Mensaje = mensaje };

    public static CambiarEstadoVentaResult Failure(string mensaje) =>
        new() { EsExitoso = false, Mensaje = mensaje };
}
