namespace ProtoHackersDotNet.GUI.MainView.ProtoHackerApi;

public static class ObservableExtensions
{
    public static IObservable<T> TakeWhileInclusive<T>(this IObservable<T> source, Func<T, bool> predicate) 
        => Observable.Create<T>(observer => source.Subscribe(item => {
                observer.OnNext(item);
                if (!predicate(item)) observer.OnCompleted();
            },
            observer.OnError,
            observer.OnCompleted));
}