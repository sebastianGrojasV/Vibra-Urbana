namespace VibraUrbana.ViewModels;

public class CatalogoDetalleViewModel
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;

    public decimal Precio { get; set; }

    public string Categoria { get; set; } = string.Empty;

    public string Talla { get; set; } = string.Empty;

    public string Color { get; set; } = string.Empty;

    public string ImagenUrl { get; set; } = string.Empty;

    public int CantidadDisponible { get; set; }

    public bool Disponible => CantidadDisponible > 0;
}
