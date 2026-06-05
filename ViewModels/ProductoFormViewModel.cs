using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace VibraUrbana.ViewModels;

public class ProductoFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre del producto es obligatorio.")]
    [StringLength(150, ErrorMessage = "El nombre no puede superar 150 caracteres.")]
    [Display(Name = "Nombre")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "La descripción no puede superar 500 caracteres.")]
    [Display(Name = "Descripción")]
    public string Descripcion { get; set; } = string.Empty;

    [Required(ErrorMessage = "El precio es obligatorio.")]
    [Range(0.01, 9999999, ErrorMessage = "El precio debe ser mayor a cero.")]
    [Display(Name = "Precio")]
    public decimal Precio { get; set; }

    [StringLength(20, ErrorMessage = "La talla no puede superar 20 caracteres.")]
    [Display(Name = "Talla")]
    public string Talla { get; set; } = string.Empty;

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

    [Range(0, int.MaxValue, ErrorMessage = "La cantidad disponible no puede ser negativa.")]
    [Display(Name = "Cantidad disponible")]
    public int CantidadDisponible { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "El stock mínimo no puede ser negativo.")]
    [Display(Name = "Stock mínimo")]
    public int StockMinimo { get; set; }

    [Display(Name = "Activo")]
    public bool Activo { get; set; } = true;

    public IEnumerable<SelectListItem> Categorias { get; set; } = Enumerable.Empty<SelectListItem>();
}
