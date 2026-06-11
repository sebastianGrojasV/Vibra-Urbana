using VibraUrbana.Models;

namespace VibraUrbana.Services
{
    public interface ICategoriaServicio
    {
        Task<List<Categoria>> ObtenerCategoriasAsync();
        Task<Categoria?> ObtenerPorIdAsync(int id);
        Task<bool> AgregarCategoriaAsync(Categoria categoria);
        Task<bool> ActualizarCategoriaAsync(Categoria categoria);
        Task<bool> EliminarCategoriaAsync(int id); // Eliminado lógico
    }
}
