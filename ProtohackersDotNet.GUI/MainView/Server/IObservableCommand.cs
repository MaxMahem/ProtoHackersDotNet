namespace ProtoHackersDotNet.GUI.MainView.Server;

public interface IObservableCommand
{
    IObservable<bool> CanExecute { get; }

    void Execute();
}