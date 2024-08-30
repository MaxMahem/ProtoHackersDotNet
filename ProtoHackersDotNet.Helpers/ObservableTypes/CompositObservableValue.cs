using System.Reactive.Linq;

namespace ProtoHackersDotNet.Helpers.ObservableTypes;

/// <summary>Represents a composite observable value that combines two observable values into a single result using a 
/// combiner function.</summary>
/// <typeparam name="T1">The type of the first observable value.</typeparam>
/// <typeparam name="T2">The type of the second observable value.</typeparam>
/// <typeparam name="TResult">The type of the result produced by combining the two values.</typeparam>
/// <param name="observableValue1">The first observable value.</param>
/// <param name="observableValue2">The second observable value.</param>
/// <param name="combiner">A function to invoke to combine elements of type <typeparamref name="T1"/> and 
/// <typeparamref name="T2"/> to get a <typeparamref name="TResult"/>.</param>
public sealed class CompositObservableValue<T1, T2, TResult>(IReadOnlyObservableValue<T1> value1, IReadOnlyObservableValue<T2> value2, Func<T1, T2, TResult> combiner)
    : IReadOnlyObservableValue<TResult>
{
    /// <summary>Gets the current combined value of the two observable values using the combiner function.</summary>
    public TResult Value => combiner(value1.Value, value2.Value);

    /// <summary>Gets an observable sequence that emits a new value whenever the combined result changes.</summary>
    public IObservable<TResult> Changes => Observable.CombineLatest(value1.Changes, value2.Changes, combiner)
                                                     .DistinctUntilChanged();

    public void Dispose()
    {
        value1.Dispose();
        value2.Dispose();
    }
}