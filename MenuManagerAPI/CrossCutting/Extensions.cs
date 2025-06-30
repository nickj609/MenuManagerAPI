using PlayerSettings;
using System.Reflection;
using MenuManagerAPI.Models;
using System.ComponentModel;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using MenuManagerAPI.Shared.Models;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Utils;
using System.Drawing;
using Microsoft.Extensions.Logging;
namespace MenuManagerAPI.CrossCutting
{
    public static class MenuExtensions
    {
        public const int MAX_VISIBLE_OPTIONS = 5;
        public const FontSize DefaultHeaderFontSize = FontSize.M;
        public const FontSize DefaultItemFontSize = FontSize.SM;
        public const FontSize DefaultFooterFontSize = FontSize.S;

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

    public static class PlayerExtensions
    {
        private static ISettingsApi? settings;

        public static void LoadSettings(ISettingsApi _settings)
        {
            settings = _settings;
        }

        public static string GetDescription(this ScreenResolution res)
        {
            FieldInfo fi = res.GetType().GetField(res.ToString())!;

            if (fi != null)
            {
                DescriptionAttribute[] attributes =
                    (DescriptionAttribute[])fi.GetCustomAttributes(
                    typeof(DescriptionAttribute),
                    false);

                if (attributes.Length > 0)
                {
                    return attributes[0].Description;
                }
            }
            return res.ToString();
        }

        public static void PlaySound(CCSPlayerController player, string sound)
        {
            if (sound != "")
            {
                player.ExecuteClientCommand("play " + sound);
            }
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

        public static ScreenResolution GetResolution(CCSPlayerController player)
        {
            string? res = settings?.GetPlayerSettingsValue(player, "resolution", Enum.GetName(ScreenResolution.R1920x1080)!);

            if (res != null)
            {
                try
                {
                    return (ScreenResolution)Enum.Parse(typeof(ScreenResolution), res);
                }
                catch (Exception)
                {
                    return ScreenResolution.R1920x1080;
                }
            }
            return ScreenResolution.R1920x1080;
        }

        public static void SetResolution(CCSPlayerController player, ScreenResolution res)
        {
            var name = Enum.GetName(res.GetType(), res);
            if (name != null)
            {
                settings?.SetPlayerSettingsValue(player, "resolution", name);
                player.PrintToChat($"{Plugin.Instance?.Localizer["resolution.selected"]} {res.GetDescription()}");
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

        // Modified CreateFakeWorldText to use properties from schwarper/CS2MenuManager's Library.cs
        public static CPointWorldText? CreateFakeWorldText(
            this CCSPlayerController player,
            string text,
            Vector position,
            float sizeMultiplier, // Renamed from 'scale' to avoid confusion with FontSize 'size' parameter
            Color color,
            string font = "Arial", // Added font parameter with default
            bool drawBackground = false, // Added drawBackground parameter with default
            float depthOffset = 0.1f // Added depthOffset parameter with default
        )
        {
            if (!player.IsValid || player.IsBot || player.IsHLTV) return null;

            try
            {
                CPointWorldText entity = Utilities.CreateEntityByName<CPointWorldText>("point_worldtext")!;

                if (entity == null || !entity.IsValid)
                {
                    // Ensure you have a Plugin.Instance accessible with a Logger, e.g., via your main plugin class.
                    Plugin.Instance?.Logger.LogError($"Failed to create CPointWorldText entity for player {player.PlayerName}. Entity is null or invalid.");
                    return null;
                }

                // Set properties based on schwarper/CS2MenuManager's Library.cs
                entity.MessageText = text;
                entity.Enabled = true;
                entity.FontSize = (int)(sizeMultiplier * 35); // Adjusted to use sizeMultiplier for flexibility
                entity.Fullbright = true;
                entity.Color = color;
                // Adopted the dynamic WorldUnitsPerPx calculation from schwarper's repo
                entity.WorldUnitsPerPx = 0.25f / 1050 * entity.FontSize;
                // Removed entity.BackgroundWorldToUV as it was not in schwarper's and caused issues
                entity.FontName = font; // Now configurable
                entity.JustifyHorizontal = PointWorldTextJustifyHorizontal_t.POINT_WORLD_TEXT_JUSTIFY_HORIZONTAL_LEFT;
                entity.JustifyVertical = PointWorldTextJustifyVertical_t.POINT_WORLD_TEXT_JUSTIFY_VERTICAL_CENTER;
                entity.ReorientMode = PointWorldTextReorientMode_t.POINT_WORLD_TEXT_REORIENT_NONE;
                entity.RenderMode = RenderMode_t.kRenderNormal;

                entity.DrawBackground = drawBackground; // Now configurable
                if (drawBackground)
                {
                    entity.BackgroundBorderHeight = 0.1f;
                    entity.BackgroundBorderWidth = 0.1f;
                }
                else
                {
                    // Ensure background is explicitly off if not drawn
                    entity.BackgroundBorderHeight = 0f;
                    entity.BackgroundBorderWidth = 0f;
                }

                entity.DepthOffset = depthOffset; // Now configurable

                // DispatchSpawn and Teleport are critical for the entity to appear and be positioned.
                entity.DispatchSpawn();
                entity.Teleport(position, new QAngle(0, 0, 0), null);

                return entity;
            }
            catch (Exception ex)
            {
                Plugin.Instance?.Logger.LogError($"Error creating world text entity for player {player.PlayerName}: {ex.Message}");
                return null;
            }
        }
    }
}