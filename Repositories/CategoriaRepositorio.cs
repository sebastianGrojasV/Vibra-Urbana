using Microsoft.EntityFrameworkCore;
using VibraUrbana.Data;
using VibraUrbana.Models;

namespace VibraUrbana.Repositories
{
    public class CategoriaRepositorio : ICategoriaRepositorio
    {
        private readonly ApplicationDbContext _context;

        public CategoriaRepositorio(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Categoria>> ObtenerCategoriasAsync()
        {
            return await _context.Categorias.ToListAsync();
        }

        public async Task<Categoria?> ObtenerPorIdAsync(int id)
        {
            return await _context.Categorias.FindAsync(id);
        }

        public async Task<bool> AgregarCategoriaAsync(Categoria categoria)
        {
            _context.Categorias.Add(categoria);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ActualizarCategoriaAsync(Categoria categoria)
        {
            _context.Categorias.Update(categoria);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> EliminarCategoriaAsync(int id)
        {
            var categoria = await _context.Categorias.FindAsync(id);
            if (categoria == null) return false;

            categoria.Activo = false; // 🔹 Eliminado lógico
            _context.Categorias.Update(categoria);
            await _context.SaveChangesAsync();
            return true;
        }


    }
}
