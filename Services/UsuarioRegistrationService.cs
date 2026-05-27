using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VibraUrbana.Data;
using VibraUrbana.Models;
using VibraUrbana.ViewModels;

namespace VibraUrbana.Services;

public class UsuarioRegistrationService : IUsuarioRegistrationService
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher<Usuario> _passwordHasher;

    public UsuarioRegistrationService(ApplicationDbContext context, IPasswordHasher<Usuario> passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<IReadOnlyList<Rol>> GetActiveRolesAsync()
    {
        return await _context.Roles
            .Where(rol => rol.Activo)
            .OrderBy(rol => rol.Nombre)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<UsuarioListadoItemViewModel>> GetUsuariosAsync()
    {
        return await _context.Usuarios
            .AsNoTracking()
            .Include(usuario => usuario.Rol)
            .OrderBy(usuario => usuario.NombreCompleto)
            .Select(usuario => new UsuarioListadoItemViewModel
            {
                Id = usuario.Id,
                Cedula = usuario.Cedula,
                NombreCompleto = usuario.NombreCompleto,
                Correo = usuario.Correo,
                Rol = usuario.Rol.Nombre,
                Activo = usuario.Activo
            })
            .ToListAsync();
    }

    public async Task<UsuarioDetalleViewModel?> GetUsuarioDetalleAsync(int id)
    {
        return await _context.Usuarios
            .AsNoTracking()
            .Include(usuario => usuario.Rol)
            .Where(usuario => usuario.Id == id)
            .Select(usuario => new UsuarioDetalleViewModel
            {
                Id = usuario.Id,
                Cedula = usuario.Cedula,
                NombreCompleto = usuario.NombreCompleto,
                Correo = usuario.Correo,
                Rol = usuario.Rol.Nombre,
                Activo = usuario.Activo,
                FechaCreacion = usuario.FechaCreacion
            })
            .SingleOrDefaultAsync();
    }

    public async Task<RegistroUsuarioResult> CreateAsync(CrearUsuarioViewModel model)
    {
        var cedula = model.Cedula.Trim();
        var correo = model.Correo.Trim().ToLowerInvariant();

        var rolExists = await _context.Roles.AnyAsync(rol => rol.Id == model.RolId && rol.Activo);

        if (!rolExists)
        {
            return RegistroUsuarioResult.RolNotFound;
        }

        var cedulaExists = await _context.Usuarios.AnyAsync(usuario => usuario.Cedula == cedula);

        if (cedulaExists)
        {
            return RegistroUsuarioResult.DuplicateCedula;
        }

        var correoExists = await _context.Usuarios.AnyAsync(usuario => usuario.Correo.ToLower() == correo);

        if (correoExists)
        {
            return RegistroUsuarioResult.DuplicateCorreo;
        }

        var usuario = new Usuario
        {
            Cedula = cedula,
            NombreCompleto = model.NombreCompleto.Trim(),
            Correo = correo,
            Activo = true,
            FechaCreacion = DateTime.UtcNow,
            RolId = model.RolId
        };

        usuario.PasswordHash = _passwordHasher.HashPassword(usuario, model.Password);

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        return RegistroUsuarioResult.Success;
    }
}
