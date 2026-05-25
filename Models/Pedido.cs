namespace VibraUrbana.Models;

public class Pedido
{
    public int Id { get; set; }

    public int? ClienteId { get; set; }

    public Cliente? Cliente { get; set; }

    public string NombreCliente { get; set; } = string.Empty;

    public string Telefono { get; set; } = string.Empty;

    public string Correo { get; set; } = string.Empty;

    public string DireccionEntrega { get; set; } = string.Empty;

    public DateTime FechaPedido { get; set; } = DateTime.UtcNow;

    public int MetodoPagoId { get; set; }

    public MetodoPago MetodoPago { get; set; } = null!;

    public decimal Subtotal { get; set; }

    public decimal Descuento { get; set; }

    public decimal Impuesto { get; set; }

    public decimal Total { get; set; }

    public string Estado { get; set; } = "Pendiente";

    public string Observacion { get; set; } = string.Empty;

    public ICollection<DetallePedido> DetallePedidos { get; set; } = new List<DetallePedido>();

    public ComprobantePago? ComprobantePago { get; set; }
}
