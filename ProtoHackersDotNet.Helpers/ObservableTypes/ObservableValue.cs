﻿using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace ProtoHackersDotNet.Helpers.ObservableTypes;

/// <summary>Wraps a value in an observable that reports changes to its status.</summary>
/// <remarks>Disposing </remarks>
/// <typeparam name="T">The type of the value.</typeparam>
/// <param name="initialValue">The value to initialize this object with.</param>
public sealed class ObservableValue<T>(T initialValue) : IDisposable
{
    readonly BehaviorSubject<T> valueObserver = new(initialValue);

    /// <summary>Gets or sets the latest value. Setting this value will alert all observers.</summary>
    public T CurrentValue
    {
        get => valueObserver.Value;
        set => valueObserver.OnNext(value);
    }

    /// <summary>Provides updates when the observed value changes.</summary>
    public IObservable<T> Value => valueObserver.AsObservable();

    /// <summary>Notifies all observers of completion of the sequence.</summary>
    /// <remarks>Note, after doing this the value can no longer be changed!</remarks>
    public void Complete() => valueObserver.OnCompleted();

    /// <summary>Disposes of this value, unsubscribing all observers. 
    /// After calling this method this value can no longer be read!</summary>
    public void Dispose() => valueObserver.Dispose();
}