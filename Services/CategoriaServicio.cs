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

            categoria.Nombre = categoria.Nombre.Trim();
            categoria.Descripcion = categoria.Descripcion?.Trim() ?? string.Empty;
            if (await NombreDuplicadoAsync(categoria.Nombre, 0)) return false;
            return await _categoriaRepositorio.AgregarCategoriaAsync(categoria);
        }

        public async Task<bool> ActualizarCategoriaAsync(Categoria categoria)
        {
            categoria.Nombre = categoria.Nombre.Trim();
            categoria.Descripcion = categoria.Descripcion?.Trim() ?? string.Empty;
            if (await NombreDuplicadoAsync(categoria.Nombre, categoria.Id)) return false;
            return await _categoriaRepositorio.ActualizarCategoriaAsync(categoria);
        }

        private async Task<bool> NombreDuplicadoAsync(string nombre, int id)
        {
            var categorias = await _categoriaRepositorio.ObtenerCategoriasAsync();
            return categorias.Any(item => item.Id != id &&
                string.Equals(item.Nombre.Trim(), nombre, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<bool> EliminarCategoriaAsync(int id)
        {
            return await _categoriaRepositorio.EliminarCategoriaAsync(id);
        }

    }
}
