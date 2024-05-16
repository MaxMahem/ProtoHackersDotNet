using Avalonia.Data.Converters;

namespace ProtoHackersDotNet.GUI.Converters;

public static class Converters
{
    public static FuncValueConverter<object?, string?> ObjectToString { get; } =
        new(obj => obj?.ToString());
}

