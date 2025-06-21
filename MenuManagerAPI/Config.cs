// Included libraries
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using MenuManagerAPI.Shared.Models;

// Declare namespace
namespace MenuManagerAPI
{
    // Define button configuration
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

    // Define configuration class
    public class Config : IBasePluginConfig
    {
        public int Version { get; set; } = 1;
        public MenuType DefaultMenu { get; set; } = MenuType.ButtonMenu;
        public string SoundScroll { get; set; } = "";
        public string SoundClick { get; set; } = "";
        public string SoundBack { get; set; } = "";
        public string SoundExit { get; set; } = "";
        public string SoundDisabled { get; set; } = "";
        public bool OptionCount { get; set; } = true;
        public bool MoveWithOpenMenu { get; set; } = false;
        public bool UseVelocityModifier { get; set; } = false;
        public ButtonsConfig ButtonsConfig { get; set; } = new();
    }
}