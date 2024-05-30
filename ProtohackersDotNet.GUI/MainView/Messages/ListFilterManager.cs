using DynamicData;
using DynamicData.Binding;

namespace ProtoHackersDotNet.GUI.MainView.Messages;

public sealed class ListFilterManager : IDisposable
{
    readonly IDisposable entriesUnsubscriber;

    [SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "Bind target")]
    ReadOnlyObservableCollection<ListFilterEntry> entries;

    public ReadOnlyObservableCollection<ListFilterEntry> Entries => entries;

    public ListFilterManager(ListFilter listFilter) =>
        this.entriesUnsubscriber = listFilter.Entries.SortAndBind(out this.entries).Subscribe();

    public void Dispose() => this.entriesUnsubscriber.Dispose();
}
