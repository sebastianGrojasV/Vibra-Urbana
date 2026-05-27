using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace VibraUrbana.ViewModels;

public class EditarUsuarioViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "La cedula es obligatoria.")]
    [StringLength(20, ErrorMessage = "La cedula no puede superar 20 caracteres.")]
    [Display(Name = "Cedula")]
    public string Cedula { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre completo es obligatorio.")]
    [StringLength(150, ErrorMessage = "El nombre completo no puede superar 150 caracteres.")]
    [Display(Name = "Nombre completo")]
    public string NombreCompleto { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo electronico es obligatorio.")]
    [EmailAddress(ErrorMessage = "Ingresa un correo electronico valido.")]
    [StringLength(150, ErrorMessage = "El correo electronico no puede superar 150 caracteres.")]
    [Display(Name = "Correo electronico")]
    public string Correo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El rol es obligatorio.")]
    [Range(1, int.MaxValue, ErrorMessage = "Selecciona un rol valido.")]
    [Display(Name = "Rol")]
    public int RolId { get; set; }

    [Display(Name = "Activo")]
    public bool Activo { get; set; }

    public IEnumerable<SelectListItem> Roles { get; set; } = Enumerable.Empty<SelectListItem>();
}
