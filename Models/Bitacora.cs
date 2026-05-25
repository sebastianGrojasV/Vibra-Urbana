namespace VibraUrbana.Models;

public class Bitacora
{
    public int Id { get; set; }

    public int? UsuarioId { get; set; }

    public Usuario? Usuario { get; set; }

    public string Modulo { get; set; } = string.Empty;

    public string Accion { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;

    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
}
