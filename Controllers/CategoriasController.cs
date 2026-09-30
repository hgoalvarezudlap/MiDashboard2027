using Dashboard.Data;
using Dashboard.Models;
using Microsoft.AspNetCore.Mvc;

namespace Dashboard.Controllers;

public class CategoriasController : Controller
{
    private readonly CategoriaRepository _categorias;

    public CategoriasController(CategoriaRepository categorias)
    {
        _categorias = categorias;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        ViewBag.Categorias = await _categorias.ListarAsync();
        return View(new CategoriaFormModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(CategoriaFormModel form)
    {
        if (ModelState.IsValid)
        {
            try
            {
                await _categorias.CrearAsync(form.Nombre, form.Color);
                ModelState.Clear();
                form = new CategoriaFormModel();
            }
            catch (DuplicadoException ex)
            {
                ModelState.AddModelError(ex.Campo, ex.Message);
            }
        }

        ViewBag.Categorias = await _categorias.ListarAsync();
        return PartialView("_Contenido", form);
    }

    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        var categoria = await _categorias.ObtenerAsync(id);
        if (categoria is null)
        {
            return NotFound();
        }

        return PartialView("_FormCategoria", new CategoriaFormModel
        {
            CategoriaId = categoria.CategoriaId,
            Nombre = categoria.Nombre,
            Color = categoria.Color,
            EsPredeterminada = categoria.EsPredeterminada,
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(CategoriaFormModel form)
    {
        if (form.CategoriaId is null)
        {
            return BadRequest();
        }

        var actual = await _categorias.ObtenerAsync(form.CategoriaId.Value);
        if (actual is null)
        {
            return NotFound();
        }

        form.EsPredeterminada = actual.EsPredeterminada;
        if (actual.EsPredeterminada)
        {
            // El nombre de "Sin categoría" no se edita.
            form.Nombre = actual.Nombre;
            ModelState.Remove(nameof(form.Nombre));
        }

        if (!ModelState.IsValid)
        {
            return PartialView("_FormCategoria", form);
        }

        try
        {
            await _categorias.ActualizarAsync(form.CategoriaId.Value, form.Nombre, form.Color);
        }
        catch (DuplicadoException ex)
        {
            ModelState.AddModelError(ex.Campo, ex.Message);
            return PartialView("_FormCategoria", form);
        }

        ViewBag.Categorias = await _categorias.ListarAsync();
        return PartialView("_ContenidoOob", new CategoriaFormModel());
    }

    [HttpGet]
    public async Task<IActionResult> ConfirmarEliminar(int id)
    {
        var categoria = await _categorias.ObtenerAsync(id);
        if (categoria is null)
        {
            return NotFound();
        }

        return PartialView("_ConfirmarEliminar", categoria);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int id)
    {
        try
        {
            await _categorias.EliminarAsync(id);
        }
        catch (ReglaNegocioException ex)
        {
            TempData["Error"] = ex.Message;
        }

        ViewBag.Categorias = await _categorias.ListarAsync();
        return PartialView("_ContenidoOob", new CategoriaFormModel());
    }
}
