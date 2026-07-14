using System.ComponentModel.DataAnnotations;

namespace VibraUrbana.ViewModels;

public class AjustarInventarioViewModel
{
    public int ProductoId { get; set; }

    public string Producto { get; set; } = string.Empty;

    public string Categoria { get; set; } = string.Empty;

    public string Talla { get; set; } = string.Empty;

    public string Color { get; set; } = string.Empty;

    public int CantidadAnterior { get; set; }

    [Required(ErrorMessage = "La nueva cantidad es obligatoria.")]
    [Range(0, int.MaxValue, ErrorMessage = "La nueva cantidad debe ser mayor o igual a cero.")]
    [Display(Name = "Nueva cantidad")]
    public int? NuevaCantidad { get; set; }

    [Required(ErrorMessage = "El motivo del ajuste es obligatorio.")]
    [RegularExpression(@".*\S.*", ErrorMessage = "El motivo del ajuste no puede estar vacío.")]
    [StringLength(300, MinimumLength = 5, ErrorMessage = "El motivo debe contener entre 5 y 300 caracteres.")]
    [Display(Name = "Motivo del ajuste")]
    public string Motivo { get; set; } = string.Empty;

    [Required]
    public string Version { get; set; } = string.Empty;
}
