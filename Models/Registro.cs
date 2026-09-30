namespace Dashboard.Models;

public class Registro
{
    public int RegistroId { get; set; }
    public string Tarea { get; set; } = "";
    public short Minutos { get; set; }
    public DateOnly Fecha { get; set; }
    public int CategoriaId { get; set; }
    public string? Comentarios { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }

    public string CategoriaNombre { get; set; } = "";
    public string CategoriaColor { get; set; } = "";
}
