using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace VibraUrbana.ViewModels;

public class ProductoFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre del producto es obligatorio.")]
    [RegularExpression(@".*\S.*", ErrorMessage = "El nombre del producto no puede estar vacío.")]
    [StringLength(150, ErrorMessage = "El nombre no puede superar 150 caracteres.")]
    [Display(Name = "Nombre")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "La descripción no puede superar 500 caracteres.")]
    [Display(Name = "Descripción")]
    public string Descripcion { get; set; } = string.Empty;

    [Required(ErrorMessage = "El precio en colones es obligatorio.")]
    [Range(typeof(decimal), "0", "999999999", ErrorMessage = "El precio en colones debe ser mayor o igual a ₡0.")]
    [Display(Name = "Precio (₡)")]
    public decimal? Precio { get; set; }

    [Required(ErrorMessage = "La talla es obligatoria.")]
    [RegularExpression(@".*\S.*", ErrorMessage = "La talla no puede estar vacía.")]
    [StringLength(20, ErrorMessage = "La talla no puede superar 20 caracteres.")]
    [Display(Name = "Talla")]
    public string Talla { get; set; } = string.Empty;

    [Required(ErrorMessage = "El color es obligatorio.")]
    [RegularExpression(@".*\S.*", ErrorMessage = "El color no puede estar vacío.")]
    [StringLength(50, ErrorMessage = "El color no puede superar 50 caracteres.")]
    [Display(Name = "Color")]
    public string Color { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "La URL de imagen no puede superar 500 caracteres.")]
    [Display(Name = "Imagen del producto")]
    public string ImagenUrl { get; set; } = string.Empty;

    [Required(ErrorMessage = "La categoría es obligatoria.")]
    [Range(1, int.MaxValue, ErrorMessage = "Selecciona una categoría válida.")]
    [Display(Name = "Categoría")]
    public int CategoriaId { get; set; }

    [Required(ErrorMessage = "La cantidad disponible es obligatoria.")]
    [Range(0, int.MaxValue, ErrorMessage = "La cantidad disponible debe ser mayor o igual a cero.")]
    [Display(Name = "Cantidad disponible")]
    public int? CantidadDisponible { get; set; }

    [Required(ErrorMessage = "El stock mínimo es obligatorio.")]
    [Range(0, int.MaxValue, ErrorMessage = "El stock mínimo debe ser mayor o igual a cero.")]
    [Display(Name = "Stock mínimo")]
    public int? StockMinimo { get; set; }

    [Display(Name = "Activo")]
    public bool Activo { get; set; } = true;

    public IEnumerable<SelectListItem> Categorias { get; set; } = Enumerable.Empty<SelectListItem>();
}
