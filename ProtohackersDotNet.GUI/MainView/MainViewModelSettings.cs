using System.Text.Json;
using System.Text.Json.Serialization;

namespace ProtoHackersDotNet.GUI.MainView;

/// <summary>Settings object for <seealso cref="MainViewModel"/>. Includes serialization functionality.</summary>
/// <param name="model">The object to collect settings from.</param>
[method: JsonConstructor]
public class MainViewModelSettings(string ip, ushort? port, string? server)
{
    const string SettingsPath = "MainVMSettings.json";

    public MainViewModelSettings(MainViewModel model)
        : this(model.ServerIP.ToString(), model.ServerPort, model.ServerFactory.Name) { }

    public string? IP { get; init; } = ip;
    public ushort? Port { get; init; } = port;
    public string? Server { get; init; } = server;

    public void SaveSettings()
    {
        using var writer = File.Create(SettingsPath);
        JsonSerializer.Serialize(writer, this, MainVMGenerationContext.Default.MainViewModelSettings);
    }

    public static MainViewModelSettings? LoadSettings()
    {
        if (File.Exists(SettingsPath)) {
            var settingsFile = File.OpenRead(SettingsPath);
            return JsonSerializer.Deserialize(settingsFile, MainVMGenerationContext.Default.MainViewModelSettings);
        }
        return null;
    }
}
