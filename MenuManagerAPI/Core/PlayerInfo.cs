// Included Libraries
using System.Text;
using MenuManagerAPI.Menus;
using MenuManagerAPI.Models;
using CounterStrikeSharp.API;
using MenuManagerAPI.CrossCutting;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;

// Declare namespace
namespace MenuManagerAPI.Core;

// Define class
public class PlayerInfo
{
    // Define class fields and properties
    private string CenterHtml = "";
    private ButtonMenu? MainMenu = null;
    private bool SelectionProcessing = false;
    public PlayerButtons Buttons { get; set; }
    public bool MenuOpen { get; set; } = false;
    public bool ButtonMenuOpen => MainMenu != null;
    public float VelocityModifier { get; set; } = 0.0f;
    public required CCSPlayerController player { get; set; }
    public int VisibleOptions = MenuExtensions.MAX_VISIBLE_OPTIONS; 
    public LinkedListNode<ButtonMenuOption>? MenuStart { get; set; }
    public LinkedListNode<ButtonMenuOption>? CurrentChoice { get; set; }
    public FontSize CurrentItemDisplayFontSize { get; private set; } = MenuExtensions.DefaultItemFontSize;
    public FontSize CurrentHeaderDisplayFontSize { get; private set; } = MenuExtensions.DefaultHeaderFontSize;
    public FontSize CurrentFooterDisplayFontSize { get; private set; } = MenuExtensions.DefaultFooterFontSize;

    // Define class methods
    public void OpenMenu(ButtonMenu? menu)
    {
        if (MainMenu == null)
        {
            // Register this menu as open for the player
            ButtonMenuManager.OpenMenu(this);

            // Freeze player
            if (Plugin.Instance != null && !Plugin.Instance.Config.ButtonMenu.MoveWithOpenMenu)
            {
                if (Plugin.Instance.Config.ButtonMenu.UseVelocityModifier)
                {
                    if (Plugin.Players.ContainsKey(player.Slot))
                        Plugin.Players[player.Slot].VelocityModifier = player.PlayerPawn.Value!.VelocityModifier;
                }
                else
                {
                    PlayerExtensions.Freeze(player.Pawn.Value!);
                }
            }

            // Set the main menu for this player
            MainMenu = menu;
            VisibleOptions = MenuExtensions.MAX_VISIBLE_OPTIONS; 
            CurrentChoice = MainMenu?.ButtonOptions.First;
        }
        else // This block handles opening sub-menus
        {
            menu!.Prev = CurrentChoice;
            VisibleOptions = MenuExtensions.MAX_VISIBLE_OPTIONS; 
            CurrentChoice = menu.ButtonOptions.First;
        }

        // Display the menu to the player
        MenuStart = CurrentChoice;
        UpdateCenterHtml();
    }

    public void CloseMenu()
    {
        if (Plugin.Instance != null)
        {
            // Unregister this menu as open for the player
            ButtonMenuManager.CloseMenu(this);

            // Unfreeze player
            if (!Plugin.Instance.Config.ButtonMenu.MoveWithOpenMenu)
            {
                if (Plugin.Instance.Config.ButtonMenu.UseVelocityModifier)
                {
                    if (player.PlayerPawn.Value != null && Plugin.Players.ContainsKey(player.Slot))
                        player.PlayerPawn.Value.VelocityModifier = Plugin.Players[player.Slot].VelocityModifier;
                }
                else
                {
                    PlayerExtensions.Unfreeze(player.Pawn.Value!);
                }
            }
        }

        // Clear menu data
        MenuOpen = false;
        MainMenu = null;
        CurrentChoice = null;
        CenterHtml = "";
    }

