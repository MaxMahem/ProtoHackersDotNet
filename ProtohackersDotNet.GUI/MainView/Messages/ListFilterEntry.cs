using System.Reactive.Subjects;

namespace ProtoHackersDotNet.GUI.MainView.Messages;

public sealed record class ListFilterEntry(string Entry) : IComparable<ListFilterEntry>
{
    readonly BehaviorSubject<bool> selectedObserver = new(true);
    public bool Selected {
        get => this.selectedObserver.Value;
        set => selectedObserver.OnNext(value);
    }
    public IObservable<bool> SelectedUpdates => this.selectedObserver.AsObservable();

    public int CompareTo(ListFilterEntry? other) => Entry.CompareTo(other?.Entry) * -1;
}