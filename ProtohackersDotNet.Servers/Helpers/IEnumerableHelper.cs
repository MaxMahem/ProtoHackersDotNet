namespace ProtoHackersDotNet.Servers.Helpers;

public static class IEnumerableHelper
{
    public static IEnumerable<T> Except<T>(this IEnumerable<T> source, T value)
        => source.Where(x => !x?.Equals(value) ?? true);
}