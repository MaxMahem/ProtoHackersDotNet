namespace ProtoHackersDotNet.GUI.MainView;

public record class MainViewModelHolder(MainViewModelOptions MainViewModelOptions);

/// <summary>Settings object for <seealso cref="MainViewModel"/>. Includes serialization functionality.</summary>
/// <param name="model">The object to collect settings from.</param>
public class MainViewModelOptions
{
    public const string SETTINGS_PATH = nameof(MainViewModelOptions) + ".json";

    public TextEndPoint? LocalEndPoint { get; init; }
    public TextEndPoint? RemoteEndPoint { get; init; }
    public string? Server { get; init; }

    public void SaveSettings()
    {
        using var writer = File.Create(SETTINGS_PATH);
        JsonSerializer.Serialize(writer, new MainViewModelHolder(this), MainVMJsonMetaData.Default.MainViewModelHolder);
        writer.Flush();
    }

    public record TextEndPoint(string? IP, ushort? Port);
}