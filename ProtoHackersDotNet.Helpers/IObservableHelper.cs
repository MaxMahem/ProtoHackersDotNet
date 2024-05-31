using System.Reactive.Linq;

namespace ProtoHackersDotNet.Helpers;

public static class IObservableHelper
{
    /// <summary>Filters for when the observable is true.</summary>
    /// <param name="observable">The observable to be filtered.</param>
    /// <returns>An observable that is only true.</returns>
    public static IObservable<bool> Where(this IObservable<bool> observable) => observable.Where(x => x);
}