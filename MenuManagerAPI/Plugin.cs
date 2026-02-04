// Included Libraries
using PlayerSettings;
using MenuManagerAPI.Core;
using CounterStrikeSharp.API;
using MenuManagerAPI.CrossCutting;
using CounterStrikeSharp.API.Core;
using Microsoft.Extensions.Logging;
using CounterStrikeSharp.API.Core.Capabilities;
using Microsoft.Extensions.DependencyInjection;
using static CounterStrikeSharp.API.Core.Listeners;

// Declare namespace
namespace MenuManagerAPI;

// Define Denependency Injection class
public class PluginDependencyInjection : IPluginServiceCollection<Plugin>
{
    public void ConfigureServices(IServiceCollection serviceCollection)
    {
        var di = new DependencyManager<Plugin, Config>();
        di.LoadDependencies(typeof(Plugin).Assembly);
        di.AddIt(serviceCollection);
    }
}

// Define main plugin class
public class Plugin : BasePlugin, IPluginConfig<Config>
{
    // Define module properties
    public override string ModuleVersion => "1.0.1";
    public override string ModuleName => "MenuManagerAPI";
    public override string ModuleAuthor => "Striker-Nick";
    private readonly DependencyManager<Plugin, Config> _dependencyManager;

    // Define constructor
    public Plugin(DependencyManager<Plugin, Config> dependencyManager)
    {
        _dependencyManager = dependencyManager;
    }

    // Define class fields and properties
    public static Plugin? Instance;
    private ISettingsApi? _settingsAPI;
    public required Config Config { get; set; }
    public static Dictionary<int, PlayerInfo> Players = new();
    private readonly PluginCapability<ISettingsApi?> settingsCapability = new("settings:nfcore");

    // Define class methods
    public void OnConfigParsed(Config _config)
    {
        if (_config.Version < 1)
            Logger.LogWarning("Your config file is too old, please backup and remove it.");

        Config = _config;
        _dependencyManager.OnConfigParsed(_config);
    }

    public override void Load(bool hotReload)
    {
        Instance = this;
        _dependencyManager.OnPluginLoad(this);
        RegisterListener<OnMapStart>(_dependencyManager.OnMapStart);

        MenuTypeManager.Initialize(_settingsAPI, Config.DefaultMenu, Logger);

        RegisterEventHandler<EventPlayerActivate>((@event, info) =>
        {
            if (@event.Userid != null)
                Players[@event.Userid.Slot] = new PlayerInfo
                {
                    player = @event.Userid,
                    Buttons = 0
                };
            return HookResult.Continue;
        });

        RegisterEventHandler<EventPlayerDisconnect>((@event, info) =>
        {
            if (@event.Userid != null)
            {
                MenuTypeManager.ClearPlayerCache(@event.Userid.SteamID);
                Players.Remove(@event.Userid.Slot);
            }
            return HookResult.Continue;
        });

        RegisterEventHandler<EventRoundEnd>((@event, info) =>
        {
            if (Config.ButtonMenu.ClearStateOnRoundEnd)
            {
                MenuTypeManager.ClearAllCache();
            }
            return HookResult.Continue;
        });

        if (hotReload)
        {
            foreach (var pl in Utilities.GetPlayers())
            {
                Players[pl.Slot] = new PlayerInfo
                {
                    player = pl,
                    Buttons = pl.Buttons
                };
            }
        }
    }

    public override void OnAllPluginsLoaded(bool hotReload)
    {
        _settingsAPI = settingsCapability.Get();
        if (_settingsAPI != null)
        {
            PlayerExtensions.LoadSettings(_settingsAPI);
            MenuTypeManager.Initialize(_settingsAPI, Config.DefaultMenu);
        }
        else
        {
            Console.WriteLine("PlayerSettings core not found - menu preferences will not be persisted.");
            MenuTypeManager.Initialize(null, Config.DefaultMenu);
        }
    }
}
