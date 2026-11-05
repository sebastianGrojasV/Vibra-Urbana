using Microsoft.EntityFrameworkCore;
using VibraUrbana.Data;
using Microsoft.Data.SqlClient;
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
            return await GuardarAsync();
        }

        public async Task<bool> ActualizarCategoriaAsync(Categoria categoria)
        {
            var existente = await _context.Categorias.FindAsync(categoria.Id);
            if (existente is null) return false;
            existente.Nombre = categoria.Nombre;
            existente.Descripcion = categoria.Descripcion;
            existente.Activo = categoria.Activo;
            return await GuardarAsync();
        }

        private async Task<bool> GuardarAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException exception) when
                (exception.InnerException is SqlException { Number: 2601 or 2627 })
            {
                return false;
            }
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
