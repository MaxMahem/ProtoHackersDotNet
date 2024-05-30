using Avalonia.Controls.ApplicationLifetimes;
using ProtoHackersDotNet.GUI.MainView.Server;

namespace ProtoHackersDotNet.GUI.Serialization;

public record class AppStates(ServerManagerState ServerManagerState);

public class StateSaver(ServerManager ServerManager)
{
    public const string SETTINGS_PATH = nameof(AppStates) + ".json";

    public void Save()
    {
        AppStates states = new(ServerManager.GetState());
        using var writer = File.Create(SETTINGS_PATH);
        JsonSerializer.Serialize(writer, states, AppStateMetaData.Default.AppStates);
        writer.Flush();
    }
}