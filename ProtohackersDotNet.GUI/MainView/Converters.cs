using Avalonia.Data.Converters;
using Avalonia.Media;

namespace ProtoHackersDotNet.GUI.MainView;

public static class Converters
{
    /// <summary>Joins a bunch of values into one string.</summary>
    public static FuncMultiValueConverter<object?, string> JoinConverter { get; } 
        = new(objects => string.Join(" - ", objects.Where(obj => obj is not null)));

    public static FuncMultiValueConverter<object?, Brush> StatusBrushConverter { get; } 
        = new(objects => objects.ToArray() switch {
            [ConnectionStatus.Connected, Brush connectedBrush, _, _] => connectedBrush,
            [ConnectionStatus.Disconnected, _, Brush disconnectedBrush] => disconnectedBrush,
            [ConnectionStatus.Exception, _, _, Brush terminatedBrush] => terminatedBrush,
            _ => (Brush) Brushes.Purple
        });

    public static FuncValueConverter<int, bool> GreaterThanZeroConverter { get; } 
        = new(number => number > 0);
}

