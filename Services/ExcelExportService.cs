using ClosedXML.Excel;
using Dashboard.Models;

namespace Dashboard.Services;

public class ExcelExportService
{
    public byte[] Generar(Periodo periodo, IReadOnlyList<Registro> registros, string? filtroCategoria)
    {
        using var libro = new XLWorkbook();
        var hoja = libro.Worksheets.Add("Registros");

        hoja.Cell(1, 1).Value = "Registro de Tiempo";
        hoja.Cell(1, 1).Style.Font.Bold = true;
        hoja.Cell(1, 1).Style.Font.FontSize = 14;
        hoja.Cell(2, 1).Value = periodo.Etiqueta + (filtroCategoria is null ? "" : $" · Categoría: {filtroCategoria}");

        const int filaEncabezado = 4;
        string[] columnas = ["Fecha", "Tarea", "Categoría", "Tiempo (horas)", "Comentarios"];
        for (var i = 0; i < columnas.Length; i++)
        {
            var celda = hoja.Cell(filaEncabezado, i + 1);
            celda.Value = columnas[i];
            celda.Style.Font.Bold = true;
            celda.Style.Font.FontColor = XLColor.White;
            celda.Style.Fill.BackgroundColor = XLColor.FromHtml("#F47A20");
        }

        var fila = filaEncabezado + 1;
        foreach (var r in registros.OrderBy(r => r.Fecha).ThenBy(r => r.FechaCreacion))
        {
            hoja.Cell(fila, 1).Value = r.Fecha.ToDateTime(TimeOnly.MinValue);
            hoja.Cell(fila, 1).Style.DateFormat.Format = "dd/MM/yyyy";
            hoja.Cell(fila, 2).Value = r.Tarea;
            hoja.Cell(fila, 3).Value = r.CategoriaNombre;
            hoja.Cell(fila, 4).Value = r.Minutos / 60.0;
            hoja.Cell(fila, 4).Style.NumberFormat.Format = "0.00";
            hoja.Cell(fila, 5).Value = r.Comentarios ?? "";
            fila++;
        }

        if (registros.Count > 0)
        {
            hoja.Cell(fila, 3).Value = "Total";
            hoja.Cell(fila, 3).Style.Font.Bold = true;
            hoja.Cell(fila, 4).FormulaA1 = $"SUM(D{filaEncabezado + 1}:D{fila - 1})";
            hoja.Cell(fila, 4).Style.NumberFormat.Format = "0.00";
            hoja.Cell(fila, 4).Style.Font.Bold = true;
        }

        hoja.Columns().AdjustToContents();
        hoja.Column(5).Width = Math.Min(hoja.Column(5).Width, 60);
        hoja.SheetView.FreezeRows(filaEncabezado);

        using var ms = new MemoryStream();
        libro.SaveAs(ms);
        return ms.ToArray();
    }
}
