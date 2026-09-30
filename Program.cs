using System.Data;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using Dapper;
using Dashboard.Data;
using Dashboard.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.Cookie.Name = "Dashboard.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

// Emitir acentos y caracteres latinos sin codificar (&#xED;) en el HTML.
builder.Services.AddWebEncoders(options =>
{
    options.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.BasicLatin, UnicodeRanges.Latin1Supplement, UnicodeRanges.GeneralPunctuation);
});

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AuthorizeFilter());
});

if (string.IsNullOrWhiteSpace(builder.Configuration["Auth:Username"])
    || string.IsNullOrWhiteSpace(builder.Configuration["Auth:Password"]))
{
    throw new InvalidOperationException(
        "Faltan 'Auth:Username' o 'Auth:Password' en la configuración.");
}

var azureSql = builder.Configuration.GetConnectionString("AzureSql");
if (string.IsNullOrWhiteSpace(azureSql))
{
    throw new InvalidOperationException(
        "Falta la cadena de conexión 'ConnectionStrings:AzureSql'.");
}

SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());

builder.Services.AddScoped<IDbConnection>(_ => new SqlConnection(azureSql));
builder.Services.AddScoped<CategoriaRepository>();
builder.Services.AddScoped<RegistroRepository>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddSingleton<PeriodoService>();
builder.Services.AddSingleton<ExcelExportService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Captura}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
