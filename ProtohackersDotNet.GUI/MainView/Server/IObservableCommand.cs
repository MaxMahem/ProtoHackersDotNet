namespace ProtoHackersDotNet.GUI.MainView.Server;

public interface IObservableCommand
{
    IObservable<bool> CanExecute { get; }

    IObservable<bool> Executing { get; }

    void Execute();
}