using VibraUrbana.Models;
using VibraUrbana.Repositories;

namespace VibraUrbana.Services
{
    public class CategoriaServicio : ICategoriaServicio
    {
        private readonly ICategoriaRepositorio _categoriaRepositorio;

        public CategoriaServicio(ICategoriaRepositorio categoriaRepositorio)
        {
            _categoriaRepositorio = categoriaRepositorio;
        }

        public async Task<List<Categoria>> ObtenerCategoriasAsync()
        {
            return await _categoriaRepositorio.ObtenerCategoriasAsync();
        }

        public async Task<Categoria?> ObtenerPorIdAsync(int id)
        {
            return await _categoriaRepositorio.ObtenerPorIdAsync(id);
        }

        public async Task<bool> AgregarCategoriaAsync(Categoria categoria)
        {

            return await _categoriaRepositorio.AgregarCategoriaAsync(categoria);
        }

        public async Task<bool> ActualizarCategoriaAsync(Categoria categoria)
        {
            return await _categoriaRepositorio.ActualizarCategoriaAsync(categoria);
        }

        public async Task<bool> EliminarCategoriaAsync(int id)
        {
            return await _categoriaRepositorio.EliminarCategoriaAsync(id);
        }

    }
}
