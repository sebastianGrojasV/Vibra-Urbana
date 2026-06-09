using System.ComponentModel.DataAnnotations;

namespace VibraUrbana.ViewModels;

public class CrearRolViewModel
{
    [Required(ErrorMessage = "El nombre del rol es obligatorio.")]
    [StringLength(80, ErrorMessage = "El nombre del rol no puede superar 80 caracteres.")]
    [Display(Name = "Nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "La descripción del rol es obligatoria.")]
    [StringLength(250, ErrorMessage = "La descripción no puede superar 250 caracteres.")]
    [Display(Name = "Descripción")]
    public string Descripcion { get; set; } = string.Empty;
}