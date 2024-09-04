using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace ProtoHackersDotNet.Helpers.ObservableTypes;

/// <summary>Wraps a value in an observable that reports changes to its status.</summary>
/// <typeparam name="T">The type of the value.</typeparam>
/// <param name="initialValue">The value to initialize this object with.</param>
public sealed class ObservableValue<T>(T initialValue) : IDisposable, IObservableValue<T>
{
    readonly BehaviorSubject<T> valueObserver = new(initialValue);

    public T Value {
        get => this.valueObserver.Value;
        set => this.valueObserver.OnNext(value);
    }

    public IObservable<T> Changes => this.valueObserver.AsObservable();

    /// <summary>Reports completion of the <see cref="ObservableValue{T}.Changes"/> stream.</summary>
    /// <remarks>After calling this <see cref="ObservableValue{T}.Value"/> can no longer be set.</remarks>
    public void Complete() => this.valueObserver.OnCompleted();

    /// <summary>Reports an exception to the <see cref="ObservableValue{T}.Changes"/> stream.</summary>
    /// <remarks>This may cause a throw, and after doing this <see cref="ObservableValue{T}.Value"/> can no longer be 
    /// set.</remarks>
    public void Error(Exception exception) => this.valueObserver.OnError(exception);

    /// <summary>Disposes of this value, unsubscribing all observers.</summary>
    /// <remarks></remarks>After calling this method this <see cref="ObservableValue{T}.Value"/> can no longer be 
    /// read!</remarks>
    public void Dispose() => this.valueObserver.Dispose();
}
