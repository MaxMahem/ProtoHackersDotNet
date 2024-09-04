namespace ProtoHackersDotNet.Helpers.ObservableTypes;

public sealed class GuardedValue<T>(IObservableValue<T> observableValue, Func<T, Exception?> guard) : IObservableValue<T>
{
    public T Value {
        get => observableValue.Value;
        set {
            if (guard(value) is Exception exception) {
                observableValue.Error(exception);
                return;
            }
            else {
                observableValue.Value = value;
            }
        }
    }

    public IObservable<T> Changes => observableValue.Changes;

    public void Complete() => observableValue.Complete();
    public void Error(Exception exception) => observableValue.Error(exception);
    public void Dispose() => observableValue.Dispose();
}