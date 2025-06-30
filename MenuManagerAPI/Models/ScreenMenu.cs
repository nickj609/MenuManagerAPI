// Included libraries
using MenuManagerAPI.Core;
using MenuManagerAPI.CrossCutting;
using CounterStrikeSharp.API.Core;
using Microsoft.Extensions.Logging;
using CounterStrikeSharp.API.Modules.Menu;

// Declare namespace
namespace MenuManagerAPI.Models;

// Define class
public class ScreenMenu : IMenu
{
    // Define class properties
    public string Title { get; set; }
    public bool ExitButton { get; set; }
    public Action<CCSPlayerController>? BackAction;
    public Action<CCSPlayerController>? ResetAction;
    public List<ChatMenuOption> MenuOptions { get; } = new();
    public LinkedListNode<ScreenMenuOption>? Prev { get; set; } = null;
    public LinkedList<ScreenMenuOption> ButtonOptions { get; set; } = new();
    public PostSelectAction PostSelectAction { get; set; } = PostSelectAction.Nothing;

    public ScreenMenu(string _title, Action<CCSPlayerController>? backAction = null, Action<CCSPlayerController>? resetAction = null)
    {
        Title = _title;
        BackAction = backAction;
        ResetAction = resetAction;
        MenuOptions = new List<ChatMenuOption>();
    }

    public ChatMenuOption AddMenuOption(string display, Action<CCSPlayerController, ChatMenuOption> onSelect, bool disabled = false)
    {
        // Add menu option
        var option = new ScreenMenuOption(display, disabled, onSelect);
        MenuOptions.Add(new ChatMenuOption(display, disabled, onSelect));
        option.Parent = this;
        ButtonOptions.AddLast(option);
        option.Index = ButtonOptions.Count - 1;
        return option;
    }

    public void Open(CCSPlayerController player)
    {
        if (Plugin.Players.TryGetValue(player.Slot, out PlayerInfo? playerInfo))
        {
            if (playerInfo != null)
            {
                // Add Logic to open Screen Menu
            }
        }
        else
        {
            Plugin.Instance?.Logger.LogError("Player not found in player list.");
        }
    }

    public void OpenToAll()
    {
        foreach (var player in PlayerExtensions.ValidPlayers())
        {
            Open(player);
        }
    }
    
    public void OnBackAction(CCSPlayerController player)
    {
        if (BackAction != null)
        {
            PlayerExtensions.PlaySound(player, Plugin.Instance!.Config.SoundBack);
            BackAction(player);
        }
    }

    public void OnResetAction(CCSPlayerController player)
    {
        if (ResetAction != null)
            ResetAction(player);
        else
            Plugin.Instance!.Logger.LogWarning($"Reset action is not passed to func! TITLE: {Title}");
    }
}