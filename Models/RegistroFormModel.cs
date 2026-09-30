using System.ComponentModel.DataAnnotations;

namespace Dashboard.Models;

public class RegistroFormModel : IValidatableObject
{
    public int? RegistroId { get; set; }

    [Required(ErrorMessage = "Escribe el nombre de la tarea.")]
    [StringLength(200, ErrorMessage = "La tarea no puede exceder 200 caracteres.")]
    [Display(Name = "Tarea")]
    public string Tarea { get; set; } = "";

    [Required(ErrorMessage = "Indica el tiempo invertido.")]
    [Range(15, 480, ErrorMessage = "El tiempo debe estar entre 15 minutos y 8 horas.")]
    [Display(Name = "Tiempo")]
    public int? Minutos { get; set; }

    [Required(ErrorMessage = "Indica la fecha.")]
    [DataType(DataType.Date)]
    [Display(Name = "Fecha")]
    public DateOnly? Fecha { get; set; }

    [Display(Name = "Categoría")]
    public int? CategoriaId { get; set; }

    [StringLength(1000, ErrorMessage = "Los comentarios no pueden exceder 1000 caracteres.")]
    [Display(Name = "Comentarios")]
    public string? Comentarios { get; set; }

    // Contexto del dashboard para volver a la misma vista tras guardar.
    public string? Vista { get; set; }
    public DateOnly? FechaVista { get; set; }
    public int? FiltroCategoriaId { get; set; }
    public int Pagina { get; set; } = 1;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Tarea))
        {
            yield return new ValidationResult("El nombre de la tarea no puede estar vacío.", [nameof(Tarea)]);
        }

        if (Minutos is int m && m % 15 != 0)
        {
            yield return new ValidationResult("El tiempo debe ser múltiplo de 15 minutos.", [nameof(Minutos)]);
        }
    }
}
