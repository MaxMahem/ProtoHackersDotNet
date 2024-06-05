using ProtoHackersDotNet.GUI.MainView.Server;

namespace ProtoHackersDotNet.GUI.Serialization;

public class StateSaver(IEnumerable<IStateSaveable> saveables)
{
    public const string SETTINGS_PATH = "appstate.json";

    public void Save()
    {
        var stateDictionary = saveables.Select(saveable => saveable.GetState()).ToDictionary(state => state.ObjectName);
        // AppStates states = new(ServerManager.GetState());
        using var writer = File.Create(SETTINGS_PATH);
        JsonSerializer.Serialize(writer, stateDictionary, AppStateMetaData.Default.DictionaryStringIState);
        writer.Flush();
    }

    /// <summary>Saves the current app state. Discarding any input.</summary>
    /// <remarks>This overload exists to make subscribing to an observable easier.</remarks>
    /// <typeparam name="T">The type of the observable event.</typeparam>
    /// <param name="_">The value of the observable event. It is discarded.</param>
    public void Save<T>(T _) => Save();
}