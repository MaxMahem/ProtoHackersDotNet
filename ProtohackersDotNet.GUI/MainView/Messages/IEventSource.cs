using System.Collections.Immutable;

namespace ProtoHackersDotNet.GUI.MainView.Messages;
public interface IEventSource
{
    Task<Unit> Completed { get; }
    ImmutableArray<string> SourceNames { get; init; }
}