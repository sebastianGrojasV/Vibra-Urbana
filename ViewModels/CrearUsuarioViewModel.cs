using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace VibraUrbana.ViewModels;

public class CrearUsuarioViewModel
{
    [Required(ErrorMessage = "La cédula es obligatoria.")]
    [RegularExpression(@"^\d{9}$", ErrorMessage = "La cédula debe contener solo números y tener 9 dígitos.")]
    [Display(Name = "Cédula")]
    public string Cedula { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre completo es obligatorio.")]
    [RegularExpression(@".*\S.*", ErrorMessage = "El nombre completo no puede estar vacío.")]
    [StringLength(150, ErrorMessage = "El nombre completo no puede superar 150 caracteres.")]
    [Display(Name = "Nombre completo")]
    public string NombreCompleto { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    [RegularExpression(@".*\S.*", ErrorMessage = "El correo electrónico no puede estar vacío.")]
    [EmailAddress(ErrorMessage = "Ingresa un correo electrónico válido.")]
    [StringLength(150, ErrorMessage = "El correo electrónico no puede superar 150 caracteres.")]
    [Display(Name = "Correo electrónico")]
    public string Correo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña inicial es obligatoria.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña inicial")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "El rol es obligatorio.")]
    [Range(1, int.MaxValue, ErrorMessage = "Selecciona un rol válido.")]
    [Display(Name = "Rol")]
    public int RolId { get; set; }

    public IEnumerable<SelectListItem> Roles { get; set; } = Enumerable.Empty<SelectListItem>();
}
