namespace VibraUrbana.ViewModels;

public class VentaOperacionResult
{
    public bool EsExitoso { get; set; }

    public string Mensaje { get; set; } = string.Empty;

    public int? VentaId { get; set; }

    public string? NumeroFactura { get; set; }

    public static VentaOperacionResult Success(
        int ventaId,
        string numeroFactura,
        string mensaje = "Venta registrada con éxito.") =>
        new() { EsExitoso = true, VentaId = ventaId, NumeroFactura = numeroFactura, Mensaje = mensaje };

    public static VentaOperacionResult Failure(string mensaje) =>
        new() { EsExitoso = false, Mensaje = mensaje };
}
