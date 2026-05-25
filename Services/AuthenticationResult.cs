using VibraUrbana.Models;

namespace VibraUrbana.Services;

public class AuthenticationResult
{
    public LoginResult Result { get; init; }

    public Usuario? Usuario { get; init; }
}
