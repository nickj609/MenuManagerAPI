// Included Libraries
using MenuManagerAPI.Models;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using MenuManagerAPI.Shared.Models;

// Declare namespace
namespace MenuManagerAPI
{
    // Define configuration classes
    public class ButtonsConfig
    {
        public PlayerButtons UpButton { get; set; } = PlayerButtons.Forward;
        public PlayerButtons DownButton { get; set; } = PlayerButtons.Back;
        public PlayerButtons LeftButton { get; set; } = PlayerButtons.Moveleft;
        public PlayerButtons RightButton { get; set; } = PlayerButtons.Moveright;
        public PlayerButtons SelectButton { get; set; } = PlayerButtons.Use;
        public PlayerButtons ExitButton { get; set; } = PlayerButtons.Reload;
        public PlayerButtons BackButton { get; set; } = PlayerButtons.Duck;
    }

    public class ButtonSounds
    {
        public string Scroll { get; set; } = "";
        public string Click { get; set; } = "";
        public string Back { get; set; } = "";
        public string Exit { get; set; } = "";
        public string Disabled { get; set; } = "";
    }

    public class TitleStyling
    {
        public string Color { get; set; } = "red";
        public FontSize FontSize { get; set; } = FontSize.M;
        public bool Bold { get; set; } = false;
        public bool Italic { get; set; } = false;
    }

    public class SelectionStyling
    {
        public string Color { get; set; } = "red";
        public FontSize FontSize { get; set; } = FontSize.SM;
        public bool Bold { get; set; } = false;
        public bool Italic { get; set; } = false;
    }

    public class SeparatorStyling
    {
        public string Color { get; set; } = "white";
        public bool Bold { get; set; } = false;
        public bool Italic { get; set; } = false;
    }

    public class ButtonKeyStyling
    {
        public string Color { get; set; } = "gold";
        public bool Bold { get; set; } = true;
        public bool Italic { get; set; } = false;
    }

    public class FooterStyling
    {
        public FontSize FontSize { get; set; } = FontSize.S;
        public SeparatorStyling Separator { get; set; } = new();
        public ButtonKeyStyling Button { get; set; } = new();
    }

    public class ButtonMenuConfig
    {
        public bool OptionCount { get; set; } = true;
        public bool MoveWithOpenMenu { get; set; } = false;
        public bool UseVelocityModifier { get; set; } = false;
        public bool ClearStateOnRoundEnd { get; set; } = true;
        public string EnabledOptionColor { get; set; } = "white";
        public string DisabledOptionColor { get; set; } = "#aaaaaa";
        public ButtonsConfig ButtonsConfig { get; set; } = new();
        public ButtonSounds ButtonSounds { get; set; } = new();
        public TitleStyling Title { get; set; } = new();
        public SelectionStyling Selection { get; set; } = new();
        public FooterStyling Footer { get; set; } = new();
    }

    // Define main configuration class
    public class Config : IBasePluginConfig
    {
        // Global Plugin Settings
        public int Version { get; set; } = 1;
        public MenuType DefaultMenu { get; set; } = MenuType.ButtonMenu;
        
        // Button Menu Settings
        public ButtonMenuConfig ButtonMenu { get; set; } = new();
    }
}