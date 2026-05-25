namespace VibraUrbana.Models;

public class MetodoPago
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public bool Activo { get; set; } = true;

    public ICollection<Venta> Ventas { get; set; } = new List<Venta>();

    public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();

    public ICollection<ComprobantePago> ComprobantesPago { get; set; } = new List<ComprobantePago>();
}
