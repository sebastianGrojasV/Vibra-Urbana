using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using VibraUrbana.Models;
using VibraUrbana.Services;

namespace VibraUrbana.Controllers
{
    [Authorize(Policy = PermisosSistema.ProductosGestionar)]
    public class CategoriaController : Controller
    {
        private readonly ICategoriaServicio _categoriaServicio;

        public CategoriaController(ICategoriaServicio categoriaServicio)
        {
            _categoriaServicio = categoriaServicio;
        }

        public async Task<IActionResult> Index()
        {
            var categorias = await _categoriaServicio.ObtenerCategoriasAsync();
            return View(categorias);
        }

        public async Task<IActionResult> Details(int id)
        {
            var categoria = await _categoriaServicio.ObtenerPorIdAsync(id);
            if (categoria == null)
                return NotFound();

            return View(categoria);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Categoria categoria)
        {
            if (ModelState.IsValid)
            {
                var creada = await _categoriaServicio.AgregarCategoriaAsync(categoria);
                if (creada)
                {
                    TempData["SuccessMessage"] = "La categoría se creó correctamente";
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError("Nombre", "Ya existe una categoría con ese nombre.");
            }

            return View(categoria);
        }


        public async Task<IActionResult> Edit(int id)
        {
            var categoria = await _categoriaServicio.ObtenerPorIdAsync(id);
            if (categoria == null)
                return NotFound();

            return View(categoria);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Categoria categoria)
        {
            if (ModelState.IsValid)
            {
                var actualizada = await _categoriaServicio.ActualizarCategoriaAsync(categoria);
                if (actualizada)
                {
                    TempData["SuccessMessage"] = "La categoría se actualizó correctamente";
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("Nombre", "Ya existe una categoría con ese nombre. Elige otro nombre.");
            }
            return View(categoria);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var categoria = await _categoriaServicio.ObtenerPorIdAsync(id);
            if (categoria == null)
                return NotFound();

            return View(categoria);
        }

        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var eliminada = await _categoriaServicio.EliminarCategoriaAsync(id);

            if (eliminada)
            {
                TempData["SuccessMessage"] = "La categoría fue desactivada correctamente";
            }
            else
            {
                TempData["ErrorMessage"] = "No se pudo desactivar la categoría";
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
                TempData["ErrorMessage"] = "No se encontró la categoría.";
                return RedirectToAction(nameof(Index));
            }

            categoria.Activo = active;
            var actualizado = await _categoriaServicio.ActualizarCategoriaAsync(categoria);

            if (actualizado)
            {
                TempData["SuccessMessage"] = active
                    ? "La categoría fue activada correctamente"
                    : "La categoría fue desactivada correctamente";
            }
            else
            {
                TempData["ErrorMessage"] = "No se pudo cambiar el estado de la categoría.";
            }

            return RedirectToAction(nameof(Index));
        }




    }
}
