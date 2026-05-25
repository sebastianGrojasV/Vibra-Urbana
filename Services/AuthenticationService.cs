using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VibraUrbana.Data;
using VibraUrbana.Models;

namespace VibraUrbana.Services;

public class AuthenticationService : IUsuarioAuthenticationService
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher<Usuario> _passwordHasher;

    public AuthenticationService(ApplicationDbContext context, IPasswordHasher<Usuario> passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<AuthenticationResult> ValidateCredentialsAsync(string correo, string password)
    {
        var normalizedEmail = correo.Trim().ToLowerInvariant();

        var usuario = await _context.Usuarios
            .Include(user => user.Rol)
            .SingleOrDefaultAsync(user => user.Correo.ToLower() == normalizedEmail);

        if (usuario is null)
        {
            return new AuthenticationResult { Result = LoginResult.InvalidCredentials };
        }

        if (!usuario.Activo)
        {
            return new AuthenticationResult { Result = LoginResult.InactiveUser };
        }

        var passwordResult = _passwordHasher.VerifyHashedPassword(usuario, usuario.PasswordHash, password);

        if (passwordResult == PasswordVerificationResult.Failed)
        {
            return new AuthenticationResult { Result = LoginResult.InvalidCredentials };
        }

        return new AuthenticationResult
        {
            Result = LoginResult.Success,
            Usuario = usuario
        };
    }
}
