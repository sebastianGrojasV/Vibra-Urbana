using System.Globalization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using VibraUrbana.Data;
using VibraUrbana.Filters;
using VibraUrbana.Models;
using VibraUrbana.Repositories;
using VibraUrbana.Services;

var builder = WebApplication.CreateBuilder(args);

var costaRicaCulture = new CultureInfo("es-CR");
CultureInfo.DefaultThreadCurrentCulture = costaRicaCulture;
CultureInfo.DefaultThreadCurrentUICulture = costaRicaCulture;

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("DefaultConnection is not configured.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();
builder.Services.AddScoped<IUsuarioAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IUsuarioRegistrationService, UsuarioRegistrationService>();
builder.Services.AddScoped<IRolRepositorio, RolRepositorio>();
builder.Services.AddScoped<IRolServicio, RolServicio>();
builder.Services.AddScoped<IClienteServicio, ClienteServicio>();
builder.Services.AddScoped<IProductoServicio, ProductoServicio>();
builder.Services.AddScoped<INavigationMenuService, NavigationMenuService>();
builder.Services.AddScoped<IAuthorizationHandler, PermisoAuthorizationHandler>();
builder.Services.AddScoped<ICategoriaRepositorio, CategoriaRepositorio>();
builder.Services.AddScoped<ICategoriaServicio, CategoriaServicio>();
builder.Services.AddScoped<IVentaServicio, VentaServicio>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/Login";
        options.Cookie.Name = "VibraUrbana.Auth";
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization(options =>
{
    foreach (var permiso in PermisosSistema.Todos)
    {
        options.AddPolicy(permiso, policy => policy.Requirements.Add(new PermisoRequirement(permiso)));
    }
});

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<NoCacheForAuthenticatedUsersFilter>();
});

var app = builder.Build();

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(costaRicaCulture),
    SupportedCultures = [costaRicaCulture],
    SupportedUICultures = [costaRicaCulture]
});

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
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
