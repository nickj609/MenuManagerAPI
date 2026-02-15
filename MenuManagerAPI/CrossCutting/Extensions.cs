// Included Libraries
using PlayerSettings;
using System.Reflection;
using MenuManagerAPI.Models;
using System.ComponentModel;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using MenuManagerAPI.Shared.Models;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Commands;

// Declare namespace
namespace MenuManagerAPI.CrossCutting
{
    // Define class
    public static class MenuExtensions
    {
        // Define class properties
        private static readonly Dictionary<string, char> PredefinedColors = typeof(ChatColors)
        .GetFields(BindingFlags.Public | BindingFlags.Static)
        .ToDictionary(field => $"{{{field.Name}}}", field => (char)(field.GetValue(null) ?? '\x01'));

        // Define class methods
        public static string FormatChatMessage(string message)
        {
            string result = message;
            foreach (var color in PredefinedColors)
            {
                result = ReplaceIgnoreCase(result, color.Key, color.Value.ToString());
            }
            return result;
        }

        public static string CleanMessage(string message)
        {
            string result = message;
            foreach (var color in PredefinedColors)
            {
                result = ReplaceIgnoreCase(result, color.Key, "");
                result = result.Replace(color.Value.ToString(), "");
            }
            return result;
        }

        private static string ReplaceIgnoreCase(string input, string search, string replacement)
        {
            return System.Text.RegularExpressions.Regex.Replace(
                input,
                System.Text.RegularExpressions.Regex.Escape(search),
                replacement,
                System.Text.RegularExpressions.RegexOptions.IgnoreCase
            );
        }
        
        public static string FormatHtmlMessage(this string message, bool isButtonMenu = false, string defaultColor = "white", string fontClass = "", string styleClasses = "")
        {
            // Return empty font tag if message is null or empty to ensure consistent formatting
            if (string.IsNullOrEmpty(message))
            {
                if (isButtonMenu)
                    return $"<font class='{fontClass}{(string.IsNullOrEmpty(styleClasses) ? "" : " " + styleClasses)}' color='{defaultColor}'></font>";
                else
                    return $"<font color='{defaultColor}'></font>";   
            }
            
            // Regex to match any color token: {color} or {#hex}
            var tokenRegex = new System.Text.RegularExpressions.Regex(@"\{([^}]+)\}", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            var closeTagRegex = new System.Text.RegularExpressions.Regex(@"\{\/\}");

            // Build output
            var output = new System.Text.StringBuilder();
            int lastIndex = 0;
            var matches = tokenRegex.Matches(message);
            if (matches.Count == 0)
            {
                // No tokens, wrap whole string in config font class, style, and color
                if (isButtonMenu)
                    output.Append($"<font class='{fontClass}{(string.IsNullOrEmpty(styleClasses) ? "" : " " + styleClasses)}' color='{defaultColor}'>");
                else
                    output.Append($"<font color='{defaultColor}'>");
                    
                output.Append(message);
                output.Append("</font>");
                return output.ToString();
            }

            // There are tokens, process segments
            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                // Append text before token, wrapped in config font class, style, and color if not empty
                if (match.Index > lastIndex)
                {
                    string segment = message.Substring(lastIndex, match.Index - lastIndex);
                    if (!string.IsNullOrEmpty(segment))
                    {
                        if (isButtonMenu)
                            output.Append($"<font class='{fontClass}{(string.IsNullOrEmpty(styleClasses) ? "" : " " + styleClasses)}' color='{defaultColor}'>");
                        else
                            output.Append($"<font color='{defaultColor}'>");
                            
                        output.Append(segment);
                        output.Append("</font>");
                    }
                }
                // Use token value directly as color
                string color = match.Groups[1].Value;

                if (isButtonMenu)
                    output.Append($"<font class='{fontClass}{(string.IsNullOrEmpty(styleClasses) ? "" : " " + styleClasses)}' color='{color}'>");
                else
                    output.Append($"<font color='{color}'>");

                lastIndex = match.Index + match.Length;
            }
            // Append remainder after last token
            if (lastIndex < message.Length)
            {
                string segment = message.Substring(lastIndex);

                // Handle close tag tokens
                segment = closeTagRegex.Replace(segment, "</font>");

                // Remove unknown tokens
                segment = System.Text.RegularExpressions.Regex.Replace(segment, "\\{[a-zA-Z]+\\}", "");
                output.Append(segment);
                output.Append("</font>");
            }
            return output.ToString();
        }

