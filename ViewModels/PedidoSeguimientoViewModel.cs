namespace VibraUrbana.ViewModels;

public class PedidoSeguimientoViewModel
{
    public int Id { get; set; }

    public string Codigo => $"PED-{Id:D6}";

    public string TokenEntrega { get; set; } = string.Empty;

    public string NombreCliente { get; set; } = string.Empty;

    public string Estado { get; set; } = string.Empty;

    public DateTime FechaPedido { get; set; }

    public DateTime? FechaEnvio { get; set; }

    public DateTime? FechaTransito { get; set; }

    public DateTime? FechaEntrega { get; set; }

    public bool PuedeConfirmarEntrega { get; set; }

    public string Mensaje { get; set; } = string.Empty;
}
