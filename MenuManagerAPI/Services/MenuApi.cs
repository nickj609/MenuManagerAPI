// Included libraries
using MenuManagerAPI.Core;
using MenuManagerAPI.Shared;
using MenuManagerAPI.CrossCutting;
using CounterStrikeSharp.API.Core;
using MenuManagerAPI.Shared.Models;
using CounterStrikeSharp.API.Modules.Menu;

// Declare namespace
namespace MenuManagerAPI.Services;

// Define class
public class MenuAPI : IMenuAPI
{

    // Define class methods
    public IMenu GetMenu(string title, Action<CCSPlayerController>? backAction = null, Action<CCSPlayerController>? resetAction = null, MenuType? forceType = null)
    {
        return new MenuInstance(title, backAction, resetAction, forceType);
    }
    public MenuType GetMenuType(CCSPlayerController player)
    {
        return PlayerExtensions.GetMenuType(player);
    }

    public bool HasOpenedMenu(CCSPlayerController player)
    {
        return Plugin.Players[player.Slot].MenuOpen;
    }

    public void OpenMenu(IMenu menu, CCSPlayerController player)
    {
        menu.Open(player);
    }

    public void OpenMenuToAll(IMenu menu)
    {
        menu.OpenToAll();
    }

    public void CloseMenu(CCSPlayerController player)
    {
        if (Plugin.Players.TryGetValue(player.Slot, out PlayerInfo? playerInfo))
        {
            if (PlayerExtensions.GetMenuType(player) is MenuType.ButtonMenu)
            {
                playerInfo.CloseMenu();
            }
            else
            {
                MenuManager.CloseActiveMenu(player);
            }

            Plugin.Players[player.Slot].MenuOpen = false;
        }
    }

    public void CloseMenuForAll()
    {
        foreach (var player in PlayerExtensions.ValidPlayers())
        {
            CloseMenu(player);
        }
    }
}