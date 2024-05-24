namespace ProtoHackersDotNet.Servers.Interfaces;

public record class Problem(int Id, string Name)
{
    public string Description => descriptionCache.Value;
    readonly Lazy<string> descriptionCache = new(() => File.ReadAllText(Path.Combine(Name, Name + ".md")));
}
