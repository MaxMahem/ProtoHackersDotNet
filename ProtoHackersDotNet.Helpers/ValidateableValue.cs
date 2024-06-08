using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace ProtoHackersDotNet.Helpers.ObservableTypes;

/// <summary>Class that encapsulates a property of type <typeparamref name="T"/> and exposes a validation observable.</summary>
/// <typeparam name="T">The type of the property encapsulated.</typeparam>
/// <param name="validator">A method to use to check of the value is valid.</param>
/// <param name="initialValue">The initial value to start with.</param>
public class ValidateableValue<T>(Func<T?, bool> validator, T? initialValue = default)
{
    T? value = initialValue;
    readonly BehaviorSubject<bool> validityObserver = new(validator(initialValue));
    readonly BehaviorSubject<T?> valueObserver = new(initialValue);

    public T? CurrentValue
    {
        get => value;
        set
        {
            if (EqualityComparer<T>.Default.Equals(this.value, value)) return;
            this.value = value;
            this.valueObserver.OnNext(value);

            bool valid = validator(value);
            if (valid != validityObserver.Value) validityObserver.OnNext(valid);
        }
    }

    public IObservable<T?> Value => this.valueObserver.AsObservable();

    /// <summary>Reports if <see cref="CurrentValue"/> is valid or not.</summary>
    public IObservable<bool> Valid => validityObserver.AsObservable();

    public IObservable<T?> ValidValues => validityObserver.Where().Select(_ => CurrentValue);

    public static ValidateableValue<T> NotNull(T? initialValue = default)
        => new(item => item is not null, initialValue);
}