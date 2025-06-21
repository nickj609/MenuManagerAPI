// Included libraries
using CounterStrikeSharp.API.Core;
using MenuManagerAPI.Shared.Models;
using CounterStrikeSharp.API.Modules.Menu;

// Declare namespace
namespace MenuManagerAPI.Shared;

// Define interface
public interface IMenuAPI
{
    // Define interface methods
    public IMenu GetMenu(string title, Action<CCSPlayerController>? backAction = null, Action<CCSPlayerController>? resetAction = null, MenuType? forceType = null);
    public MenuType GetMenuType(CCSPlayerController player);
    public bool HasOpenedMenu(CCSPlayerController player);
    public void OpenMenu(IMenu menu, CCSPlayerController player);
    public void OpenMenuToAll(IMenu menu);
    public void CloseMenu(CCSPlayerController player);
    public void CloseMenuForAll();
}