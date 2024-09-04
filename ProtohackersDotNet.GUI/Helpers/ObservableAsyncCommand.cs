using System.Reactive.Subjects;

namespace ProtoHackersDotNet.GUI.Helpers;

public class ObservableAsyncCommand<T>(Func<Task<T>> execute, IObservable<bool> canExecute, bool initialState) : IObservableCommand<T>
{
    readonly Subject<T> resultSubject = new();
    readonly BehaviorSubject<bool> canExecuteSubject = new(initialState);

    public IObservable<bool> CanExecute => Observable.CombineLatest(canExecute, this.canExecuteSubject.AsObservable(),
        (external, intern) => external && intern).DistinctUntilChanged();

    public async void Execute()
    {
        if (!this.canExecuteSubject.Value) { ThrowInvalidOperationException(); }
        try {
            this.canExecuteSubject.OnNext(false);

            T result = await execute.Invoke();
            this.resultSubject.OnNext(result);

            this.canExecuteSubject.OnNext(true);
        }
        catch (Exception exception) {
            this.resultSubject.OnError(exception);
            this.canExecuteSubject.OnNext(false);
        }
    }

    public IObservable<T> Results => this.resultSubject.AsObservable();
}