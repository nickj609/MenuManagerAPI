// Included libraries
using MenuManagerAPI.Shared;
using MenuManagerAPI.Contracts;
using MenuManagerAPI.Services;
using MenuManagerAPI.CrossCutting;
using CounterStrikeSharp.API.Core;
using MenuManagerAPI.Shared.Models;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Core.Capabilities;

// Define namespace
namespace MenuManagerAPI.Features;

// Define class
public class MenusCommand : IPluginDependency<Plugin, Config>
{
    // Define class dependencies
    private Plugin? plugin;
    private IMenuAPI? menuAPI;
    private readonly PluginCapability<IMenuAPI?> pluginCapability = new("menu:api");

    // Define class constructor
    public MenusCommand(){}

    // Define on load behavior
    public void OnLoad(Plugin _plugin)
    {
        plugin = _plugin;
        plugin.AddCommand("css_changemenu", "Allows the player to change their menu type.", OnMenusCommand);
        menuAPI = new MenuAPI();
        Capabilities.RegisterPluginCapability(pluginCapability, () => menuAPI);
    }

    // Define admin map menu command handler
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void OnMenusCommand(CCSPlayerController? player, CommandInfo command)
    {
        if (player != null && plugin != null)
        {
            var menu = menuAPI?.GetMenu(plugin.Localizer["menutype.select"]);
            menu!.PostSelectAction = PostSelectAction.Close;

            menu.AddMenuOption(plugin.Localizer["menutype.console"], (player, option) => { PlayerExtensions.SetMenuType(player, MenuType.ConsoleMenu); });
            menu.AddMenuOption(plugin.Localizer["menutype.chat"], (player, option) => { PlayerExtensions.SetMenuType(player, MenuType.ChatMenu); });
            menu.AddMenuOption(plugin.Localizer["menutype.center"], (player, option) => { PlayerExtensions.SetMenuType(player, MenuType.CenterMenu); });
            menu.AddMenuOption(plugin.Localizer["menutype.button"], (player, option) => { PlayerExtensions.SetMenuType(player, MenuType.ButtonMenu); });
            menu.Open(player);
        }
    }
}