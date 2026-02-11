// Included Libraries
using PlayerSettings;
using System.Reflection;
using MenuManagerAPI.Models;
using System.ComponentModel;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using MenuManagerAPI.Shared.Models;
using CounterStrikeSharp.API.Modules.Memory;

// Declare namespace
namespace MenuManagerAPI.CrossCutting
{
    // Define class
    public static class MenuExtensions
    {
        // Define class methods
        public static string NormalizeMenuText(this string input, string fontClass, string styleClasses, string defaultColor)
        {
            if (string.IsNullOrEmpty(input))
                return $"<font class='{fontClass}{(string.IsNullOrEmpty(styleClasses) ? "" : " " + styleClasses)}' color='{defaultColor}'></font>";

            // Map tokens to colors (can be extended)
            var colorMap = new Dictionary<string, string>
            {
                { "{green}", "lime" },
                { "{red}", "red" },
                { "{yellow}", "yellow" },
                { "{blue}", "blue" },
                // Add more as needed
            };

            // Regex to match color tokens
            var tokenRegex = new System.Text.RegularExpressions.Regex(@"\{(green|red|yellow|blue)\}", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            var closeTagRegex = new System.Text.RegularExpressions.Regex(@"\{\/\}");

            // Build output
            var output = new System.Text.StringBuilder();
            int lastIndex = 0;
            var matches = tokenRegex.Matches(input);
            if (matches.Count == 0)
            {
                // No tokens, wrap whole string in config font class, style, and color
                output.Append($"<font class='{fontClass}{(string.IsNullOrEmpty(styleClasses) ? "" : " " + styleClasses)}' color='{defaultColor}'>");
                output.Append(input);
                output.Append("</font>");
                return output.ToString();
            }

            // There are tokens, process segments
            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                // Append text before token, wrapped in config font class, style, and color if not empty
                if (match.Index > lastIndex)
                {
                    string segment = input.Substring(lastIndex, match.Index - lastIndex);
                    if (!string.IsNullOrEmpty(segment))
                    {
                        output.Append($"<font class='{fontClass}{(string.IsNullOrEmpty(styleClasses) ? "" : " " + styleClasses)}' color='{defaultColor}'>");
                        output.Append(segment);
                        output.Append("</font>");
                    }
                }
                // Append token as font tag, using config font class and style, but token color
                string token = match.Value.ToLower();
                string color = colorMap.ContainsKey(token) ? colorMap[token] : defaultColor;
                output.Append($"<font class='{fontClass}{(string.IsNullOrEmpty(styleClasses) ? "" : " " + styleClasses)}' color='{color}'>");
                lastIndex = match.Index + match.Length;
            }
            // Append remainder after last token
            if (lastIndex < input.Length)
            {
                string segment = input.Substring(lastIndex);
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

        public static void SetMenuType(CCSPlayerController player, MenuType type)
        {
            var name = Enum.GetName(type.GetType(), type);
            if (name != null)
            {
                settings?.SetPlayerSettingsValue(player, "menutype", name);
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