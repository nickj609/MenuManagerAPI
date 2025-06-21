// Included libraries
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

// Create class to load dependencies
public class PluginDependencyInjection : IPluginServiceCollection<Plugin>
{
    public void ConfigureServices(IServiceCollection serviceCollection)
    {
        var di = new DependencyManager<Plugin, Config>();
        di.LoadDependencies(typeof(Plugin).Assembly);
        di.AddIt(serviceCollection);
    }
}

// Define plugin class
public class Plugin : BasePlugin, IPluginConfig<Config>
{
    // Define module properties
    public override string ModuleVersion => "1.0.1";
    public override string ModuleName => "MenuManagerAPI";
    public override string ModuleAuthor => "Striker-Nick";

    // Define class dependencies
    private readonly DependencyManager<Plugin, Config> _dependencyManager;

    // Define class constructor
    public Plugin(DependencyManager<Plugin, Config> dependencyManager)
    {
        _dependencyManager = dependencyManager;
    }

    // Define class properties
    public static Plugin? Instance;
    private ISettingsApi? _settingsAPI;
    public required Config Config { get; set; }
    public static Dictionary<int, PlayerInfo> Players = new();
    private readonly PluginCapability<ISettingsApi?> settingsCapability = new("settings:nfcore");

    // Define on config parsed behavior
    public void OnConfigParsed(Config _config)
    {
        // Check config version
        if (_config.Version < 1)
        {
            Logger.LogWarning($"Your config file is too old, please backup and remove it.");
        }
        
        // Set config
        Config = _config;

        // Load dependencies
        _dependencyManager.OnConfigParsed(_config);
    }

    // Define on load behavior
    public override void Load(bool hotReload)
    {
        // Set instance
        Instance = this;

        // Load dependencies
        _dependencyManager.OnPluginLoad(this);
        RegisterListener<OnMapStart>(_dependencyManager.OnMapStart);

        // Register event handlers
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
            if (@event.Userid != null) Players.Remove(@event.Userid.Slot);
            return HookResult.Continue;
        });

        // Define hot reload behavior
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

    // Define on all plugins loaded behavior
    public override void OnAllPluginsLoaded(bool hotReload)
    {
        _settingsAPI = settingsCapability.Get();
        if (_settingsAPI != null)
        {
            PlayerExtensions.LoadSettings(_settingsAPI);
        }
        else
        {
            Console.WriteLine("PlayerSettings core not found...");
        }
    }
}
