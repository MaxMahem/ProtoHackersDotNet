namespace ProtoHackersDotNet.GUI.Serialization;

/// <summary>Type that can provide a serializable state.</summary>
public interface IStateSaveable
{
    /// <summary>Get the current serializable state of this object.</summary>
    /// <returns>The current serializable state of this object.</returns>
    IState GetState();
}