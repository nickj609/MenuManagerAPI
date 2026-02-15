// Included libraries
using CounterStrikeSharp.API.Core;

// Define namespace
namespace MenuManagerAPI.Core
{
    // Define class
    public static class ButtonMenuManager
    {
        // Define class properties
        public static IEnumerable<PlayerInfo> GetOpenMenus() => OpenMenus.Values;
        private static readonly Dictionary<int, PlayerInfo> OpenMenus = new();
        private static bool TickRegistered = false;

        // Define class methods
        public static void OpenMenu(PlayerInfo info)
        {
            OpenMenus[info.player.Slot] = info;

            // Register tick handler for this player
            if (!TickRegistered && Plugin.Instance != null && OpenMenus.Count == 1)
            {
                Plugin.Instance.RegisterListener<Listeners.OnTick>(OnTick);
                TickRegistered = true;
            }
        }

        public static void CloseMenu(PlayerInfo info)
        {
            OpenMenus.Remove(info.player.Slot);

            // Unregister tick handler if no more open menus
            if (TickRegistered && Plugin.Instance != null && OpenMenus.Count == 0)
            {
                Plugin.Instance.RemoveListener<Listeners.OnTick>(OnTick);
                TickRegistered = false;
            }
        }

        private static void OnTick()
        {
            foreach (var info in OpenMenus.Values)
                info.OnTick();
        }

        public static bool HasOpenMenu(int slot) => OpenMenus.ContainsKey(slot);
    }
}
