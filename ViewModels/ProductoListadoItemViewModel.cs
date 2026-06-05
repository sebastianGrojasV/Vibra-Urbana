namespace VibraUrbana.ViewModels;

public class ProductoListadoItemViewModel
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;

    public decimal Precio { get; set; }

    public string Talla { get; set; } = string.Empty;

    public string Color { get; set; } = string.Empty;

    public string ImagenUrl { get; set; } = string.Empty;

    public bool Activo { get; set; }

    public string Categoria { get; set; } = string.Empty;

    public int CantidadDisponible { get; set; }

    public int StockMinimo { get; set; }

    public bool StockBajo => CantidadDisponible <= StockMinimo;
}
