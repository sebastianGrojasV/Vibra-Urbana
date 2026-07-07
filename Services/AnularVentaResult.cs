namespace VibraUrbana.Services;

public class AnularVentaResult
{
    public bool EsExitoso { get; private set; }

    public string Mensaje { get; private set; } = string.Empty;

    public static AnularVentaResult Success(string mensaje = "Venta anulada correctamente.") =>
        new() { EsExitoso = true, Mensaje = mensaje };

    public static AnularVentaResult Failure(string mensaje) =>
        new() { EsExitoso = false, Mensaje = mensaje };
}
