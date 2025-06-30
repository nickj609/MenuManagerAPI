// Included libraries
using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Utils;
using MenuManagerAPI.Shared.Models;

// Define namespace for rendering components
namespace MenuManagerAPI.GameModeManager.Rendering
{
    /// <summary>
    /// Provides utility methods for managing display of world text and player view information.
    /// Adapted from CS2ScreenMenuAPI.
    /// </summary>
    public static class DisplayManager
    {
        /// <summary>
        /// Represents position and angle data for world text placement.
        /// </summary>
        public readonly record struct VectorData(Vector Position, QAngle Angle);

        /// <summary>
        /// Retrieves observer information for a player, including mode and observed target.
        /// </summary>
        /// <param name="player">The player controller.</param>
        /// <returns>An <see cref="ObserverInfo"/> struct containing observer details.</returns>
        public static ObserverInfo GetObserverInfo(this CCSPlayerController player)
        {
            if (player.Pawn.Value is not CBasePlayerPawn pawn)
                return new(ObserverMode.Roaming, null);

            if (pawn.ObserverServices is not CPlayer_ObserverServices observerServices)
                return new(ObserverMode.FirstPerson, pawn.As<CCSPlayerPawnBase>());

            var observerMode = (ObserverMode_t)observerServices.ObserverMode;
            var observing = observerServices.ObserverTarget?.Value?.As<CCSPlayerPawnBase>();

            return new()
            {
                Mode = observerMode switch
                {
                    ObserverMode_t.OBS_MODE_IN_EYE => ObserverMode.FirstPerson,
                    ObserverMode_t.OBS_MODE_CHASE => ObserverMode.ThirdPerson,
                    _ => ObserverMode.Roaming,
                },
                Observing = observing,
            };
        }

        /// <summary>
        /// Represents eye angles and position of a player or observed target.
        /// </summary>
        public struct EyeAngles
        {
            public System.Numerics.Vector3 Position { get; set; }
            public System.Numerics.Vector3 Angle { get; set; }
            public System.Numerics.Vector3 Forward { get; set; }
            public System.Numerics.Vector3 Right { get; set; }
            public System.Numerics.Vector3 Up { get; set; }
        }

        private static readonly Vector _Forward = new();
        private static readonly Vector _Right = new();
        private static readonly Vector _Up = new();

        /// <summary>
        /// Gets the eye angles and vectors for the observed entity.
        /// </summary>
        /// <param name="observerInfo">The observer information.</param>
        /// <returns>An <see cref="EyeAngles"/> struct if available, otherwise null.</returns>
        public static EyeAngles? GetEyeAngles(this ObserverInfo observerInfo)
        {
            if (observerInfo.Observing is not CCSPlayerPawnBase pawn) return null;

            var eyeAngles = pawn.EyeAngles;
            NativeAPI.AngleVectors(eyeAngles.Handle, _Forward.Handle, _Right.Handle, _Up.Handle);

            var origin = new System.Numerics.Vector3(pawn.AbsOrigin!.X, pawn.AbsOrigin!.Y, pawn.AbsOrigin!.Z);
            var viewOffset = new System.Numerics.Vector3(pawn.ViewOffset.X, pawn.ViewOffset.Y, pawn.ViewOffset.Z);

            return new()
            {
                Position = origin + viewOffset,
                Angle = new System.Numerics.Vector3(eyeAngles.X, eyeAngles.Y, eyeAngles.Z),
                Forward = new System.Numerics.Vector3(_Forward.X, _Forward.Y, _Forward.Z),
                Right = new System.Numerics.Vector3(_Right.X, _Right.Y, _Right.Z),
                Up = new System.Numerics.Vector3(_Up.X, _Up.Y, _Up.Z),
            };
        }

        /// <summary>
        /// Finds the vector data for displaying world text based on player's position and resolution.
        /// </summary>
        /// <param name="player">The player controller.</param>
        /// <param name="resolution">The player's resolution settings.</param>
        /// <param name="size">Optional size of the text, used for FOV scaling.</param>
        /// <returns>A <see cref="VectorData"/> struct if successful, otherwise null.</returns>
        public static VectorData? FindVectorData(CCSPlayerController player, ScreenResolution resolution, float? size = null)
        {
            CCSPlayerPawn? playerPawn = GetPlayerPawn(player);
            if (playerPawn == null)
                return null;

            QAngle eyeAngles = playerPawn.EyeAngles;
            Vector forward = new(), right = new(), up = new();
            NativeAPI.AngleVectors(eyeAngles.Handle, forward.Handle, right.Handle, up.Handle);

            // Fix This, should be using resolution type
            float currentX = -9.0f;
            float currentY = 0.0f;
            float currentSize = size ?? 32; // Default size if not provided

            if (size.HasValue)
            {
                (float newX, float newY, float newSize) = GetWorldTextPosition(player, currentX, currentY, currentSize);
                currentX = newX;
                currentY = newY;
                currentSize = newSize;
            }

            Vector offset = forward * 7 + right * currentX + up * currentY;
            QAngle angle = new()
            {
                Y = eyeAngles.Y + 270,
                Z = 90 - eyeAngles.X,
                X = 0
            };

            return new VectorData()
            {
                Position = playerPawn.AbsOrigin! + offset + new Vector(0, 0, playerPawn.ViewOffset.Z),
                Angle = angle,
            };
        }

