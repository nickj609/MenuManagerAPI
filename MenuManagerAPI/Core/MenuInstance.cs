// Included libraries
using MenuManagerAPI.Models;
using MenuManagerAPI.CrossCutting;
using CounterStrikeSharp.API.Core;
using MenuManagerAPI.Shared.Models;
using CounterStrikeSharp.API.Modules.Menu;

// Define namespace
namespace MenuManagerAPI.Core
{
    // Define class
    public class MenuInstance : IMenu
    {
        // Define public properties
        public MenuType? ForceType;
        public string Title { get; set; }
        public bool ExitButton { get; set; }
        public Action<CCSPlayerController>? BackAction;
        public Action<CCSPlayerController>? ResetAction;
        public List<ChatMenuOption> MenuOptions { get; }
        public PostSelectAction PostSelectAction { get; set; } = PostSelectAction.Nothing;

        // Define class constructor
        public MenuInstance(string _title, Action<CCSPlayerController>? _backAction = null, Action<CCSPlayerController>? _resetAction = null, MenuType? _forceType = null)
        {
            Title = _title;
            ExitButton = true;
            ForceType = _forceType;
            BackAction = _backAction;
            ResetAction = _resetAction;

            // Create menus
            chatMenu = new ChatMenu(Title);
            screenMenu = new ScreenMenu(Title);
            buttonMenu = new ButtonMenu(Title);
            buttonMenu.BackAction = _backAction;
            buttonMenu.ResetAction = _resetAction;
            consoleMenu = new ConsoleMenu(Title);
            MenuOptions = new List<ChatMenuOption>();

            if (Plugin.Instance != null)
            {
                centerHtmlMenu = new CenterHtmlMenu(Title, Plugin.Instance);
            }
        }

        // Define private properties
        private ChatMenu chatMenu;
        private ScreenMenu screenMenu;
        private ButtonMenu buttonMenu;
        private ConsoleMenu consoleMenu;
        private CenterHtmlMenu? centerHtmlMenu;

        // Define class methods
        public ChatMenuOption AddMenuOption(string display, Action<CCSPlayerController, ChatMenuOption> onSelect, bool disabled = false)
        {
            var option = new ChatMenuOption(display, disabled, (p, opt) => onSelect(p, opt));
            MenuOptions.Add(option);
            chatMenu?.AddMenuOption(display, onSelect, disabled);
            buttonMenu?.AddMenuOption(display, onSelect, disabled);
            screenMenu?.AddMenuOption(display, onSelect, disabled);
            consoleMenu?.AddMenuOption(display, onSelect, disabled);
            centerHtmlMenu?.AddMenuOption(display, onSelect, disabled);
            return option;
        }

        public void Open(CCSPlayerController player)
        {
            MenuType forceType = ForceType ?? PlayerExtensions.GetMenuType(player);
            IMenu? menu;

            switch (forceType)
            {
                case MenuType.ChatMenu:
                    menu = chatMenu;
                    menu.PostSelectAction = PostSelectAction;
                    menu.Open(player);
                    break;
                case MenuType.ConsoleMenu:
                    menu = consoleMenu;
                    menu.PostSelectAction = PostSelectAction;
                    menu.Open(player);
                    break;
                case MenuType.CenterMenu:
                    menu = centerHtmlMenu;
                    menu!.PostSelectAction = PostSelectAction;
                    menu?.Open(player);
                    break;
                case MenuType.ButtonMenu:
                    menu = buttonMenu;
                    menu.PostSelectAction = PostSelectAction;
                    menu.Open(player);
                    break;
                case MenuType.ScreenMenu:
                    menu = screenMenu;
                    menu.PostSelectAction = PostSelectAction;
                    menu.Open(player);
                    break;
                case MenuType.ScrollMenu:
                    menu = screenMenu;
                    menu.PostSelectAction = PostSelectAction;
                    menu.Open(player);
                    break;
            }

            Plugin.Players[player.Slot].MenuOpen = true;
        }

        public void OpenToAll()
        {
            foreach (var player in PlayerExtensions.ValidPlayers())
            {
                Open(player);
            }
        }
    }
}