// Included libraries
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;

// Declare namespace
namespace MenuManagerAPI.Models;

// Define class
public class ButtonMenuOption : ChatMenuOption
{
    public ButtonMenuOption(string display, bool disabled, Action<CCSPlayerController, ChatMenuOption> onSelect) : base(display, disabled, onSelect)
    {
        OnChoose = onSelect;
        OptionDisplay = display;
    }

    // Define class properties
    public int Index { get; set; }
    public ButtonMenu? Parent { get; set; }
    public string OptionDisplay { get; set; }
    public Action<CCSPlayerController, ChatMenuOption>? OnChoose { get; set; }
}