namespace VibraUrbana.Services;

public interface IUsuarioAuthenticationService
{
    Task<AuthenticationResult> ValidateCredentialsAsync(string correo, string password);
}
