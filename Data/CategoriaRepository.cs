using System.Data;
using Dapper;
using Dashboard.Models;
using Microsoft.Data.SqlClient;

namespace Dashboard.Data;

public class CategoriaRepository
{
    private readonly IDbConnection _db;

    public CategoriaRepository(IDbConnection db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<Categoria>> ListarAsync()
    {
        const string sql = """
            SELECT c.CategoriaId, c.Nombre, c.Color, c.EsPredeterminada, c.FechaCreacion,
                   (SELECT COUNT(*) FROM dbo.RT_Registro r WHERE r.CategoriaId = c.CategoriaId) AS TotalRegistros
              FROM dbo.RT_Categoria c
             ORDER BY c.EsPredeterminada DESC, c.Nombre;
            """;
        var rows = await _db.QueryAsync<Categoria>(sql);
        return rows.ToList();
    }

    public Task<Categoria?> ObtenerAsync(int categoriaId)
    {
        const string sql = """
            SELECT c.CategoriaId, c.Nombre, c.Color, c.EsPredeterminada, c.FechaCreacion,
                   (SELECT COUNT(*) FROM dbo.RT_Registro r WHERE r.CategoriaId = c.CategoriaId) AS TotalRegistros
              FROM dbo.RT_Categoria c
             WHERE c.CategoriaId = @categoriaId;
            """;
        return _db.QuerySingleOrDefaultAsync<Categoria>(sql, new { categoriaId });
    }

    public Task<Categoria> ObtenerPredeterminadaAsync()
    {
        const string sql = """
            SELECT TOP 1 CategoriaId, Nombre, Color, EsPredeterminada, FechaCreacion
              FROM dbo.RT_Categoria
             WHERE EsPredeterminada = 1;
            """;
        return _db.QuerySingleAsync<Categoria>(sql);
    }

    public async Task<int> CrearAsync(string nombre, string color)
    {
        const string sql = """
            INSERT INTO dbo.RT_Categoria (Nombre, Color, EsPredeterminada)
            OUTPUT INSERTED.CategoriaId
            VALUES (@nombre, @color, 0);
            """;
        try
        {
            return await _db.ExecuteScalarAsync<int>(sql, new { nombre = nombre.Trim(), color = color.ToUpperInvariant() });
        }
        catch (SqlException ex) when (EsDuplicado(ex))
        {
            throw TraducirDuplicado(ex);
        }
    }

    public async Task ActualizarAsync(int categoriaId, string nombre, string color)
    {
        // El nombre de la categoría predeterminada no se modifica.
        const string sql = """
            UPDATE dbo.RT_Categoria
               SET Nombre = CASE WHEN EsPredeterminada = 1 THEN Nombre ELSE @nombre END,
                   Color  = @color
             WHERE CategoriaId = @categoriaId;
            """;
        try
        {
            await _db.ExecuteAsync(sql, new { categoriaId, nombre = nombre.Trim(), color = color.ToUpperInvariant() });
        }
        catch (SqlException ex) when (EsDuplicado(ex))
        {
            throw TraducirDuplicado(ex);
        }
    }

    public async Task EliminarAsync(int categoriaId)
    {
        try
        {
            await _db.ExecuteAsync(
                "dbo.RT_sp_EliminarCategoria",
                new { CategoriaId = categoriaId },
                commandType: CommandType.StoredProcedure);
        }
        catch (SqlException ex) when (ex.Number is 50001 or 50002)
        {
            throw new ReglaNegocioException(ex.Message);
        }
    }

    private static bool EsDuplicado(SqlException ex) => ex.Number is 2601 or 2627;

    private static DuplicadoException TraducirDuplicado(SqlException ex)
    {
        if (ex.Message.Contains("UX_RT_Categoria_Color", StringComparison.OrdinalIgnoreCase))
        {
            return new DuplicadoException("Color", "Ese color ya está en uso por otra categoría.");
        }

        return new DuplicadoException("Nombre", "Ya existe una categoría con ese nombre.");
    }
}
