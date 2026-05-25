namespace VibraUrbana.Models;

public class Producto
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;

    public decimal Precio { get; set; }

    public string Talla { get; set; } = string.Empty;

    public string Color { get; set; } = string.Empty;

    public string ImagenUrl { get; set; } = string.Empty;

    public bool Activo { get; set; } = true;

    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    public int CategoriaId { get; set; }

    public Categoria Categoria { get; set; } = null!;

    public Inventario? Inventario { get; set; }

    public ICollection<MovimientoInventario> MovimientosInventario { get; set; } = new List<MovimientoInventario>();

    public ICollection<DetalleVenta> DetalleVentas { get; set; } = new List<DetalleVenta>();

    public ICollection<DetallePedido> DetallePedidos { get; set; } = new List<DetallePedido>();
}