        /// <summary>
        /// Gets the player's pawn, handling observer targets if the player is dead.
        /// </summary>
        /// <param name="player">The player controller.</param>
        /// <returns>The <see cref="CCSPlayerPawn"/> if valid, otherwise null.</returns>
        public static CCSPlayerPawn? GetPlayerPawn(CCSPlayerController player)
        {
            if (player.Pawn.Value is not CBasePlayerPawn pawn)
                return null;

            if (pawn.LifeState == (byte)LifeState_t.LIFE_DEAD)
            {
                if (pawn.ObserverServices?.ObserverTarget.Value?.As<CBasePlayerPawn>() is not CBasePlayerPawn observer)
                    return null;

                pawn = observer;
            }

            return pawn.As<CCSPlayerPawn>();
        }

        /// <summary>
        /// Adjusts world text position and size based on the player's FOV.
        /// </summary>
        /// <param name="controller">The player controller.</param>
        /// <param name="x">The initial X-position.</param>
        /// <param name="y">The initial Y-position.</param>
        /// <param name="size">The initial font size.</param>
        /// <returns>A tuple containing adjusted X, Y, and Size.</returns>
        private static (float x, float y, float size) GetWorldTextPosition(CCSPlayerController controller, float x, float y, float size)
        {
            float fov = controller.DesiredFOV == 0 ? 90 : controller.DesiredFOV;

            if (fov == 90)
                return (x, y, size);

            float scaleFactor = (float)Math.Tan((fov / 2) * Math.PI / 180) / (float)Math.Tan(45 * Math.PI / 180);

            float newX = x * scaleFactor;
            float newY = y * scaleFactor;
            float newSize = size * scaleFactor;

            return (newX, newY, newSize);
        }

        /// <summary>
        /// Ensures a custom view model exists for the player, used for parenting world text.
        /// </summary>
        /// <param name="player">The player controller.</param>
        /// <returns>The <see cref="CCSGOViewModel"/> if successful, otherwise null.</returns>
        public static CCSGOViewModel? EnsureCustomView(CCSPlayerController player)
        {
            var pawn = GetPlayerPawn(player);
            if (pawn == null || pawn.ViewModelServices == null)
                return null;

            int offset = Schema.GetSchemaOffset("CCSPlayer_ViewModelServices", "m_hViewModel");
            IntPtr viewModelHandleAddress = pawn.ViewModelServices.Handle + offset + 4;

            CHandle<CCSGOViewModel> handle = new(viewModelHandleAddress);
            if (!handle.IsValid)
            {
                CCSGOViewModel viewmodel = Utilities.CreateEntityByName<CCSGOViewModel>("predicted_viewmodel")!;
                viewmodel.DispatchSpawn();
                handle.Raw = viewmodel.EntityHandle.Raw;
                Utilities.SetStateChanged(pawn, "CCSPlayerPawnBase", "m_pViewModelServices");
            }

            return handle.Value;
        }

        /// <summary>
        /// Creates a <see cref="CPointWorldText"/> entity for display in the world.
        /// </summary>
        /// <param name="text">The text content.</param>
        /// <param name="size">The font size.</param>
        /// <param name="color">The text color.</param>
        /// <param name="font">The font name.</param>
        /// <param name="background">Whether to draw a background.</param>
        /// <param name="backgroundColor">The background color.</param>
        /// <param name="offset">Depth offset for layering.</param>
        /// <param name="position">The world position.</param>
        /// <param name="angle">The world angle.</param>
        /// <param name="viewModel">The view model to parent the text to.</param>
        /// <param name="backgroundXOffset">X-offset for background positioning.</param>
        /// <returns>The created <see cref="CPointWorldText"/> entity, or null if creation fails.</returns>
        public static CPointWorldText? CreateWorldText(
            string text,
            int size,
            Color color,
            string font,
            bool background,
            Color backgroundColor,
            float offset,
            Vector position,
            QAngle angle,
            CCSGOViewModel viewModel,
            float backgroundXOffset = 0f)
        {
            CPointWorldText entity = Utilities.CreateEntityByName<CPointWorldText>("point_worldtext")!;

            if (entity == null || !entity.IsValid)
                return null;

            if (backgroundXOffset != 0f)
            {
                Vector forward = new(), left = new(), up = new();
                NativeAPI.AngleVectors(angle.Handle, forward.Handle, left.Handle, up.Handle);

                Vector newPosition = position + (left * backgroundXOffset);
                position = newPosition;
            }

            entity.MessageText = text;
            entity.Enabled = true;
            entity.FontSize = size;
            entity.Fullbright = true;
            entity.Color = color;
            entity.WorldUnitsPerPx = 0.0085f;
            entity.BackgroundWorldToUV = 0.01f;
            entity.FontName = font;
            entity.JustifyHorizontal = PointWorldTextJustifyHorizontal_t.POINT_WORLD_TEXT_JUSTIFY_HORIZONTAL_LEFT;
            entity.JustifyVertical = PointWorldTextJustifyVertical_t.POINT_WORLD_TEXT_JUSTIFY_VERTICAL_CENTER;
            entity.ReorientMode = PointWorldTextReorientMode_t.POINT_WORLD_TEXT_REORIENT_NONE;
            entity.RenderMode = RenderMode_t.kRenderNormal;

            if (background)
            {
                entity.DrawBackground = true;
                entity.BackgroundBorderHeight = 0.1f;
                entity.BackgroundBorderWidth = 0.1f;
            }

            entity.DepthOffset = offset;

            entity.DispatchSpawn();
            entity.Teleport(position, angle, null);
            entity.AcceptInput("SetParent", viewModel, null, "!activator");

            return entity;
        }
    }

    /// <summary>
    /// Enum to represent different observer modes.
    /// </summary>
    public enum ObserverMode
    {
        FirstPerson,
        ThirdPerson,
        Roaming,
    }

    /// <summary>
    /// Record struct to hold observer mode and observed pawn.
    /// </summary>
    public readonly record struct ObserverInfo(ObserverMode Mode, CCSPlayerPawnBase? Observing);
}
