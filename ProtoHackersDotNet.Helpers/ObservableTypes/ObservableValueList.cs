using System.Collections.ObjectModel;

namespace ProtoHackersDotNet.Helpers.ObservableTypes;

/// <summary>A collection of <typeparamref name="T"/> values, with an observable count.
/// Not thread safe.</summary>
/// <typeparam name="T"></typeparam>
public class ObservableValueList<T> : IEnumerable<T>
{
    readonly List<T> values = [];
    readonly ObservableValue<int> countObservable = new(0);

    public int Count => this.values.Count;
    public IObservable<int> CurrentCount => this.countObservable.Value;

    public void Add(T item)
    {
        this.values.Add(item);
        this.countObservable.CurrentValue++;
    }
    public void Clear()
    {
        this.values.Clear();
        this.countObservable.CurrentValue = 0;
    }

    public IEnumerator<T> GetEnumerator() => this.values.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => this.values.GetEnumerator();
}

public static class ObservableCollectionHelper
{
    public static ReadOnlyObservableCollection<T> AsReadOnlyObservableCollection<T>(this ObservableCollection<T> items) => new(items);
}