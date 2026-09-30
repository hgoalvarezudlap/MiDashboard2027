using Microsoft.AspNetCore.Mvc;

namespace Dashboard.Controllers;

public class DocsController : Controller
{
    public record GuiaCategoria(string Nombre, string Incluye);

    public static readonly IReadOnlyList<GuiaCategoria> Categorias =
    [
        new("Comunicación y reuniones", "Correo electrónico, reuniones de trabajo, seguimiento y 1:1 con el equipo."),
        new("Planeación y gestión", "Planeación de actividades, priorización, seguimiento de proyectos, trámites y procesos administrativos."),
        new("Desarrollo de aplicaciones", "Programar, revisar código, despliegues, CI/CD."),
        new("Infraestructura y soporte", "Administración de servidores, accesos, monitoreo, incidentes y soporte a usuarios. El soporte a personas invidentes se registra como subcategoría."),
        new("Análisis y reportes", "Análisis de información, generar reportes y documentación de procesos o sistemas."),
        new("Generación de contenido", "Textos, blog, newsletter y material de difusión."),
        new("Aprendizaje e investigación", "Capacitación, investigación y lectura de información."),
        new("Docencia", "Clases, preparación y revisión de trabajos. Fundamentos de mercadotecnia se registra como subcategoría de clases."),
        new("Otros", "Lo que no encaje en las anteriores."),
    ];

    [HttpGet("/Docs")]
    public IActionResult Index() => View(Categorias);
}
