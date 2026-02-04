// Included Libraries
using MenuManagerAPI.Menus;
using MenuManagerAPI.CrossCutting;
using CounterStrikeSharp.API.Core;
using MenuManagerAPI.Shared.Models;
using CounterStrikeSharp.API.Modules.Menu;

// Declare namespace
namespace MenuManagerAPI.Core;

// Define class
public class MenuInstance : IMenu
{
    // Define class properties
    public MenuType? ForceType;
    public string Title { get; set; }
    public bool ExitButton { get; set; }
    public Action<CCSPlayerController>? BackAction;
    public Action<CCSPlayerController>? ResetAction;
    public List<ChatMenuOption> MenuOptions { get; }
    public PostSelectAction PostSelectAction { get; set; } = PostSelectAction.Nothing;

    public int ItemsPerPage { get; set; } = 7;
    public int CurrentPage { get; set; }
    public Stack<int> PageOffsets { get; set; } = new();

    // Define constructor
    public MenuInstance(string _title, Action<CCSPlayerController>? _backAction = null, Action<CCSPlayerController>? _resetAction = null, MenuType? _forceType = null)
    {
        Title = _title;
        ExitButton = true;
        ForceType = _forceType;
        BackAction = _backAction;
        ResetAction = _resetAction;
        MenuOptions = new List<ChatMenuOption>();

        chatMenu = new ChatMenu(Title);
        buttonMenu = new ButtonMenu(Title);
        buttonMenu.BackAction = _backAction;
        buttonMenu.ResetAction = _resetAction;
        consoleMenu = new ConsoleMenu(Title);
    }

    // Define class fields
    private ChatMenu chatMenu;
    private ButtonMenu buttonMenu;
    private ConsoleMenu consoleMenu;

    public int TotalPages => (int)Math.Ceiling((double)MenuOptions.Count / ItemsPerPage);

    // Define class methods
    public void NextPage()
    {
        if (CurrentPage < TotalPages - 1)
        {
            PageOffsets.Push(CurrentPage * ItemsPerPage);
            CurrentPage++;
        }
    }

    public void PreviousPage()
    {
        if (CurrentPage > 0 && PageOffsets.Count > 0)
        {
            CurrentPage--;
            PageOffsets.Pop();
        }
    }

    public void GoToPage(int pageNumber)
    {
        if (pageNumber >= 0 && pageNumber < TotalPages)
        {
            if (pageNumber != CurrentPage)
                PageOffsets.Push(CurrentPage * ItemsPerPage);
            CurrentPage = pageNumber;
        }
    }

    public ChatMenuOption AddMenuOption(string display, Action<CCSPlayerController, ChatMenuOption> onSelect, bool disabled = false)
    {
        var option = new ChatMenuOption(display, disabled, (p, opt) => onSelect(p, opt));
        MenuOptions.Add(option);
        chatMenu?.AddMenuOption(display, onSelect, disabled);
        buttonMenu?.AddMenuOption(display, onSelect, disabled);
        consoleMenu?.AddMenuOption(display, onSelect, disabled);
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
            case MenuType.ButtonMenu:
                menu = buttonMenu;
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