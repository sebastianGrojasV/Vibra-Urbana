namespace VibraUrbana.Models;

public class ComprobantePago
{
    public int Id { get; set; }

    public int PedidoId { get; set; }

    public Pedido Pedido { get; set; } = null!;

    public int MetodoPagoId { get; set; }

    public MetodoPago MetodoPago { get; set; } = null!;

    public string ReferenciaPago { get; set; } = string.Empty;

    public string ImagenComprobanteUrl { get; set; } = string.Empty;

    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    public string EstadoVerificacion { get; set; } = "Pendiente";
}
