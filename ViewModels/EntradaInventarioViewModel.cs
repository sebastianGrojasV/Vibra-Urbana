using System.ComponentModel.DataAnnotations;

namespace VibraUrbana.ViewModels;

public class EntradaInventarioViewModel
{
    public int ProductoId { get; set; }

    public string Producto { get; set; } = string.Empty;

    public string Categoria { get; set; } = string.Empty;

    public string Talla { get; set; } = string.Empty;

    public string Color { get; set; } = string.Empty;

    public int CantidadActual { get; set; }

    [Required(ErrorMessage = "La cantidad ingresada es obligatoria.")]
    [Range(1, int.MaxValue, ErrorMessage = "La cantidad ingresada debe ser mayor a cero.")]
    [Display(Name = "Cantidad ingresada")]
    public int? CantidadIngresada { get; set; }

    [StringLength(300, ErrorMessage = "La observación no puede superar los 300 caracteres.")]
    [Display(Name = "Observación")]
    public string Observacion { get; set; } = string.Empty;

    [Required]
    public string Version { get; set; } = string.Empty;
}
