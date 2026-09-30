using Dashboard.Data;
using Dashboard.Models;
using Dashboard.Services;
using Microsoft.AspNetCore.Mvc;

namespace Dashboard.Controllers;

public class RegistrosController : Controller
{
    private const string VistaContenido = "~/Views/Dashboard/_Contenido.cshtml";
    private const string VistaContenidoOob = "~/Views/Dashboard/_ContenidoOob.cshtml";

    private readonly RegistroRepository _registros;
    private readonly CategoriaRepository _categorias;
    private readonly DashboardService _dashboard;

    public RegistrosController(RegistroRepository registros, CategoriaRepository categorias, DashboardService dashboard)
    {
        _registros = registros;
        _categorias = categorias;
        _dashboard = dashboard;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(RegistroFormModel form)
    {
        ViewBag.Categorias = await _categorias.ListarAsync();

        if (!ModelState.IsValid)
        {
            return VistaCaptura(form);
        }

        var categoriaId = await ResolverCategoriaAsync(form.CategoriaId);
        await _registros.CrearAsync(form.Tarea, form.Minutos!.Value, form.Fecha!.Value, categoriaId, form.Comentarios);

        var tarea = form.Tarea.Trim();
        var minutos = form.Minutos!.Value;
        var fecha = form.Fecha!.Value;
        var exito = $"{tarea} · {TiempoFormato.Horas(minutos)}";
        var enlaceDia = Url.Action("Index", "Dashboard", new { vista = "dia", fecha = fecha.ToString("yyyy-MM-dd") });
        ModelState.Clear();

        if (!EsHtmx())
        {
            TempData["Exito"] = exito;
            TempData["EnlaceDia"] = enlaceDia;
            TempData["Fecha"] = fecha.ToString("yyyy-MM-dd");
            if (form.CategoriaId is int id)
            {
                TempData["CategoriaId"] = id;
            }

            return RedirectToAction("Index", "Captura");
        }

        ViewBag.Exito = exito;
        ViewBag.EnlaceDia = enlaceDia;
        return PartialView("~/Views/Captura/_Formulario.cshtml", new RegistroFormModel
        {
            Fecha = fecha,
            CategoriaId = form.CategoriaId,
        });
    }

    [HttpGet]
    public async Task<IActionResult> Editar(int id, string? vista, DateOnly? fecha, int? categoriaId, int pagina = 1)
    {
        var registro = await _registros.ObtenerAsync(id);
        if (registro is null)
        {
            return NotFound();
        }

        var form = new RegistroFormModel
        {
            RegistroId = registro.RegistroId,
            Tarea = registro.Tarea,
            Minutos = registro.Minutos,
            Fecha = registro.Fecha,
            CategoriaId = registro.CategoriaId,
            Comentarios = registro.Comentarios,
            Vista = vista,
            FechaVista = fecha,
            FiltroCategoriaId = categoriaId,
            Pagina = pagina,
        };
        ViewBag.Categorias = await _categorias.ListarAsync();
        return PartialView("_FormRegistro", form);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(RegistroFormModel form)
    {
        if (form.RegistroId is null)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Categorias = await _categorias.ListarAsync();
            return PartialView("_FormRegistro", form);
        }

        var categoriaId = await ResolverCategoriaAsync(form.CategoriaId);
        var filas = await _registros.ActualizarAsync(
            form.RegistroId.Value, form.Tarea, form.Minutos!.Value, form.Fecha!.Value, categoriaId, form.Comentarios);
        if (filas == 0)
        {
            return NotFound();
        }

        var model = await _dashboard.ConstruirAsync(form.Vista, form.FechaVista, form.FiltroCategoriaId, form.Pagina);
        return PartialView(VistaContenidoOob, model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int id, string? vista, DateOnly? fecha, int? categoriaId, int pagina = 1)
    {
        await _registros.EliminarAsync(id);
        var model = await _dashboard.ConstruirAsync(vista, fecha, categoriaId, pagina);
        return PartialView(VistaContenido, model);
    }

    [HttpGet]
    public async Task<IActionResult> Sugerencias(string? tarea)
    {
        IReadOnlyList<string> sugerencias = string.IsNullOrWhiteSpace(tarea)
            ? []
            : await _registros.SugerenciasAsync(tarea);
        return PartialView("_Sugerencias", sugerencias);
    }

    private bool EsHtmx() => Request.Headers.ContainsKey("HX-Request");

    private IActionResult VistaCaptura(RegistroFormModel form)
    {
        if (EsHtmx())
        {
            return PartialView("~/Views/Captura/_Formulario.cshtml", form);
        }

        ViewBag.Hoy = form.Fecha;
        return View("~/Views/Captura/Index.cshtml", form);
    }

    private async Task<int> ResolverCategoriaAsync(int? categoriaId)
    {
        if (categoriaId is int id && await _categorias.ObtenerAsync(id) is not null)
        {
            return id;
        }

        return (await _categorias.ObtenerPredeterminadaAsync()).CategoriaId;
    }
}
