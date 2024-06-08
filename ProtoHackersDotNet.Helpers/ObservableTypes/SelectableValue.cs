using System.Collections.ObjectModel;

namespace ProtoHackersDotNet.Helpers.ObservableTypes;

public class SelectableValue<T>(IEnumerable<T> values, T? initialValue = default) 
    : ValidateableValue<T>(value => value is null || values.Contains(value), initialValue)
{        
    public ReadOnlyObservableCollection<T> Values { get; } = new ObservableCollection<T>(values).AsReadOnlyObservableCollection();
}
