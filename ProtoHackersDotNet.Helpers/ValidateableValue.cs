using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace ProtoHackersDotNet.Helpers.ObservableTypes;

/// <summary>Class that encapsulates a property of type <typeparamref name="T"/> and exposes a validation observable.</summary>
/// <typeparam name="T">The type of the property encapsulated.</typeparam>
/// <param name="validator">A method to use to check of the value is valid.</param>
/// <param name="initialValue">The initial value to start with.</param>
public sealed class ValidateableValue<T>(Func<T?, bool> validator, T? initialValue = default) : IObservableValue<T?>, IValidateableValue
{
    readonly BehaviorSubject<T?> valueObserver = new(initialValue);

    public T? Value {
        get => this.valueObserver.Value;
        set => this.valueObserver.OnNext(value);
    }

    public IObservable<T?> Changes => this.valueObserver.AsObservable();

    /// <summary>Reports completion of the <see cref="ObservableValue{T}.Changes"/> stream.</summary>
    /// <remarks>After callnig this <see cref="ObservableValue{T}.Value"/> can no longer be set.</remarks>
    public void Complete() => this.valueObserver.OnCompleted();

    /// <summary>Reports an exception to the <see cref="ObservableValue{T}.Changes"/> stream.</summary>
    /// <remarks>This may cause a throw, and after doing this <see cref="ObservableValue{T}.Value"/> can no longer be set.</remarks>
    public void Error(Exception exception) => this.valueObserver.OnError(exception);

    /// <summary>Gets the current validity of this value.</summary>
    public bool Valid => validator(this.valueObserver.Value);

    /// <summary>Reports if <see cref="Value"/> is valid or not.</summary>
    public IObservable<bool> Validity => this.valueObserver.Select(value => validator(value)).DistinctUntilChanged();

    public static ValidateableValue<T> NotNull(T? initialValue = default)
        => new(item => item is not null, initialValue);

    public static ValidateableValue<T> InSet(IEnumerable<T> values, T? initialValue = default)
        => new(value => value is null || values.Contains(value), initialValue);

    public void Dispose() => this.valueObserver.Dispose();
}
