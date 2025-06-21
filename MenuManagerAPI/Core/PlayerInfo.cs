using System.Text;
using MenuManagerAPI.Models;
using CounterStrikeSharp.API;
using MenuManagerAPI.CrossCutting;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;

namespace MenuManagerAPI.Core;

public class PlayerInfo
{
    private string CenterHtml = "";
    private ButtonMenu? MainMenu = null;
    public PlayerButtons Buttons { get; set; }
    public bool MenuOpen { get; set; } = false;
    public float VelocityModifier { get; set; } = 0.0f;
    public required CCSPlayerController player { get; set; }
    public int VisibleOptions = MenuExtensions.MAX_VISIBLE_OPTIONS; 
    public LinkedListNode<ButtonMenuOption>? MenuStart { get; set; }
    public LinkedListNode<ButtonMenuOption>? CurrentChoice { get; set; }
    public ButtonMenuFontSize CurrentItemDisplayFontSize { get; private set; } = MenuExtensions.DefaultItemFontSize;
    public ButtonMenuFontSize CurrentHeaderDisplayFontSize { get; private set; } = MenuExtensions.DefaultHeaderFontSize;
    public ButtonMenuFontSize CurrentFooterDisplayFontSize { get; private set; } = MenuExtensions.DefaultFooterFontSize;


    public void OpenMenu(ButtonMenu? menu)
    {
        if (MainMenu == null)
        {
            if (Plugin.Instance != null && !Plugin.Instance.Config.MoveWithOpenMenu)
            {
                if (Plugin.Instance.Config.UseVelocityModifier)
                {
                    if (Plugin.Players.ContainsKey(player.Slot))
                    {
                        Plugin.Players[player.Slot].VelocityModifier = player.PlayerPawn.Value!.VelocityModifier;
                    }
                }
                else
                {
                    if (player.Pawn.Value != null)
                    {
                        PlayerExtensions.Freeze(player.Pawn.Value);
                    }
                }
            }

            if (Plugin.Instance != null)
            {
                Plugin.Instance.RegisterListener<Listeners.OnTick>(OnTick);
            }

            // Font sizes are now fixed
            CurrentItemDisplayFontSize = MenuExtensions.DefaultItemFontSize;
            CurrentHeaderDisplayFontSize = MenuExtensions.DefaultHeaderFontSize;
            CurrentFooterDisplayFontSize = MenuExtensions.DefaultFooterFontSize;

            MainMenu = menu;
            // Ensure VisibleOptions is always MAX_VISIBLE_OPTIONS for the main menu
            VisibleOptions = MenuExtensions.MAX_VISIBLE_OPTIONS; 
            CurrentChoice = MainMenu?.ButtonOptions.First;
            MenuStart = CurrentChoice;
            UpdateCenterHtml();
        }
        else // This block handles opening sub-menus
        {
            // Font sizes are now fixed
            CurrentItemDisplayFontSize = MenuExtensions.DefaultItemFontSize;
            CurrentHeaderDisplayFontSize = MenuExtensions.DefaultHeaderFontSize;
            CurrentFooterDisplayFontSize = MenuExtensions.DefaultFooterFontSize;

            menu!.Prev = CurrentChoice;
            // !! FIX: Ensure sub-menus also respect MAX_VISIBLE_OPTIONS
            VisibleOptions = MenuExtensions.MAX_VISIBLE_OPTIONS; 
            CurrentChoice = menu.ButtonOptions.First;
            MenuStart = CurrentChoice;
            UpdateCenterHtml();
        }
    }

    // ... (rest of your PlayerInfo class methods remain the same)
    public void CloseMenu()
    {
        if (Plugin.Instance != null)
        {
            Plugin.Instance.RemoveListener<Listeners.OnTick>(OnTick);

            if (!Plugin.Instance.Config.MoveWithOpenMenu)
            {
                if (Plugin.Instance.Config.UseVelocityModifier)
                {
                    if (player.PlayerPawn.Value != null && Plugin.Players.ContainsKey(player.Slot))
                    {
                        player.PlayerPawn.Value.VelocityModifier = Plugin.Players[player.Slot].VelocityModifier;
                    }
                }
                else
                {
                    if (player.Pawn.Value != null)
                    {
                        PlayerExtensions.Unfreeze(player.Pawn.Value);
                    }
                }
            }
        }

        MainMenu = null;
        CurrentChoice = null;
        CenterHtml = "";
    }

