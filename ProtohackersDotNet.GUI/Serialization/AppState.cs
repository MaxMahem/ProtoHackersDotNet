using System.Reactive.Disposables;
using System.Reactive.Subjects;

namespace ProtoHackersDotNet.GUI.Serialization;

public sealed class AppState : IReadOnlyObservableValue<ReadOnlyDictionary<string, IState>>, IDisposable
{
    readonly Dictionary<string, IState> stateDictionary;
    readonly ReadOnlyDictionary<string, IState> readOnlyStateDictionary;
    readonly BehaviorSubject<ReadOnlyDictionary<string, IState>> stateDictionaryObservable;
    readonly CompositeDisposable disposables;

    public AppState(IEnumerable<IStateProvider> stateProviders)
    {
        // multiple objects created here to avoid unnecessary allocations to feed the observable
        this.stateDictionary = [];                                          // holds the app state
        this.readOnlyStateDictionary = this.stateDictionary.AsReadOnly();   // readonly wrapper around the app state
        this.stateDictionaryObservable = new(this.readOnlyStateDictionary); // observable wrapper around the app state

        this.disposables = [ 
            Observable.CombineLatest(stateProviders.Select(saveable => saveable.StateChanges)).Subscribe(UpdateStateDictionary),
            this.stateDictionaryObservable,
        ];
    }

    public IObservable<ReadOnlyDictionary<string, IState>> Changes => this.stateDictionaryObservable.AsObservable();
    public ReadOnlyDictionary<string, IState> Value => this.stateDictionaryObservable.Value;

    void UpdateStateDictionary(IList<IState> states)
    {
        foreach (var state in states) { this.stateDictionary[state.ObjectName] = state; }
        // readOnlystateDictionary wraps stateDictionary and will reflect its changes
        this.stateDictionaryObservable.OnNext(this.readOnlyStateDictionary); 
    }

    public void Dispose() => this.disposables.Dispose();
}