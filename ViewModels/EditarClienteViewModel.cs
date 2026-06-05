using System.ComponentModel.DataAnnotations;

namespace VibraUrbana.ViewModels;

public class EditarClienteViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "La identificación es obligatoria.")]
    [StringLength(30, ErrorMessage = "La identificación no puede superar 30 caracteres.")]
    [Display(Name = "Identificación")]
    public string Identificacion { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre completo es obligatorio.")]
    [StringLength(150, ErrorMessage = "El nombre completo no puede superar 150 caracteres.")]
    [Display(Name = "Nombre completo")]
    public string NombreCompleto { get; set; } = string.Empty;

    [StringLength(30, ErrorMessage = "El teléfono no puede superar 30 caracteres.")]
    [Display(Name = "Teléfono")]
    public string Telefono { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Ingresa un correo electrónico válido.")]
    [StringLength(150, ErrorMessage = "El correo electrónico no puede superar 150 caracteres.")]
    [Display(Name = "Correo electrónico")]
    public string Correo { get; set; } = string.Empty;

    [StringLength(300, ErrorMessage = "La dirección no puede superar 300 caracteres.")]
    [Display(Name = "Dirección")]
    public string Direccion { get; set; } = string.Empty;

    [Display(Name = "Activo")]
    public bool Activo { get; set; }
}
