// Included libraries
using MenuManagerAPI.Shared;
using MenuManagerAPI.Services;
using MenuManagerAPI.Contracts;
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
        
        // Register the command to change menu type
        plugin.AddCommand("css_changemenu", "Allows the player to change their menu type.", OnMenusCommand);
        menuAPI = new MenuAPI();
        
        // Register the MenuAPI capability for other plugins to use
        Capabilities.RegisterPluginCapability(pluginCapability, () => menuAPI);
    }

    // Define admin map menu command handler
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void OnMenusCommand(CCSPlayerController? player, CommandInfo command)
    {
        if (player != null && plugin != null)
        {
            // Check for an argument specifying the menu type
            if (command.ArgCount > 1)
            {
                var arg = command.GetArg(1).ToLowerInvariant();
                MenuType? selectedType = arg switch
                {
                    "chat" or "chatmenu" => MenuType.ChatMenu,
                    "console" or "consolemenu" => MenuType.ConsoleMenu,
                    "center" or "centermenu" => MenuType.CenterMenu,
                    "button" or "buttonmenu" => MenuType.ButtonMenu,
                    _ => null
                };

                if (selectedType.HasValue)
                    PlayerExtensions.SetMenuType(player, selectedType.Value);
                else
                    command.ReplyToCommand(plugin.Localizer["menutype.notfound"] ?? $"{command.GetArg(1)} menu type not found.");

                return;
            }

            // No valid argument, show menu selection
            var menu = menuAPI?.GetMenu(plugin.Localizer["menutype.select"]);
            menu!.PostSelectAction = PostSelectAction.Close;
            menu.AddMenuOption(plugin.Localizer["menutype.chat"], (player, option) => { PlayerExtensions.SetMenuType(player, MenuType.ChatMenu, command); });
            menu.AddMenuOption(plugin.Localizer["menutype.center"], (player, option) => { PlayerExtensions.SetMenuType(player, MenuType.CenterMenu, command); });
            menu.AddMenuOption(plugin.Localizer["menutype.button"], (player, option) => { PlayerExtensions.SetMenuType(player, MenuType.ButtonMenu, command); });
            menu.AddMenuOption(plugin.Localizer["menutype.console"], (player, option) => { PlayerExtensions.SetMenuType(player, MenuType.ConsoleMenu, command); });
            menu.Open(player);
        }
    }
}