namespace ProtoHackersDotNet.GUI.Serialization;

public sealed class StateSerializer : IDisposable
{
    readonly IDisposable disposable;
    readonly AppState appState;
    readonly StateSerializationOptions options;

    public StateSerializer(StateSerializationOptions options, AppState appState)
    {
        this.options = options;
        this.appState = appState;
        this.disposable = this.appState.Changes.Throttle(options.SaveThrottle).Subscribe(SaveDictionary);
    }

    void SaveDictionary(IReadOnlyDictionary<string, IState> stateDictionary)
    {
        using var saveFile = File.Create(this.options.SavePath);
        JsonSerializer.Serialize(saveFile, stateDictionary, AppStateMetaData.Default.ReadOnlyDictionaryStringIState);
    }

    public void Save() => SaveDictionary(this.appState.Value);

    public void Dispose() => this.disposable.Dispose();
}
