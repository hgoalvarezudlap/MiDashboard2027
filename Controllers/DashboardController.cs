using Dashboard.Data;
using Dashboard.Models;
using Dashboard.Services;
using Microsoft.AspNetCore.Mvc;

namespace Dashboard.Controllers;

public class DashboardController : Controller
{
    private readonly DashboardService _dashboard;
    private readonly RegistroRepository _registros;
    private readonly CategoriaRepository _categorias;
    private readonly PeriodoService _periodos;
    private readonly ExcelExportService _excel;

    public DashboardController(
        DashboardService dashboard,
        RegistroRepository registros,
        CategoriaRepository categorias,
        PeriodoService periodos,
        ExcelExportService excel)
    {
        _dashboard = dashboard;
        _registros = registros;
        _categorias = categorias;
        _periodos = periodos;
        _excel = excel;
    }

    [HttpGet("/Dashboard")]
    [HttpGet("/Dashboard/Index")]
    public async Task<IActionResult> Index(string? vista, DateOnly? fecha, int? categoriaId, int pagina = 1)
    {
        var model = await _dashboard.ConstruirAsync(vista, fecha, categoriaId, pagina);
        return View(model);
    }

    [HttpGet("/Dashboard/Exportar")]
    public async Task<IActionResult> Exportar(string? vista, DateOnly? fecha, int? categoriaId)
    {
        var periodo = _periodos.Calcular(PeriodoService.ParseVista(vista), fecha ?? _periodos.Hoy());
        string? nombreCategoria = null;
        if (categoriaId is not null)
        {
            nombreCategoria = (await _categorias.ObtenerAsync(categoriaId.Value))?.Nombre;
            if (nombreCategoria is null)
            {
                categoriaId = null;
            }
        }

        var registros = await _registros.ListarAsync(periodo.Desde, periodo.Hasta, categoriaId);
        var bytes = _excel.Generar(periodo, registros, nombreCategoria);
        var nombre = $"RegistroTiempo_{PeriodoService.VistaSlug(periodo.Vista)}_{periodo.Desde:yyyy-MM-dd}.xlsx";
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", nombre);
    }
}
