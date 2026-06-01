using System.Security.Claims;

namespace VibraUrbana.Services;

public interface INavigationMenuService
{
    Task<IReadOnlyList<MenuItemViewModel>> GetItemsAsync(ClaimsPrincipal user);
}
