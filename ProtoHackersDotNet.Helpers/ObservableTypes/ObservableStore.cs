using System.Collections.Concurrent;

namespace ProtoHackersDotNet.Helpers.ObservableTypes;

/// <summary>A simple thread-safe dictionary whose count can be observed.</summary>
/// <typeparam name="TKey">The type of the key that identifies the values. Cannot be null.</typeparam>
/// <typeparam name="TValue">The type of the value stored in this object. Cannot be null.</typeparam>
/// <param name="keySelector">A method for deriving or selecting <typeparamref name="TKey"/> from 
/// <typeparamref name="TValue"/>.</param>
public sealed class ObservableStore<TKey, TValue>(Func<TValue, TKey> keySelector)
    : IEnumerable<TValue> where TKey : notnull
{
    readonly ConcurrentDictionary<TKey, TValue> store = [];
    readonly ObservableValue<int> countObservable = new(0);

    public IEnumerator<TValue> GetEnumerator() => store.Values.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => store.Values.GetEnumerator();

    public int Count => store.Count;

    public IObservable<int> CurrentCount => countObservable.Value;

    public void Add(TValue value)
    {
        _ = store.TryAdd(keySelector(value), value) || ThrowArgumentException<bool>("Value already exists.");
        countObservable.CurrentValue = Count;
    }

    public void Remove(TValue value)
    {
        _ = store.TryRemove(keySelector(value), out _) || ThrowArgumentException<bool>("Value not found.");
        countObservable.CurrentValue = Count;
    }

    public void Clear()
    {
        store.Clear();
        countObservable.CurrentValue = 0;
    }
}