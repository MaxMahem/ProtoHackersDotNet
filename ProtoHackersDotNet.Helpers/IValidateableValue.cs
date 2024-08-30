
namespace ProtoHackersDotNet.Helpers.ObservableTypes;

/// <summary>Represents a value that maintains a valid vs invalid state, and reports changes in that state.</summary>
public interface IValidateableValue
{
    /// <summary>Gets the current validity state of this value.</summary>
    bool Valid { get; }

    /// <summary>Reports when the validity state of this value changes.</summary>
    IObservable<bool> Validity { get; }
}