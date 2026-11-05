using System.ComponentModel.DataAnnotations;

namespace VibraUrbana.Models;

public class Permiso
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre del permiso es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre del permiso no puede superar 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "La descripción del permiso es obligatoria.")]
    [StringLength(250, ErrorMessage = "La descripción del permiso no puede superar 250 caracteres.")]
    public string Descripcion { get; set; } = string.Empty;

    public bool Activo { get; set; } = true;

    public ICollection<RolPermiso> RolPermisos { get; set; } = new List<RolPermiso>();
}
