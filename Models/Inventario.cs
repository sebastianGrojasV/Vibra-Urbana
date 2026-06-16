namespace VibraUrbana.Models;

public class Inventario
{
    public int Id { get; set; }

    public int ProductoId { get; set; }

    public Producto Producto { get; set; } = null!;

    public int CantidadDisponible { get; set; }

    public int StockMinimo { get; set; }

    public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;

    public byte[] Version { get; set; } = [];
}
