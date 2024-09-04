using System.Reactive.Linq;

namespace ProtoHackersDotNet.Helpers.ObservableTypes;

public static class GuardedValueHelper
{
    public static IObservableValue<T> Guard<T>(this IObservableValue<T> source, Func<T, Exception?> guard)
        => new GuardedValue<T>(source, guard);

    public static IObservableValue<T> GuardNotNull<T>(this IObservableValue<T?> source)
        => new GuardedValue<T>(source!, value => value is null ? new ArgumentNullException(nameof(value)) : null);

    public static IObservableValue<T> GuardInSet<T>(this IObservableValue<T> source, IEnumerable<T> values)
        => new GuardedValue<T>(source, value => !values.Contains(value) ? new ArgumentOutOfRangeException(nameof(value)) : null);
}
