namespace VibraUrbana.ViewModels;

public class InventarioListadoItemViewModel
{
    public int ProductoId { get; set; }

    public string Producto { get; set; } = string.Empty;

    public string Categoria { get; set; } = string.Empty;

    public string Talla { get; set; } = string.Empty;

    public string Color { get; set; } = string.Empty;

    public bool Activo { get; set; }

    public int CantidadDisponible { get; set; }

    public int StockMinimo { get; set; }

    public DateTime FechaActualizacion { get; set; }

    public bool StockBajo => CantidadDisponible <= StockMinimo;
}
