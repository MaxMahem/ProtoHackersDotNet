using DynamicData;
using DynamicData.Binding;

namespace ProtoHackersDotNet.GUI.MainView.Messages;

public sealed class ListFilter : IDisposable
{
    readonly SourceCache<ListFilterEntry, string> entryCache = new(entry => entry.Entry);
    public IObservable<IChangeSet<ListFilterEntry, string>> Entries { get; }

    public IObservable<Unit> Updates { get; }

    readonly HashSet<string> sources = [];

    /// <summary>Evaluates the filter for <paramref name="key"/>. 
    /// <list type="bullet">
    /// <item><c>true</c> if the entry is present and selected.</item>
    /// <item><c>false</c> if no entry is found, or if the entry is no selected.</item>
    /// </list></summary>
    /// <param name="key">The key to be evaluated.</param>
    /// <returns><c>true</c> if the entry is present and selected, false otherwise.</returns>
    public bool this[string key] => this.entryCache.Lookup(key) is { HasValue: true } lookup ? lookup.Value.Selected : true;

    public ListFilter()
    {
        Updates = this.entryCache.Connect().AutoRefreshOnObservable(entry => entry.SelectedUpdates)
                                           .Select(_ => Unit.Default).StartWith(Unit.Default);
        Entries = this.entryCache.Connect();
    }

    public void AddEntries(string[] sources)
    {
        foreach(var source in sources.Where(this.sources.Add))
            this.entryCache.AddOrUpdate(new ListFilterEntry(source));
    }

    public void AddEntry(string source)
    {
        if (this.sources.Add(source)) return;
            this.entryCache.AddOrUpdate(new ListFilterEntry(source));
    }

    public void Clear()
    {
        this.sources.Clear();
        this.entryCache.Clear();
    }

    public void Dispose() => this.entryCache.Dispose();
}