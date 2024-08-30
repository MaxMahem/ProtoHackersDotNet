using System.Reactive.Linq;

namespace ProtoHackersDotNet.Helpers.ObservableTypes;

public class CompositValidatableValue<T1, T2, TResult>(ValidateableValue<T1> value1, ValidateableValue<T2> value2, Func<T1?, T2?, TResult?> combiner)
    : IValidateableValue
{    
    /// <summary>Gets the current combined value of the two observable values using the combiner function.</summary>
    public TResult? Value => combiner(value1.Value, value2.Value);

    /// <summary>Gets an observable sequence that emits a new value whenever the combined result changes.</summary>
    public IObservable<TResult?> Changes => Observable.CombineLatest(value1.Changes, value2.Changes, combiner)
                                                      .DistinctUntilChanged();

    /// <summary>Gets the current combined value of the two observable values using the combiner function.</summary>
    public bool Valid => value1.Valid && value2.Valid;

    /// <summary>Gets the current combined value of the two observable values using the combiner function.</summary>
    public IObservable<bool> Validity => Observable.CombineLatest(value1.Validity, value2.Validity, (v1, v2) => v1 && v2)
                                                   .DistinctUntilChanged();
}