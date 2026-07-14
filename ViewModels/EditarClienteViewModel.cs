using System.ComponentModel.DataAnnotations;

namespace VibraUrbana.ViewModels;

public class EditarClienteViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "La identificación es obligatoria.")]
    [RegularExpression(@"^\d{9,12}$", ErrorMessage = "La identificación debe contener solo números, entre 9 y 12 dígitos.")]
    [Display(Name = "Identificación")]
    public string Identificacion { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre completo es obligatorio.")]
    [RegularExpression(@".*\S.*", ErrorMessage = "El nombre completo no puede estar vacío.")]
    [StringLength(150, ErrorMessage = "El nombre completo no puede superar 150 caracteres.")]
    [Display(Name = "Nombre completo")]
    public string NombreCompleto { get; set; } = string.Empty;

    [Required(ErrorMessage = "El teléfono es obligatorio.")]
    [RegularExpression(@"^[245678]\d{7}$", ErrorMessage = "El teléfono debe tener 8 dígitos válidos de Costa Rica.")]
    [Display(Name = "Teléfono")]
    public string Telefono { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    [RegularExpression(@".*\S.*", ErrorMessage = "El correo electrónico no puede estar vacío.")]
    [EmailAddress(ErrorMessage = "Ingresa un correo electrónico válido.")]
    [StringLength(150, ErrorMessage = "El correo electrónico no puede superar 150 caracteres.")]
    [Display(Name = "Correo electrónico")]
    public string Correo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La dirección es obligatoria.")]
    [RegularExpression(@".*\S.*", ErrorMessage = "La dirección no puede estar vacía.")]
    [StringLength(300, ErrorMessage = "La dirección no puede superar 300 caracteres.")]
    [Display(Name = "Dirección")]
    public string Direccion { get; set; } = string.Empty;

    [Display(Name = "Activo")]
    public bool Activo { get; set; }
}
