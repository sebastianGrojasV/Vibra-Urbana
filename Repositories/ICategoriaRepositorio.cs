using VibraUrbana.Models;

namespace VibraUrbana.Repositories
{
    public interface ICategoriaRepositorio
    {
        Task<List<Categoria>> ObtenerCategoriasAsync();
        Task<Categoria?> ObtenerPorIdAsync(int id);
        Task<bool> AgregarCategoriaAsync(Categoria categoria);
        Task<bool> ActualizarCategoriaAsync(Categoria categoria);
        Task<bool> EliminarCategoriaAsync(int id); // Eliminado lógico
    }
}
