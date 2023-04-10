using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text.Json.Serialization;
using TareaMVC;
using TareaMVC.Servicios;

var builder = WebApplication.CreateBuilder(args);
var politicasUsuariosAutenticados = new AuthorizationPolicyBuilder()
                                .RequireAuthenticatedUser()
                                .Build();

// Add services to the container.
builder.Services.AddControllersWithViews(opciones =>
            opciones.Filters.Add(new AuthorizeFilter(politicasUsuariosAutenticados))
).AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
.AddDataAnnotationsLocalization(opciones =>
{
    opciones.DataAnnotationLocalizerProvider = (_, factoria) =>
    factoria.Create(typeof(RecursoCompartido));
}).AddJsonOptions(opciones =>
{
    opciones.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});
builder.Services.AddDbContext<ApplicationDbContext>(optiones =>
optiones.UseSqlServer("name=DefaultConnection"));

builder.Services.AddAuthentication().AddMicrosoftAccount(optiones =>
{
    optiones.ClientId = builder.Configuration["MicrosoftClientId"];
    optiones.ClientSecret = builder.Configuration["MicrosoftSecretId"];
});

builder.Services.AddTransient<IServicioUsuario, ServicioUsuario>();
builder.Services.AddTransient<IAlmacenadorArchivos, AlmacenadorArchivosAzure>();
builder.Services.AddTransient<IAlmacenadorArchivos, AlmacenadorArchivosLocal>();

builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddIdentity<IdentityUser, IdentityRole>(opciones =>
{
    opciones.SignIn.RequireConfirmedAccount = false;
    opciones.Password.RequireNonAlphanumeric = false;
    opciones.Password.RequireLowercase = false;
    opciones.Password.RequireUppercase = false;
}).AddEntityFrameworkStores<ApplicationDbContext>()
  .AddDefaultTokenProviders();

builder.Services.PostConfigure<CookieAuthenticationOptions>(IdentityConstants.ApplicationScheme, opciones =>
{
    opciones.LoginPath = "/Usuarios/Login";
    opciones.AccessDeniedPath = "/Usuarios/Login"; ;
});

builder.Services.AddLocalization(opciones =>
{
    opciones.ResourcesPath = "Recursos";
}) ;

var app = builder.Build();

app.UseRequestLocalization(opciones =>
{
    opciones.DefaultRequestCulture = new RequestCulture("es");
    opciones.SupportedUICultures = Constante.CulturasIASoportadas.
    Select(culturas => new CultureInfo(culturas.Value)).ToList();
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
