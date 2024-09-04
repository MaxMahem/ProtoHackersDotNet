namespace ProtoHackersDotNet.GUI.Helpers;

public interface IObservableCommand<T>
{
    /// <summary>Reports if this command can be executed or not.</summary>
    IObservable<bool> CanExecute { get; }

    /// <summary>Executes this command.</summary>
    void Execute();

    /// <summary>Reports whenever this command has new results.</summary>
    IObservable<T> Results { get; }
}