    public void OnTick()
    {
        if (Plugin.Instance != null)
        {
            if (Plugin.Instance.Config.UseVelocityModifier)
            {
                player.PlayerPawn.Value!.VelocityModifier = 0.0f;
            }

            if ((Buttons & Plugin.Instance.Config.ButtonsConfig.UpButton) == 0 && (player.Buttons & Plugin.Instance.Config.ButtonsConfig.UpButton) != 0)
            {
                PlayerExtensions.PlaySound(player, Plugin.Instance!.Config!.SoundScroll);
                ScrollUp();
            }
            else if ((Buttons & Plugin.Instance.Config.ButtonsConfig.DownButton) == 0 && (player.Buttons & Plugin.Instance.Config.ButtonsConfig.DownButton) != 0)
            {
                PlayerExtensions.PlaySound(player, Plugin.Instance!.Config.SoundScroll);
                ScrollDown();
            }
            else if ((Buttons & Plugin.Instance.Config.ButtonsConfig.SelectButton) == 0 && (player.Buttons & Plugin.Instance.Config.ButtonsConfig.SelectButton) != 0)
            {
                Choose();
            }
            else if ((Buttons & Plugin.Instance.Config.ButtonsConfig.BackButton) == 0 && (player.Buttons & Plugin.Instance.Config.ButtonsConfig.BackButton) != 0)
            {
                if (MainMenu?.BackAction != null)
                {
                    MainMenu.BackAction(player);
                    return;
                }

                if (CurrentChoice?.Value.Parent?.Prev != null)
                {
                    PlayerExtensions.PlaySound(player, Plugin.Instance!.Config.SoundBack);
                    GoBackToPrev(CurrentChoice?.Value.Parent.Prev);
                }
            }
            else if ((Buttons & Plugin.Instance.Config.ButtonsConfig.ExitButton) == 0 && (player.Buttons & Plugin.Instance.Config.ButtonsConfig.ExitButton) != 0)
            {
                PlayerExtensions.PlaySound(player, Plugin.Instance!.Config.SoundExit);
                CloseMenu();
            }
            else if ((Buttons & Plugin.Instance.Config.ButtonsConfig.LeftButton) == 0 && (player.Buttons & Plugin.Instance.Config.ButtonsConfig.LeftButton) != 0)
            {
                PlayerExtensions.PlaySound(player, Plugin.Instance!.Config.SoundScroll);
                JumpToTop();
            }
            else if ((Buttons & Plugin.Instance.Config.ButtonsConfig.RightButton) == 0 && (player.Buttons & Plugin.Instance.Config.ButtonsConfig.RightButton) != 0)
            {
                PlayerExtensions.PlaySound(player, Plugin.Instance!.Config.SoundScroll);
                JumpToBottom();
            }
            Buttons = player.Buttons;
            if (CenterHtml != "")
                Server.NextFrame(() =>
                player.PrintToCenterHtml(CenterHtml)
            );
        }

    }

    public void Choose()
    {
        if (player != null && CurrentChoice?.Value?.OnChoose != null && Plugin.Instance != null)
        {
            if (CurrentChoice.Value.Disabled)
            {
                PlayerExtensions.PlaySound(player, Plugin.Instance.Config.SoundDisabled);
            }
            else
            {
                PlayerExtensions.PlaySound(player, Plugin.Instance.Config.SoundClick);
                CurrentChoice.Value.OnChoose(player, CurrentChoice.Value);
            }

            switch (MainMenu?.PostSelectAction)
            {
                case PostSelectAction.Close:
                    CloseMenu();
                    break;
                case PostSelectAction.Reset:
                    if (MainMenu.ResetAction != null && !MenuOpen)
                        MainMenu.ResetAction(player);
                    break;
            }
        }
    }

    public void GoBackToPrev(LinkedListNode<ButtonMenuOption>? menu)
    {
        if (menu != null)
        {
            VisibleOptions = MenuExtensions.MAX_VISIBLE_OPTIONS;
            CurrentChoice = menu;

            if (CurrentChoice?.Value.Index >= MenuExtensions.MAX_VISIBLE_OPTIONS)
            {
                MenuStart = CurrentChoice;
                for (int i = 0; i < MenuExtensions.MAX_VISIBLE_OPTIONS - 1; i++)
                {
                    MenuStart = MenuStart?.Previous;
                }
            }
            else
            {
                MenuStart = CurrentChoice?.List?.First;
            }
            UpdateCenterHtml();
        }
        else
        {
            CurrentChoice = MainMenu?.ButtonOptions.First;
            MenuStart = CurrentChoice;
            UpdateCenterHtml();
        }
    }

    public void ScrollDown()
    {
        if (CurrentChoice == null || MainMenu == null)
            return;

        CurrentChoice = CurrentChoice.Next ?? CurrentChoice.List?.First;
        MenuStart = CurrentChoice!.Value.Index >= VisibleOptions ? MenuStart!.Next : CurrentChoice.List?.First;
        UpdateCenterHtml();
    }

