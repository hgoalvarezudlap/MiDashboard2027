using System.Globalization;
using Dashboard.Data;
using Dashboard.Models;
using Dashboard.Services;
using Microsoft.AspNetCore.Mvc;

namespace Dashboard.Controllers;

public class CapturaController : Controller
{
    private readonly CategoriaRepository _categorias;
    private readonly PeriodoService _periodos;

    public CapturaController(CategoriaRepository categorias, PeriodoService periodos)
    {
        _categorias = categorias;
        _periodos = periodos;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        ViewBag.Categorias = await _categorias.ListarAsync();

        var hoy = _periodos.Hoy();
        var fecha = hoy;
        if (TempData["Fecha"] is string texto && DateOnly.TryParse(texto, CultureInfo.InvariantCulture, out var conservada))
        {
            fecha = conservada;
        }

        int? categoriaId = TempData["CategoriaId"] switch
        {
            int id => id,
            string s when int.TryParse(s, out var id) => id,
            _ => null,
        };

        ViewBag.Hoy = hoy;
        ViewBag.Exito = TempData["Exito"] as string;
        ViewBag.EnlaceDia = TempData["EnlaceDia"] as string;
        return View(new RegistroFormModel { Fecha = fecha, CategoriaId = categoriaId });
    }
}