    public void OnTick()
    {
        if (Plugin.Instance == null)
            return;

        if (Plugin.Instance.Config.ButtonMenu.UseVelocityModifier)
            player.PlayerPawn.Value!.VelocityModifier = 0.0f;

        if ((Buttons & Plugin.Instance.Config.ButtonMenu.ButtonsConfig.UpButton) == 0 && (player.Buttons & Plugin.Instance.Config.ButtonMenu.ButtonsConfig.UpButton) != 0)
        {
            PlayerExtensions.PlaySound(player, Plugin.Instance!.Config!.ButtonMenu.ButtonSounds.Scroll);
            ScrollUp();
        }
        else if ((Buttons & Plugin.Instance.Config.ButtonMenu.ButtonsConfig.DownButton) == 0 && (player.Buttons & Plugin.Instance.Config.ButtonMenu.ButtonsConfig.DownButton) != 0)
        {
            PlayerExtensions.PlaySound(player, Plugin.Instance!.Config.ButtonMenu.ButtonSounds.Scroll);
            ScrollDown();
        }
        else if ((Buttons & Plugin.Instance.Config.ButtonMenu.ButtonsConfig.SelectButton) == 0 && (player.Buttons & Plugin.Instance.Config.ButtonMenu.ButtonsConfig.SelectButton) != 0)
        {
            Choose();
        }
        else if ((Buttons & Plugin.Instance.Config.ButtonMenu.ButtonsConfig.BackButton) == 0 && (player.Buttons & Plugin.Instance.Config.ButtonMenu.ButtonsConfig.BackButton) != 0)
        {
            if (MainMenu?.BackAction != null)
            {
                MainMenu.BackAction(player);
                return;
            }

            if (CurrentChoice?.Value.Parent?.Prev != null)
            {
                PlayerExtensions.PlaySound(player, Plugin.Instance!.Config.ButtonMenu.ButtonSounds.Back);
                GoBackToPrev(CurrentChoice?.Value.Parent.Prev);
            }
        }
        else if (MainMenu != null && MainMenu.ExitButton &&
            (Buttons & Plugin.Instance.Config.ButtonMenu.ButtonsConfig.ExitButton) == 0 &&
            (player.Buttons & Plugin.Instance.Config.ButtonMenu.ButtonsConfig.ExitButton) != 0)
        {
            PlayerExtensions.PlaySound(player, Plugin.Instance!.Config.ButtonMenu.ButtonSounds.Exit);
            CloseMenu();
        }
        else if ((Buttons & Plugin.Instance.Config.ButtonMenu.ButtonsConfig.LeftButton) == 0 && (player.Buttons & Plugin.Instance.Config.ButtonMenu.ButtonsConfig.LeftButton) != 0)
        {
            PlayerExtensions.PlaySound(player, Plugin.Instance!.Config.ButtonMenu.ButtonSounds.Scroll);
            JumpToTop();
        }
        else if ((Buttons & Plugin.Instance.Config.ButtonMenu.ButtonsConfig.RightButton) == 0 && (player.Buttons & Plugin.Instance.Config.ButtonMenu.ButtonsConfig.RightButton) != 0)
        {
            PlayerExtensions.PlaySound(player, Plugin.Instance!.Config.ButtonMenu.ButtonSounds.Scroll);
            JumpToBottom();
        }
        Buttons = player.Buttons;
        if (CenterHtml != "")
            Server.NextFrame(() => player.PrintToCenterHtml(CenterHtml));
    }

