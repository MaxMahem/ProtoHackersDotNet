using System.Diagnostics.CodeAnalysis;
using System.Reactive;
using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using DynamicData.Binding;

namespace ProtoHackersDotNet.GUI.MainView.Messages;

public partial class FilterManager : IDisposable 
{
    readonly SourceCache<FilterEntry, string> entryCache = new(entry => entry.Entry);

    [SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "Binding target.")]
    ReadOnlyObservableCollection<FilterEntry> entries;
    public ReadOnlyObservableCollection<FilterEntry> Entries => this.entries;

    public IObservable<Unit> FilterListUpdates { get; }

    readonly HashSet<string> sources = [];

    public bool this[string key] => this.entryCache.Lookup(key) is { HasValue: true } lookup && lookup.Value.Selected;

    readonly IDisposable entriesUnsubscriber;

    public FilterManager()
    {
        FilterListUpdates = this.entryCache.Connect().AutoRefreshOnObservable(entry => entry.WhenAnyPropertyChanged())
                                           .Select(_ => Unit.Default).StartWith(Unit.Default);
        this.entriesUnsubscriber = this.entryCache.Connect().SortBy(entry => entry.Entry).Bind(out this.entries).Subscribe();
    }

    public void Add(string source)
    {
        if (this.sources.Add(source)) this.entryCache.AddOrUpdate(new FilterEntry(source));
    }

    public void Clear()
    {
        this.sources.Clear();
        this.entryCache.Clear();
    }

    public void Dispose()
    {
        this.entryCache.Dispose();
        this.entriesUnsubscriber.Dispose();
    }

    public partial class FilterEntry(string entry, bool selected = true) : ObservableObject
    {
        public string Entry { get; } = entry;

        [ObservableProperty]
        bool selected = selected;
    }
}