using Avalonia.Data.Converters;
using Avalonia.Media;

namespace ProtoHackersDotNet.GUI.MainView;

public static class Converters
{
    /// <summary>Converts an endpoint and a string into a name.</summary>
    public static FuncMultiValueConverter<object?, string> ClientNameConverter { get; } =
        new(objects => objects.ToArray() switch {
            [EndPoint endPoint, string name] => $"{endPoint} - \"{name}\"",
            [EndPoint endPoint, _] => endPoint.ToString() ?? ThrowHelper.ThrowArgumentNullException<string>(),
            _ => string.Empty
        });

    public static FuncMultiValueConverter<object?, Brush> StatusBrushConverter { get; } =
        new(objects => objects.ToArray() switch {
            [ConnectionStatus.Connected, Brush connectedBrush, _, _] => connectedBrush,
            [ConnectionStatus.Disconnected, _, Brush disconnectedBrush] => disconnectedBrush,
            [ConnectionStatus.Terminated, _, _, Brush terminatedBrush] => terminatedBrush,
            _ => (Brush) Brushes.Purple
        });
}

