using System.Data;
using Dapper;
using Dashboard.Models;

namespace Dashboard.Data;

public class RegistroRepository
{
    private readonly IDbConnection _db;

    public RegistroRepository(IDbConnection db)
    {
        _db = db;
    }

    private const string SelectRegistro = """
        SELECT r.RegistroId, r.Tarea, r.Minutos, r.Fecha, r.CategoriaId, r.Comentarios,
               r.FechaCreacion, r.FechaModificacion,
               c.Nombre AS CategoriaNombre, c.Color AS CategoriaColor
          FROM dbo.RT_Registro r
          JOIN dbo.RT_Categoria c ON c.CategoriaId = r.CategoriaId
        """;

    public Task<Registro?> ObtenerAsync(int registroId)
    {
        return _db.QuerySingleOrDefaultAsync<Registro>(
            SelectRegistro + " WHERE r.RegistroId = @registroId;",
            new { registroId });
    }

    public async Task<IReadOnlyList<Registro>> ListarAsync(DateOnly desde, DateOnly hasta, int? categoriaId)
    {
        var sql = SelectRegistro + """
             WHERE r.Fecha BETWEEN @desde AND @hasta
               AND (@categoriaId IS NULL OR r.CategoriaId = @categoriaId)
             ORDER BY r.Fecha DESC, r.FechaCreacion DESC, r.RegistroId DESC;
            """;
        var rows = await _db.QueryAsync<Registro>(sql, new { desde, hasta, categoriaId });
        return rows.ToList();
    }

    public Task<int> ContarAsync(DateOnly desde, DateOnly hasta, int? categoriaId)
    {
        const string sql = """
            SELECT COUNT(*)
              FROM dbo.RT_Registro
             WHERE Fecha BETWEEN @desde AND @hasta
               AND (@categoriaId IS NULL OR CategoriaId = @categoriaId);
            """;
        return _db.ExecuteScalarAsync<int>(sql, new { desde, hasta, categoriaId });
    }

    public async Task<IReadOnlyList<Registro>> ListarPaginaAsync(
        DateOnly desde, DateOnly hasta, int? categoriaId, int offset, int tamano)
    {
        var sql = SelectRegistro + """
             WHERE r.Fecha BETWEEN @desde AND @hasta
               AND (@categoriaId IS NULL OR r.CategoriaId = @categoriaId)
             ORDER BY r.Fecha DESC, r.FechaCreacion DESC, r.RegistroId DESC
             OFFSET @offset ROWS FETCH NEXT @tamano ROWS ONLY;
            """;
        var rows = await _db.QueryAsync<Registro>(sql, new { desde, hasta, categoriaId, offset, tamano });
        return rows.ToList();
    }

    public Task<int> TotalMinutosAsync(DateOnly desde, DateOnly hasta, int? categoriaId)
    {
        const string sql = """
            SELECT ISNULL(SUM(Minutos), 0)
              FROM dbo.RT_Registro
             WHERE Fecha BETWEEN @desde AND @hasta
               AND (@categoriaId IS NULL OR CategoriaId = @categoriaId);
            """;
        return _db.ExecuteScalarAsync<int>(sql, new { desde, hasta, categoriaId });
    }

    public async Task<IReadOnlyList<DistribucionCategoria>> DistribucionAsync(DateOnly desde, DateOnly hasta, int? categoriaId)
    {
        const string sql = """
            SELECT c.CategoriaId, c.Nombre, c.Color, SUM(r.Minutos) AS Minutos
              FROM dbo.RT_Registro r
              JOIN dbo.RT_Categoria c ON c.CategoriaId = r.CategoriaId
             WHERE r.Fecha BETWEEN @desde AND @hasta
               AND (@categoriaId IS NULL OR r.CategoriaId = @categoriaId)
             GROUP BY c.CategoriaId, c.Nombre, c.Color
             ORDER BY SUM(r.Minutos) DESC, c.Nombre;
            """;
        var rows = (await _db.QueryAsync<DistribucionCategoria>(sql, new { desde, hasta, categoriaId })).ToList();
        var total = rows.Sum(r => r.Minutos);
        foreach (var row in rows)
        {
            row.Porcentaje = total == 0 ? 0 : Math.Round(row.Minutos * 100m / total, 1);
        }
        return rows;
    }

    public async Task<IReadOnlyList<TotalPorDia>> TotalesPorDiaAsync(DateOnly desde, DateOnly hasta, int? categoriaId)
    {
        const string sql = """
            SELECT Fecha, SUM(Minutos) AS Minutos
              FROM dbo.RT_Registro
             WHERE Fecha BETWEEN @desde AND @hasta
               AND (@categoriaId IS NULL OR CategoriaId = @categoriaId)
             GROUP BY Fecha
             ORDER BY Fecha;
            """;
        var rows = await _db.QueryAsync<TotalPorDia>(sql, new { desde, hasta, categoriaId });
        return rows.ToList();
    }

    public async Task<IReadOnlyList<string>> SugerenciasAsync(string texto, int max = 10)
    {
        const string sql = """
            SELECT DISTINCT TOP (@max) Tarea
              FROM dbo.RT_Registro
             WHERE Tarea LIKE @patron
             ORDER BY Tarea;
            """;
        var patron = texto.Trim().Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]") + "%";
        var rows = await _db.QueryAsync<string>(sql, new { patron, max });
        return rows.ToList();
    }

    public Task<int> CrearAsync(string tarea, int minutos, DateOnly fecha, int categoriaId, string? comentarios)
    {
        const string sql = """
            INSERT INTO dbo.RT_Registro (Tarea, Minutos, Fecha, CategoriaId, Comentarios)
            OUTPUT INSERTED.RegistroId
            VALUES (@tarea, @minutos, @fecha, @categoriaId, @comentarios);
            """;
        return _db.ExecuteScalarAsync<int>(sql, new
        {
            tarea = tarea.Trim(),
            minutos = (short)minutos,
            fecha,
            categoriaId,
            comentarios = string.IsNullOrWhiteSpace(comentarios) ? null : comentarios.Trim(),
        });
    }

    public Task<int> ActualizarAsync(int registroId, string tarea, int minutos, DateOnly fecha, int categoriaId, string? comentarios)
    {
        const string sql = """
            UPDATE dbo.RT_Registro
               SET Tarea = @tarea,
                   Minutos = @minutos,
                   Fecha = @fecha,
                   CategoriaId = @categoriaId,
                   Comentarios = @comentarios,
                   FechaModificacion = SYSUTCDATETIME()
             WHERE RegistroId = @registroId;
            """;
        return _db.ExecuteAsync(sql, new
        {
            registroId,
            tarea = tarea.Trim(),
            minutos = (short)minutos,
            fecha,
            categoriaId,
            comentarios = string.IsNullOrWhiteSpace(comentarios) ? null : comentarios.Trim(),
        });
    }

    public Task<int> EliminarAsync(int registroId)
    {
        return _db.ExecuteAsync("DELETE FROM dbo.RT_Registro WHERE RegistroId = @registroId;", new { registroId });
    }
}
