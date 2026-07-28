using VibraUrbana.ViewModels;

namespace VibraUrbana.Services;

public interface IPedidoServicio
{
    Task<PedidosIndexViewModel> ObtenerPedidosAsync(string? estado, DateTime? fechaInicio, DateTime? fechaFin, string? cliente);

    Task<PedidoDetalleViewModel?> ObtenerDetalleAsync(int id);

    Task<PedidoOperacionResult> CambiarEstadoAsync(int pedidoId, string nuevoEstado, int usuarioId);
}

public class PedidoOperacionResult
{
    private PedidoOperacionResult(bool esExitoso, string mensaje)
    {
        EsExitoso = esExitoso;
        Mensaje = mensaje;
    }

    public bool EsExitoso { get; }

    public string Mensaje { get; }

    public static PedidoOperacionResult Success(string mensaje) => new(true, mensaje);

    public static PedidoOperacionResult Failure(string mensaje) => new(false, mensaje);
}
