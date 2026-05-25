namespace VibraUrbana.Models;

public class MovimientoInventario
{
    public int Id { get; set; }

    public int ProductoId { get; set; }

    public Producto Producto { get; set; } = null!;

    public string TipoMovimiento { get; set; } = string.Empty;

    public int Cantidad { get; set; }

    public int CantidadAnterior { get; set; }

    public int CantidadNueva { get; set; }

    public string Motivo { get; set; } = string.Empty;

    public DateTime FechaMovimiento { get; set; } = DateTime.UtcNow;

    public int? UsuarioId { get; set; }

    public Usuario? Usuario { get; set; }
}
