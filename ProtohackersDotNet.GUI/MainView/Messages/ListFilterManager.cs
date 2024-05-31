namespace ProtoHackersDotNet.GUI.MainView.Messages;

public sealed class ListFilterManager : IDisposable
{
    readonly IDisposable entriesUnsubscriber;

    [SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "Bind target")]
    ReadOnlyObservableCollection<StringFilterEntry> entries;

    public ReadOnlyObservableCollection<StringFilterEntry> Entries => entries;

    public ListFilterManager(MemberObservingDictionary<string, StringFilterEntry, bool> listFilter) =>
        this.entriesUnsubscriber = listFilter.Entries.SortAndBind(out this.entries).Subscribe();

    public void Dispose() => this.entriesUnsubscriber.Dispose();

    public void SetAll(bool value)
    {
        foreach(var entry in this.entries) entry.Selected = value;
    }
}
