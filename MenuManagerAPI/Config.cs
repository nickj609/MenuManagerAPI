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
        [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
        public PlayerButtons UpButton { get; set; } = PlayerButtons.Forward;

        [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
        public PlayerButtons DownButton { get; set; } = PlayerButtons.Back;

        [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
        public PlayerButtons LeftButton { get; set; } = PlayerButtons.Moveleft;

        [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
        public PlayerButtons RightButton { get; set; } = PlayerButtons.Moveright;

        [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
        public PlayerButtons SelectButton { get; set; } = PlayerButtons.Use;

        [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
        public PlayerButtons ExitButton { get; set; } = PlayerButtons.Reload;

        [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
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
        [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
        public FontSize FontSize { get; set; } = FontSize.M;
        public bool Bold { get; set; } = false;
        public bool Italic { get; set; } = false;
    }

    public class SelectionStyling
    {
        public string Color { get; set; } = "red";
        [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
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
        [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
        public FontSize FontSize { get; set; } = FontSize.S;
        public SeparatorStyling Separator { get; set; } = new();
        public ButtonKeyStyling Button { get; set; } = new();
    }

    public class ButtonMenuConfig
    {
        public bool OptionCount { get; set; } = true;
        public bool MoveWithOpenMenu { get; set; } = false;
        public bool UseVelocityModifier { get; set; } = false;
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
        public int Version { get; set; } = 2;
        [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
        public MenuType DefaultMenu { get; set; } = MenuType.ButtonMenu;
        public ButtonMenuConfig ButtonMenu { get; set; } = new();
    }
}