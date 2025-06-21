# MenuManagerAPI

MenuManagerAPI takes the powerful groundwork established by [MenuManagerCS2](https://github.com/NickFox007/MenuManagerCS2) and elevates it. We've refined the core concepts to offer a smarter, faster, and more versatile experience, focusing on:

- **A Modern Look & Feel**: Say goodbye to outdated menus. MenuManagerAPI gives you a stylish and highly customizable button menu right out of the box, making interactions more engaging for players.

- **Built for Efficiency**: Whether you have a small, simple menu or a complex one, MenuManagerAPI is engineered to be lightweight and responsive. Its intelligent design ensures menus open and respond quickly, providing a smoother experience for everyone.

- **Effortless Integration for Developers**: If you're currently using [MenuManagerCS2](https://github.com/NickFox007/MenuManagerCS2), switching is a breeze. We've kept the core API consistent, meaning you can upgrade your existing plugins with
minimal changes and immediately benefit from these performance and UI improvements.

## Credits

This plugin has incorporated code and/or design principles from the following plugins:

- [WASDMenuAPI](https://github.com/Interesting-exe/WASDMenuAPI) by @Interesting-exe
- [MenuManagerCS2](https://github.com/NickFox007/MenuManagerCS2) by @NickFox007

## Commands

Players can say `!changemenu` to change their preferred menu type.

![MenuSelection](menu.png)

## Configuration

Below is the default configuration of this plugin, which you can modify as you so choose.

```json

```

## Language Support

Below is an example lang file that you can use to customize the language of this plugin.

```json
{
  "menutype.select": "Select Menu",
  "menutype.console": "Console",
  "menutype.chat": "Chat",
  "menutype.center": "Center",
  "menutype.button": "Button",
  "menutype.selected": "Selected menu: ",
  "menu.enabled.color": "white",
  "menu.disabled.color": "#aaaaaa",
  "menu.selection.left": "<font class='fontSize-s' color='red'>&#9654;</font><font class='fontSize-s' color='red'> [ </font>",
  "menu.selection.right": "<font class='fontSize-s' color='red'> ] </font><font class='fontSize-s' color='red'>&#9664;</font>",
  "menu.title.color": "red",
  "menu.bottom.text": "<font color='white'>Scroll </font><font color='gold' class='stratum-bold'>W/S</font><font color='white'> | Sel </font><font color='gold' class='stratum-bold'>E</font><font color='white'> | Prev <font color='gold' class='stratum-bold'>CTRL</font><font color='white'> | Exit </font><font color='gold' class='stratum-bold'>R</font>"
}
```

## Interface

```csharp
public interface IMenuAPI
{
    public IMenu GetMenu(string title, Action<CCSPlayerController>? backAction = null, Action<CCSPlayerController>? resetAction = null, MenuType? forceType = null);
    public MenuType GetMenuType(CCSPlayerController player);
    public bool HasOpenedMenu(CCSPlayerController player);
    public void OpenMenu(IMenu menu, CCSPlayerController player);
    public void OpenMenuToAll(IMenu menu);
    public void CloseMenu(CCSPlayerController player);
    public void CloseMenuForAll();
}
```

## Example Usage

```csharp
// Included libraries
using MenuManagerAPI.Shared;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Core.Capabilities;
using CounterStrikeSharp.API.Core.Attributes.Registration;

// Define plugin class
public class Plugin : BasePlugin
{
    // Define module properties
    public override string ModuleName => "Example Menu";
    public override string ModuleVersion => "1.0.1";
    public override string ModuleAuthor => "Striker-Nick";
    public override string ModuleDescription => "Example Menu Plugin";

    // Define class properties
    private IMenuApi? _api;
    private readonly PluginCapability<IMenuApi?> _pluginCapability = new("menu:api");    

    // Define class methods
    public override void OnAllPluginsLoaded(bool hotReload)
    {
        _api = _pluginCapability.Get();
        if (_api == null) Console.WriteLine("MenuManagerAPI not found...");
    }

    [ConsoleCommand("css_test_menu", "Test menu!")]
    public void OnCommand(CCSPlayerController? player, CommandInfo command)
    {
        if (player != null)
        {            
            IMenu? menu = _api.GetMenu("Menu Title");
            for (int i = 0; i < 10; i++)
            {
                menu.AddMenuOption($"itemline{i}", (player, option) => { player.PrintToChat($"Selected: {option.Text}"); });
            }
            menu.Open(player);
        }
    }
}
```

## Need Help?

Still need help? [create a new issue](https://github.com/nickj609/MenuManagerAPI/issues/new/choose).
