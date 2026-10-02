# Registro de Tiempo

Aplicación web para registrar el tiempo de trabajo por tarea y categoría, y consultarlo por día, semana o mes.

## Páginas

| Ruta | Uso |
|---|---|
| `/` | Captura. Un registro a la vez. El tiempo inicia en 1 h. |
| `/Dashboard` | Total, distribución, horas por día y lista del periodo. |
| `/Categorias` | Alta, edición y baja de categorías. |
| `/Docs` | Guía de qué actividad corresponde a cada categoría. |

El acceso exige usuario y contraseña. La sesión dura 24 horas y se renueva si se sigue usando la aplicación.

## Reglas de un registro

- La tarea es obligatoria.
- El tiempo va de 15 minutos a 8 horas, en múltiplos de 15.
- La fecha usa la zona `America/Mexico_City`.
- Si no se elige categoría, el registro queda en **Sin categoría**. Esa categoría no se puede eliminar. Al borrar otra, sus registros pasan a ella.
- El nombre y el color de una categoría son únicos.

En el dashboard, la semana va de lunes a domingo. El total incluye el fin de semana; el desglose diario muestra solo lunes a viernes e indica las horas del fin de semana aparte. La lista de la semana y del mes muestra 10 registros por página. La exportación a Excel incluye todo el periodo visible, con el filtro de categoría si está activo.

## Requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Node.js 20 o superior
- SQL Server o Azure SQL con el esquema aplicado

## Configuración

La aplicación no arranca si falta el usuario, la contraseña o la cadena de conexión.

`appsettings.json` define `Auth:Username` y `Auth:Password`.

`appsettings.Development.json` define `ConnectionStrings:AzureSql`. Ese archivo solo se carga con `ASPNETCORE_ENVIRONMENT=Development`.

En Azure App Service el entorno es Production. Hay que crear esta variable de aplicación, con el valor de la cadena de conexión:

```text
ConnectionStrings__AzureSql
```

El doble guion bajo equivale a los dos puntos de la configuración. Sin esa variable el proceso termina al iniciar y el sitio responde 503.

## Base de datos

Ejecutar `sql/RT_schema.sql` una vez en la base de destino. El script crea `RT_Categoria`, `RT_Registro`, los índices y el procedimiento `RT_sp_EliminarCategoria`, e inserta **Sin categoría**.

El script activa `QUOTED_IDENTIFIER`, necesario para el índice filtrado de la categoría predeterminada.

## Ejecutar en local

```bash
npm install
dotnet run --launch-profile http
```

`npm install` deja las librerías de interfaz. Si existe `node_modules`, la compilación de .NET ejecuta `npm run build`: genera `wwwroot/css/site.css` y copia Alpine.js, htmx, ECharts y Tabulator a `wwwroot/lib`.

Con el perfil `http` la aplicación queda en `http://localhost:5077`.

## Interfaz

Tailwind CSS y daisyUI, con naranja UDLAP (`#F47A20`) como color principal. Alpine.js abre el menú en pantallas chicas. htmx actualiza la captura y los listados sin recargar toda la página. ECharts dibuja la distribución por categoría y las horas por día. El acceso a datos usa Dapper y la exportación usa ClosedXML.

## Publicación

El workflow `.github/workflows/main_dedpwudlap.yml` compila con .NET 10 y publica en el App Service `dedpwudlap` al empujar a `main`. El sitio debe usar la pila .NET 10 y tener configurada `ConnectionStrings__AzureSql`.
