using Microsoft.AspNetCore.Mvc;
using VibraUrbana.Models;
using VibraUrbana.Services;

namespace VibraUrbana.Controllers
{
    public class CategoriaController : Controller
    {
        private readonly ICategoriaServicio _categoriaServicio;

        public CategoriaController(ICategoriaServicio categoriaServicio)
        {
            _categoriaServicio = categoriaServicio;
        }

        // GET: Categoria
        public async Task<IActionResult> Index()
        {
            var categorias = await _categoriaServicio.ObtenerCategoriasAsync();
            return View(categorias); // Vista Index.cshtml
        }

        // GET: Categoria/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var categoria = await _categoriaServicio.ObtenerPorIdAsync(id);
            if (categoria == null)
                return NotFound();

            return View(categoria); // Vista Details.cshtml
        }

        // GET: Categoria/Create
        public IActionResult Create()
        {
            return View(); // Vista Create.cshtml con formulario
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Categoria categoria)
        {
            if (ModelState.IsValid)
            {
                // 🔹 Validar si ya existe una categoría con el mismo nombre
                var categorias = await _categoriaServicio.ObtenerCategoriasAsync();
                if (categorias.Any(c => c.Nombre.ToLower() == categoria.Nombre.ToLower()))
                {
                    ModelState.AddModelError("Nombre", "Ya existe una categoría con ese nombre");
                    return View(categoria);
                }

                var creada = await _categoriaServicio.AgregarCategoriaAsync(categoria);
                if (creada)
                {
                    TempData["CategoriaCreada"] = " La categoría se creó correctamente";
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError(string.Empty, "Error al crear la categoría");
            }

            return View(categoria);
        }


        // GET: Categoria/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var categoria = await _categoriaServicio.ObtenerPorIdAsync(id);
            if (categoria == null)
                return NotFound();

            return View(categoria); // Vista Edit.cshtml
        }

        // POST: Categoria/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Categoria categoria)
        {
            if (ModelState.IsValid)
            {
                var actualizada = await _categoriaServicio.ActualizarCategoriaAsync(categoria);
                if (actualizada)
                {
                    TempData["CategoriaActualizada"] = "✏️ La categoría se actualizó correctamente";
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError(string.Empty, "Error al actualizar la categoría");
            }
            return View(categoria);
        }

        // GET: Categoria/Delete/5 → Vista de confirmación
        public async Task<IActionResult> Delete(int id)
        {
            var categoria = await _categoriaServicio.ObtenerPorIdAsync(id);
            if (categoria == null)
                return NotFound();

            return View(categoria); // Vista Delete.cshtml
        }

        // POST: Categoria/DeleteConfirmed/5 → Desactivación lógica
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var eliminada = await _categoriaServicio.EliminarCategoriaAsync(id);

            if (eliminada)
            {
                TempData["CategoriaEliminada"] = "🛑 La categoría fue desactivada correctamente";
            }
            else
            {
                TempData["CategoriaError"] = "⚠️ No se pudo desactivar la categoría";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarCambiarEstado(int id, bool active)
        {
            var categoria = await _categoriaServicio.ObtenerPorIdAsync(id);
            if (categoria == null)
            {
                TempData["CategoriaError"] = "⚠️ No se encontró la categoría.";
                return RedirectToAction(nameof(Index));
            }

            categoria.Activo = active;
            var actualizado = await _categoriaServicio.ActualizarCategoriaAsync(categoria);

            if (actualizado)
            {
                TempData["CategoriaActualizada"] = active
                    ? "✅ La categoría fue activada correctamente"
                    : "🛑 La categoría fue desactivada correctamente";
            }
            else
            {
                TempData["CategoriaError"] = "⚠️ No se pudo cambiar el estado de la categoría.";
            }

            return RedirectToAction(nameof(Index));
        }




    }
}
