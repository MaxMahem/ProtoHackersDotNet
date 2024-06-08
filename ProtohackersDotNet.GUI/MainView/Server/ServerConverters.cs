using Avalonia.Data.Converters;
using Material.Icons;

namespace ProtoHackersDotNet.GUI.MainView.Server;

public static class ServerConverters
{
    /// <summary>Converts a grade into a material icon</summary>
    public static FuncValueConverter<Grade, MaterialIconKind?> GradeToIcon { get; }
        = new(grade => grade switch { 
            Grade.Success => MaterialIconKind.Success,
            Grade.Failure => MaterialIconKind.Error,
            _ => null,
        });
}