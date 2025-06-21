// Declare namespace
namespace MenuManagerAPI.Contracts
{
    // Define interface
    public interface IPluginDependency<TPlugin, TConfig>
    {
        // Define interface methods
        public virtual void Clear() { }
        public virtual void OnLoad(TPlugin plugin) { }
        public virtual void OnMapStart(string mapName) { }
        public virtual void OnConfigParsed(TConfig config) { }
    }
}