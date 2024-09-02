namespace ProtoHackersDotNet.GUI.Serialization;

/// <summary>Type that can provide a serializable state.</summary>
public interface IStateProvider
{
    /// <summary>Reports the new state whenever the state of this object changes.</summary>
    IObservable<IState> StateChanges { get; }
}