    public void Choose()
    {
        if (SelectionProcessing || player == null || CurrentChoice?.Value?.OnChoose == null || Plugin.Instance == null)
            return;

        SelectionProcessing = true;

        if (CurrentChoice.Value.Disabled)
        {
            PlayerExtensions.PlaySound(player, Plugin.Instance.Config.ButtonMenu.ButtonSounds.Disabled);
            SelectionProcessing = false;
        }
        else
        {
            PlayerExtensions.PlaySound(player, Plugin.Instance.Config.ButtonMenu.ButtonSounds.Click);
            CurrentChoice.Value.OnChoose(player, CurrentChoice.Value);

            switch (MainMenu?.PostSelectAction)
            {
                case PostSelectAction.Close:
                    CloseMenu();
                    SelectionProcessing = false;
                    break;
                case PostSelectAction.Reset:
                    if (MainMenu.ResetAction != null && !MenuOpen)
                        MainMenu.ResetAction(player);
                    SelectionProcessing = false;
                    break;
                case PostSelectAction.Nothing:
                    SelectionProcessing = false;
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
                    MenuStart = MenuStart?.Previous;
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

        // Use StringBuilderPool for better performance
        StringBuilder builder = StringBuilderPool.Get();
        try
        {
            FontSize actualHeaderFontSize = CurrentHeaderDisplayFontSize;
            FontSize actualFooterFontSize = CurrentFooterDisplayFontSize;
            FontSize actualOptionCountFontSize = FontSize.S; 

            // Header section
            bool hasHeader = !string.IsNullOrEmpty(MainMenu?.Title);
            if (hasHeader)
            {
                var titleStyle = Plugin.Instance.Config.ButtonMenu.Title;
                string headerFontClass = MenuExtensions.GetCssClassForFontSize(titleStyle.FontSize);
                string headerColor = titleStyle.Color;
                string headerStyleClasses = titleStyle.Bold ? "stratum-bold" : string.Empty;
                if (titleStyle.Italic)
                    headerStyleClasses += titleStyle.Bold ? " stratum-bold-italic" : "stratum-italic";

                string normalizedHeader = MainMenu?.Title.FormatHtmlMessage(true, headerColor, headerFontClass, headerStyleClasses) ?? "";
                if (Plugin.Instance!.Config.ButtonMenu.OptionCount)
                {
                    builder.AppendLine($"{normalizedHeader}</u><font class='{MenuExtensions.GetCssClassForFontSize(actualOptionCountFontSize)} stratum-bold-italic'>{CurrentChoice?.Value?.Index + 1}/{CurrentChoice?.List?.Count}</font></font><br>");
                }
                else
                {
                    builder.AppendLine($"{normalizedHeader}<br>");
                }
            }

            // Menu items section
            LinkedListNode<ButtonMenuOption>? option = MenuStart!;
            int itemsCountedForDisplay = 0;
            int maxItemsToRenderInLoop = VisibleOptions;

            while (itemsCountedForDisplay < maxItemsToRenderInLoop && option != null)
            {
                string itemFontClass = MenuExtensions.GetCssClassForFontSize(CurrentItemDisplayFontSize);
                string fontColor = option?.Value.Disabled == true
                    ? Plugin.Instance!.Config.ButtonMenu.DisabledOptionColor
                    : Plugin.Instance!.Config.ButtonMenu.EnabledOptionColor;
                string styleClasses = string.Empty;

                if (option == CurrentChoice && Plugin.Instance != null)
                {
                    string leftSelection = Plugin.Instance.Localizer["menu.selection.left"] ?? string.Empty;
                    string rightSelection = Plugin.Instance.Localizer["menu.selection.right"] ?? string.Empty;
                    var arrowStyle = Plugin.Instance.Config.ButtonMenu.Selection;
                    string arrowFontClass = MenuExtensions.GetCssClassForFontSize(arrowStyle.FontSize);
                    string arrowStyleClasses = arrowStyle.Bold ? "stratum-bold" : string.Empty;
                    if (arrowStyle.Italic) arrowStyleClasses += arrowStyle.Bold ? " stratum-bold-italic" : "stratum-italic";

                    string arrowTag = string.IsNullOrEmpty(arrowStyleClasses)
                        ? $"<font class='{arrowFontClass}' color='{arrowStyle.Color}'>"
                        : $"<font class='{arrowFontClass} {arrowStyleClasses}' color='{arrowStyle.Color}'>";

                    // Parse selection parts: leftSelection = "&#9654; [ " -> arrow + bracket
                    // rightSelection = "] &#9664;" -> bracket + arrow
                    string[] leftParts = leftSelection.Split(new[] { " " }, System.StringSplitOptions.RemoveEmptyEntries);
                    string[] rightParts = rightSelection.Split(new[] { " " }, System.StringSplitOptions.RemoveEmptyEntries);
                    
                    // Build: arrow | bracket | content | bracket | arrow
                    builder.Append($"{arrowTag}" + (leftParts.Length > 0 ? leftParts[0] : "") + "</font> ");
                    builder.Append($"{arrowTag}" + (leftParts.Length > 1 ? string.Join(" ", leftParts.Skip(1)) : "[") + "</font> ");
                    string normalizedOptionDisplay = option?.Value?.OptionDisplay.FormatHtmlMessage(true, fontColor, itemFontClass, styleClasses) ?? "";
                    builder.Append($"{normalizedOptionDisplay} ");
                    builder.Append($"{arrowTag}" + (rightParts.Length > 0 ? rightParts[0] : "]") + "</font> ");
                    builder.Append($"{arrowTag}" + (rightParts.Length > 1 ? rightParts[rightParts.Length - 1] : "") + "</font>");
                }
                else
                {
                    string normalizedOptionDisplay = option?.Value.OptionDisplay.FormatHtmlMessage(true, fontColor, itemFontClass, styleClasses) ?? "";
                    builder.Append(normalizedOptionDisplay);
                }

                builder.Append("<br>");
                option = option?.Next;
                itemsCountedForDisplay++;
            }

            // Footer section
            var footerStyle = Plugin.Instance?.Config.ButtonMenu.Footer;
            string footerFontClass = MenuExtensions.GetCssClassForFontSize(footerStyle?.FontSize ?? actualFooterFontSize);
            string separatorColor = footerStyle?.Separator.Color ?? "white";
            string separatorBoldClass = footerStyle?.Separator.Bold == true ? " class='stratum-bold'" : string.Empty;
            string buttonColor = footerStyle?.Button.Color ?? "gold";
            string buttonBoldClass = footerStyle?.Button.Bold == true ? " class='stratum-bold'" : string.Empty;
            
            // Build footer from lang keys
            string scroll = Plugin.Instance?.Localizer["menu.footer.scroll"] ?? "Scroll";
            string scrollButton = Plugin.Instance?.Localizer["menu.footer.scroll.button"] ?? "W/S";
            string select = Plugin.Instance?.Localizer["menu.footer.select"] ?? "Sel";
            string selectButton = Plugin.Instance?.Localizer["menu.footer.select.button"] ?? "E";
            string previous = Plugin.Instance?.Localizer["menu.footer.previous"] ?? "Prev";
            string previousButton = Plugin.Instance?.Localizer["menu.footer.previous.button"] ?? "CTRL";
            string exit = Plugin.Instance?.Localizer["menu.footer.exit"] ?? "Exit";
            string exitButton = Plugin.Instance?.Localizer["menu.footer.exit.button"] ?? "R";
            
            builder.Append("<font class='fontSize-xs'>&nbsp;</font><br>");
            builder.Append($"<font class='{footerFontClass}'>");

            string normalizedScroll = scroll.FormatHtmlMessage(true, separatorColor, footerFontClass, separatorBoldClass.Trim());
            string normalizedScrollButton = scrollButton.FormatHtmlMessage(true, buttonColor, footerFontClass, buttonBoldClass.Trim());
            string normalizedSelect = select.FormatHtmlMessage(true, separatorColor, footerFontClass, separatorBoldClass.Trim());
            string normalizedSelectButton = selectButton.FormatHtmlMessage(true, buttonColor, footerFontClass, buttonBoldClass.Trim());
            string normalizedPrevious = previous.FormatHtmlMessage(true, separatorColor, footerFontClass, separatorBoldClass.Trim());
            string normalizedPreviousButton = previousButton.FormatHtmlMessage(true, buttonColor, footerFontClass, buttonBoldClass.Trim());
            string normalizedExit = exit.FormatHtmlMessage(true, separatorColor, footerFontClass, separatorBoldClass.Trim());
            string normalizedExitButton = exitButton.FormatHtmlMessage(true, buttonColor, footerFontClass, buttonBoldClass.Trim());
            string normalizedSeparator = "|".FormatHtmlMessage(true, separatorColor, footerFontClass, separatorBoldClass.Trim());

            if (MainMenu != null && MainMenu.ExitButton)
                builder.Append($"{normalizedScroll} {normalizedScrollButton} {normalizedSeparator} {normalizedSelect} {normalizedSelectButton} {normalizedSeparator} {normalizedPrevious} {normalizedPreviousButton} {normalizedSeparator} {normalizedExit} {normalizedExitButton}");
            else
                builder.Append($"{normalizedScroll} {normalizedScrollButton} {normalizedSeparator} {normalizedSelect} {normalizedSelectButton} {normalizedSeparator} {normalizedPrevious} {normalizedPreviousButton}");

            builder.Append("</font>");
            CenterHtml = builder.ToString();
        }
        finally
        {
            // Return StringBuilder to pool
            StringBuilderPool.Return(builder);
        }
    }
    
    public void RefreshDisplay()
    {
        if (MainMenu != null && player.IsValid)
        {
            UpdateCenterHtml();
            player.PrintToCenterHtml(CenterHtml);
        }
    }
}