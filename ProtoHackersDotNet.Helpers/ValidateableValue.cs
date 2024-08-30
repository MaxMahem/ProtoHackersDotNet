using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace ProtoHackersDotNet.Helpers.ObservableTypes;

/// <summary>Class that encapsulates a property of type <typeparamref name="T"/> and exposes a validation observable.</summary>
/// <typeparam name="T">The type of the property encapsulated.</typeparam>
/// <param name="validator">A method to use to check of the value is valid.</param>
/// <param name="initialValue">The initial value to start with.</param>
public sealed class ValidateableValue<T>(Func<T?, bool> validator, T? initialValue = default) : IObservableValue<T?>, IValidateableValue
{
    T? value = initialValue;
    readonly BehaviorSubject<bool> validityObserver = new(validator(initialValue));
    readonly BehaviorSubject<T?> valueObserver = new(initialValue);

    public T? Value {
        get => value;
        set {
            if (EqualityComparer<T>.Default.Equals(this.value, value))
                return;
            this.value = value;
            this.valueObserver.OnNext(value);

            bool valid = validator(value);
            if (valid != validityObserver.Value)
                validityObserver.OnNext(valid);
        }
    }

    public IObservable<T?> Changes => this.valueObserver.AsObservable();

    /// <summary>Gets the current validity of this value.</summary>
    public bool Valid => this.validityObserver.Value;

    /// <summary>Reports if <see cref="Value"/> is valid or not.</summary>
    public IObservable<bool> Validity => this.validityObserver.AsObservable();

    public static ValidateableValue<T> NotNull(T? initialValue = default)
        => new(item => item is not null, initialValue);

    public static ValidateableValue<T> InSet(IEnumerable<T> values, T? initialValue = default)
        => new(value => value is null || values.Contains(value), initialValue);

    public void Dispose()
    {
        this.validityObserver.Dispose();
        this.valueObserver.Dispose();
    }
}