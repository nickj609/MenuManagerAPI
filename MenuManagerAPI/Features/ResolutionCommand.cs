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
public class ResolutionCommand : IPluginDependency<Plugin, Config>
{
    // Define class dependencies
    private Plugin? plugin;
    private IMenuAPI? menuAPI;
    private readonly PluginCapability<IMenuAPI?> pluginCapability = new("menu:api");

    // Define class constructor
    public ResolutionCommand(){}

    // Define on load behavior
    public void OnLoad(Plugin _plugin)
    {
        plugin = _plugin;
        // Register the command to change menu type
        plugin.AddCommand("css_resolution", "Allows the player to change their menu type.", OnResolutionCommand);
        menuAPI = new MenuAPI();
        // Register the MenuAPI capability for other plugins to use
        Capabilities.RegisterPluginCapability(pluginCapability, () => menuAPI);
    }

    // Define admin map menu command handler
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void OnResolutionCommand(CCSPlayerController? player, CommandInfo command)
    {
        if (player != null && plugin != null)
        {
            if (PlayerExtensions.GetMenuType(player) == MenuType.ScreenMenu || PlayerExtensions.GetMenuType(player) == MenuType.ScrollMenu)
            {
                // Get a new menu instance for selecting menu type
                var menu = menuAPI?.GetMenu(plugin.Localizer["resolution.select"]);
                menu!.PostSelectAction = PostSelectAction.Close; // Close menu after selection

                foreach (ScreenResolution res in Enum.GetValues(typeof(ScreenResolution)))
                {
                    menu.AddMenuOption(res.GetDescription(), (player, option) => { PlayerExtensions.SetResolution(player, res); });
                }
                menu.Open(player); // Open the menu for the player
            }
        }
        else
        {
            command.ReplyToCommand("Your current menu type does not support changing resolution.");
        }
    }
}