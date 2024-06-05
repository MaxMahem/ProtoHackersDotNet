using ReactiveUI;

namespace ProtoHackersDotNet.GUI.MainView.Messages;

public sealed class ListFilterManager : IDisposable
{
    readonly IDisposable entriesUnsubscriber;

    [SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "Bind target")]
    ReadOnlyObservableCollection<StringFilterEntry> entries;

    public ReadOnlyObservableCollection<StringFilterEntry> Entries => entries;

    public ListFilterManager(SourceCache<StringFilterEntry, string> listFilter) =>
        this.entriesUnsubscriber = listFilter.Connect().ObserveOn(RxApp.MainThreadScheduler)
                                             .SortAndBind(out this.entries).Subscribe();

    public void Dispose() => this.entriesUnsubscriber.Dispose();

    public void SetAll(bool value)
    {
        foreach(var entry in this.entries) entry.Selected = value;
    }
}
