// Included libraries
using PlayerSettings;
using CounterStrikeSharp.API.Core;
using MenuManagerAPI.Shared.Models;
using Microsoft.Extensions.Logging;

// Declare namespace
namespace MenuManagerAPI.Core;

// Define class
public class MenuTypeManager
{
    // Define class properties
    private static ILogger? _logger;
    private static ISettingsApi? _settingsApi;
    private static MenuType _defaultMenuType = MenuType.ButtonMenu;
    private static readonly Dictionary<ulong, MenuType> MenuTypeCache = new();

    // Define class methods
    public static void Initialize(ISettingsApi? settingsApi, MenuType defaultMenuType, ILogger? logger = null)
    {
        _logger = logger;
        _settingsApi = settingsApi;
        _defaultMenuType = defaultMenuType;
    }

    public static MenuType GetPlayerMenuType(CCSPlayerController player)
    {
        if (MenuTypeCache.TryGetValue(player.SteamID, out MenuType cachedType))
            return cachedType;

        MenuType menuType = _defaultMenuType;

        if (_settingsApi != null)
        {
            try
            {
                string? savedType = _settingsApi.GetPlayerSettingsValue(player, "menutype", string.Empty);
                if (!string.IsNullOrEmpty(savedType) && Enum.TryParse<MenuType>(savedType, out MenuType parsedType))
                    menuType = parsedType;
            }
            catch (Exception ex)
            {
                _logger?.LogWarning($"Failed to load menu type preference for player {player.PlayerName} ({player.SteamID}): {ex.Message}");
            }
        }

        MenuTypeCache[player.SteamID] = menuType;
        return menuType;
    }

    public static MenuType SetPlayerMenuType(CCSPlayerController player, MenuType menuType)
    {
        MenuTypeCache[player.SteamID] = menuType;

        if (_settingsApi != null)
        {
            try
            {
                _settingsApi.SetPlayerSettingsValue(player, "menutype", menuType.ToString());
            }
            catch (Exception ex)
            {
                _logger?.LogWarning($"Failed to save menu type preference for player {player.PlayerName} ({player.SteamID}): {ex.Message}");
            }
        }

        return menuType;
    }

    public static void ClearPlayerCache(ulong steamId)
    {
        MenuTypeCache.Remove(steamId);
    }

    public static void ClearAllCache()
    {
        MenuTypeCache.Clear();
    }

    public static bool IsValidMenuType(MenuType menuType)
    {
        return Enum.IsDefined(typeof(MenuType), menuType);
    }

    public static MenuType GetDefaultMenuType()
    {
        return _defaultMenuType;
    }
}