    public void ScrollUp()
    {
        if (CurrentChoice == null || MainMenu == null)
            return;

        CurrentChoice = CurrentChoice.Previous ?? CurrentChoice.List?.Last;
        if (CurrentChoice == CurrentChoice?.List?.Last && CurrentChoice?.Value.Index >= VisibleOptions)
        {
            MenuStart = CurrentChoice;
            for (int i = 0; i < VisibleOptions - 1; i++)
                MenuStart = MenuStart?.Previous;
        }
        else
        {
            MenuStart = CurrentChoice!.Value.Index >= VisibleOptions ? MenuStart!.Previous : CurrentChoice.List?.First;
        }

        UpdateCenterHtml();
    }

    public void JumpToTop()
    {
        if (CurrentChoice == null || MainMenu == null || CurrentChoice.List == null || CurrentChoice == CurrentChoice.List.First)
            return;

        CurrentChoice = CurrentChoice.List.First;
        MenuStart = CurrentChoice?.List?.First;
        UpdateCenterHtml();
    }

    public void JumpToBottom()
    {
        if (CurrentChoice == null || MainMenu == null || CurrentChoice.List == null || CurrentChoice == CurrentChoice.List.Last)
            return;

        CurrentChoice = CurrentChoice.List.Last;
        LinkedListNode<ButtonMenuOption>? newMenuStart = CurrentChoice;
        for (int i = 0; i < VisibleOptions - 1; i++)
        {
            if (newMenuStart?.Previous != null)
            {
                newMenuStart = newMenuStart.Previous;
            }
            else
            {
                newMenuStart = CurrentChoice?.List?.First;
                break;
            }
        }
        MenuStart = newMenuStart;
        UpdateCenterHtml();
    }

    private void UpdateCenterHtml()
    {
        if (CurrentChoice == null || MainMenu == null || Plugin.Instance == null)
            return;

        StringBuilder builder = new StringBuilder();
        int linesRenderedContent = 0;

        bool hasFooterActually = !string.IsNullOrEmpty(Plugin.Instance!.Localizer["menu.bottom.text"]);

        ButtonMenuFontSize actualHeaderFontSize = CurrentHeaderDisplayFontSize;
        ButtonMenuFontSize actualFooterFontSize = CurrentFooterDisplayFontSize;
        ButtonMenuFontSize actualOptionCountFontSize = ButtonMenuFontSize.S; 

        // --- Header Section ---
        bool hasHeader = !string.IsNullOrEmpty(MainMenu?.Title);
        if (hasHeader)
        {
            string headerFontClass = MenuExtensions.GetCssClassForFontSize(actualHeaderFontSize);
            string headerColor = Plugin.Instance.Localizer["menu.title.color"] ?? "red";


            if (Plugin.Instance!.Config.OptionCount)
            {
                builder.AppendLine($"<font class='{headerFontClass}' color='{headerColor}'>{MainMenu?.Title}</font></u><font class='{MenuExtensions.GetCssClassForFontSize(actualOptionCountFontSize)} stratum-bold-italic'>{CurrentChoice?.Value?.Index + 1}/{CurrentChoice?.List?.Count}</font></font><br>");
            }
            else
            {
                builder.AppendLine($"<font class='{headerFontClass}' color='{headerColor}'>{MainMenu?.Title}</font><br>");
            }
            linesRenderedContent++;
        }

        // --- Menu Items Section ---
        LinkedListNode<ButtonMenuOption>? option = MenuStart!;
        int itemsCountedForDisplay = 0;
        int maxItemsToRenderInLoop = VisibleOptions;

        while (itemsCountedForDisplay < maxItemsToRenderInLoop && option != null)
        {
            string itemFontClass = MenuExtensions.GetCssClassForFontSize(CurrentItemDisplayFontSize);
            string fontColorKey = option?.Value.Disabled == true ? "menu.disabled.color" : "menu.enabled.color";
            string fontColor = Plugin.Instance?.Localizer[fontColorKey] ?? "white";

            string fontTagStart = $"<font class='{itemFontClass}' color='{fontColor}'>";
            string fontTagEnd = "</font>";

            if (option == CurrentChoice)
            {
                builder.Append($"{Plugin.Instance?.Localizer["menu.selection.left"]}{fontTagStart}{option?.Value?.OptionDisplay}{fontTagEnd}{Plugin.Instance?.Localizer?["menu.selection.right"]}");
            }
            else
            {
                builder.Append($"{fontTagStart}{option?.Value.OptionDisplay}{fontTagEnd}");
            }

            builder.Append("<br>");
            option = option?.Next;
            itemsCountedForDisplay++;
            linesRenderedContent++;
        }

        // --- Separator and Footer Section ---
        if (hasFooterActually)
        {
            builder.Append("<font class='fontSize-xs'>&nbsp;</font><br>");
            string footerFontClass = MenuExtensions.GetCssClassForFontSize(actualFooterFontSize);
            builder.Append($"<font class='{footerFontClass}'>{Plugin.Instance?.Localizer["menu.bottom.text"]}</font>");
            linesRenderedContent += 2;
        }

        CenterHtml = builder.ToString();
    }
}