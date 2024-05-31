using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace ProtoHackersDotNet.Helpers;

/// <summary>Class that encapsulates a property of type <typeparamref name="T"/> and exposes a validation observable.</summary>
/// <typeparam name="T">The type of the property encapsulated.</typeparam>
/// <param name="validator">A method to use to check of the value is valid.</param>
/// <param name="initialValue">The initial value to start with.</param>
public class ValidateableValue<T>(Func<T?, bool> validator, T? initialValue = default)
{
    T? value = initialValue;
    readonly BehaviorSubject<bool> validityObserver = new(validator(initialValue));

    public T? Value
    {
        get => value;
        set
        {
            if (EqualityComparer<T>.Default.Equals(this.value, value)) return;
            this.value = value;

            bool valid = validator(value);
            if (valid != validityObserver.Value) validityObserver.OnNext(valid);
        }
    }

    /// <summary>Reports if <see cref="Value"/> is valid or not.</summary>
    public IObservable<bool> Valid => validityObserver.AsObservable();

    public IObservable<T?> ValidValue => validityObserver.Where().Select(_ => Value);

    public static ValidateableValue<T> NotNull(T? initialValue = default)
        => new(item => item is not null, initialValue);
}