        // Define class constants and properties
        public const int MAX_VISIBLE_OPTIONS = 5;
        public const FontSize DefaultHeaderFontSize = FontSize.M;
        public const FontSize DefaultItemFontSize = FontSize.SM;
        public const FontSize DefaultFooterFontSize = FontSize.S;

        // Define class methods
        public static string GetDescription(this FontSize value)
        {
            FieldInfo? fieldInfo = value.GetType().GetField(value.ToString());
            if (fieldInfo == null)
            {
                return value.ToString();
            }
            DescriptionAttribute? attribute = fieldInfo.GetCustomAttribute<DescriptionAttribute>();
            return attribute?.Description ?? value.ToString();
        }

        public static string GetCssClassForFontSize(FontSize fontSize)
        {
            return fontSize.GetDescription();
        }
    }

    // Define class
    public static class PlayerExtensions
    {
        // Define class properties
        private static ISettingsApi? settings;

        // Define class methods
        public static void LoadSettings(ISettingsApi _settings)
        {
            settings = _settings;
        }

        public static void PlaySound(CCSPlayerController player, string sound)
        {
            PlaySound(player, sound, 1.0f);
        }

        public static void PlaySound(CCSPlayerController player, string sound, float volume)
        {
            if (string.IsNullOrWhiteSpace(sound))
                return;

            if (volume <= 0f)
                return;

            if (volume >= 0.99f)
            {
                player.ExecuteClientCommand("play " + sound);
                return;
            }

            string volumeText = volume.ToString(System.Globalization.CultureInfo.InvariantCulture);
            player.ExecuteClientCommand("playvol " + sound + " " + volumeText);
        }

        public static void Freeze(this CBasePlayerPawn pawn)
        {
            pawn.MoveType = MoveType_t.MOVETYPE_OBSOLETE;
            Schema.SetSchemaValue(pawn.Handle, "CBaseEntity", "m_nActualMoveType", 1);
            Utilities.SetStateChanged(pawn, "CBaseEntity", "m_MoveType");
        }

        public static void Unfreeze(this CBasePlayerPawn pawn)
        {
            pawn.MoveType = MoveType_t.MOVETYPE_WALK;
            Schema.SetSchemaValue(pawn.Handle, "CBaseEntity", "m_nActualMoveType", 2);
            Utilities.SetStateChanged(pawn, "CBaseEntity", "m_MoveType");
        }

        public static CCSPlayerController[] ValidPlayers(bool considerBots = false)
        {
            return Utilities.GetPlayers()
                .Where(x => x.ReallyValid(considerBots))
                .Where(x => !x.IsHLTV)
                .Where(x => considerBots || !x.IsBot)
                .ToArray();
        }

        public static bool ReallyValid(this CCSPlayerController? player, bool considerBots = false)
        {
            return player is not null && player.IsValid && player.Connected == PlayerConnectedState.PlayerConnected &&
                (considerBots || (!player.IsBot && !player.IsHLTV));
        }

        public static MenuType GetMenuType(CCSPlayerController player)
        {
            string? menuType = settings?.GetPlayerSettingsValue(player, "menutype", GetMenuTypeName(Plugin.Instance!.Config.DefaultMenu));
            if (menuType != null)
            {
                try
                {
                    return (MenuType)Enum.Parse(typeof(MenuType), menuType);
                }
                catch (Exception)
                {
                    return Plugin.Instance!.Config.DefaultMenu;
                }
            }
            return Plugin.Instance!.Config.DefaultMenu;
        }

        public static void SetMenuType(CCSPlayerController player, MenuType type, CommandInfo? command = null)
        {
            var name = Enum.GetName(type.GetType(), type);
            if (name != null)
            {
                settings?.SetPlayerSettingsValue(player, "menutype", name);
                if (command != null)
                    command.ReplyToCommand($"{Plugin.Instance?.Localizer["menutype.selected"]} {GetMenuTypeName(type)}");
                else
                    player.PrintToChat($"{Plugin.Instance?.Localizer["menutype.selected"]} {GetMenuTypeName(type)}");
            }
        }

        public static string GetMenuTypeName(MenuType type)
        {
            if (Plugin.Instance != null)
            {
                switch (type)
                {
                    case MenuType.ChatMenu: return Plugin.Instance.Localizer["menutype.chat"];
                    case MenuType.ConsoleMenu: return Plugin.Instance.Localizer["menutype.console"];
                    case MenuType.CenterMenu: return Plugin.Instance.Localizer["menutype.center"];
                    case MenuType.ButtonMenu: return Plugin.Instance.Localizer["menutype.button"];
                    default: return "Undefined";
                }
            }
            return "Undefined";
        }
    }
}