using System.ComponentModel.DataAnnotations;

namespace VibraUrbana.ViewModels;

public class EditarRolViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre del rol es obligatorio.")]
    [StringLength(80, ErrorMessage = "El nombre del rol no puede superar 80 caracteres.")]
    [Display(Name = "Nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "La descripción del rol es obligatoria.")]
    [StringLength(250, ErrorMessage = "La descripción no puede superar 250 caracteres.")]
    [Display(Name = "Descripción")]
    public string Descripcion { get; set; } = string.Empty;

    [Display(Name = "Activo")]
    public bool Activo { get; set; }

    public List<int> PermisosSeleccionados { get; set; } = new();

    public List<PermisoSeleccionViewModel> Permisos { get; set; } = new();
}
