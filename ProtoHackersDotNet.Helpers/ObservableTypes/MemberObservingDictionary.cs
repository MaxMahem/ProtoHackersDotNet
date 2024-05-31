using System.Reactive;
using System.Reactive.Linq;
using DynamicData;
namespace ProtoHackersDotNet.Helpers.ObservableTypes;

/// <summary>Dictionary that monitors one of its member property for changes in order to propagate updates.</summary>
/// <typeparam name="TKey">The type of key that uniquely identifies values in this dictionary.</typeparam>
/// <typeparam name="TValue">The type of value this dictionary holds.</typeparam>
/// <typeparam name="TObserved">The type of property that is monitored for changes.</typeparam>
public sealed class MemberObservingDictionary<TKey, TValue, TObserved> : IDisposable
    where TKey : notnull
    where TValue : notnull
{
    readonly SourceCache<TValue, TKey> entryCache;

    /// <summary>Provides a stream of changes in element state that occur within this dictionary.</summary>
    public IObservable<IChangeSet<TValue, TKey>> Entries { get; }

    /// <summary>Reports when any member of the dictionary has changed in value.</summary>
    public IObservable<Unit> Updated { get; }

    public IEnumerable<TKey> Keys => entryCache.Keys;
    public IEnumerable<TValue> Values => entryCache.Items;

    public MemberObservingDictionary(Func<TValue, TKey> keySelector, Func<TValue, IObservable<TObserved>> observableSelector)
    {
        this.entryCache = new(keySelector);
        Updated = this.entryCache.Connect().AutoRefreshOnObservable(observableSelector)
                                           .Select(_ => Unit.Default).StartWith(Unit.Default);
        Entries = this.entryCache.Connect();
    }

    /// <summary>Retrieves a member by key.</summary>
    /// <param name="key">The key to be evaluated.</param>
    /// <returns><c>true</c> if the entry is present and selected, false otherwise.</returns>
    public TValue this[TKey key] => this.entryCache.Lookup(key) is { HasValue: true } lookup ? lookup.Value 
        : ThrowInvalidOperationException<TValue>($"Key {key} not found.");

    /// <summary>Adds a list of entries to the filter.</summary>
    /// <param name="entries">The list of entries to add.</param>
    public void AddEntries(IEnumerable<TValue> entries)
    {
        foreach(var entry in entries)
            this.entryCache.AddOrUpdate(entry);
    }

    /// <summary>Adds a single entry to the dictionary.</summary>
    /// <param name="entry">The entry to add.</param>
    public void AddEntry(TValue entry) => this.entryCache.AddOrUpdate(entry);

    /// <summary>Removes a list of entries.</summary>
    /// <param name="keys">The list of entries to remove.</param>
    public void Remove(IEnumerable<TKey> keys) => this.entryCache.RemoveKeys(keys);
    
    /// <summary>Removes a single entry from the dictionary.</summary>
    /// <param name="key">The entry to remove.</param>
    public void Remove(TKey key) => this.entryCache.RemoveKey(key);

    public void Dispose() => this.entryCache.Dispose();
}