using ProtoHackersDotNet.GUI.MainView.Client;
using ProtoHackersDotNet.GUI.MainView.Grader;
using ProtoHackersDotNet.GUI.MainView.Messages;
using ProtoHackersDotNet.GUI.MainView.Server;
namespace ProtoHackersDotNet.GUI.MainView;

public sealed class MainViewModel
{
    public MainViewModel(ServerManager serverManager, ClientManager clientManager, MessageManager messageManager,
        GradingService gradingService, StartServerCommand startServerCommand, TestServerCommand testServerCommand, ClearLogCommand clearLogCommand)
    {
        ServerManager = serverManager;
        ClientManager = clientManager;
        MessageManager = messageManager;
        GradingService = gradingService;

        StartServerCommand = startServerCommand;
        TestServerCommand  = testServerCommand;

        // When the server changes, clear the logs.
        serverManager.Server.Subscribe(onNext: _ => clearLogCommand.ClearClientsAndMessages()).DiscardUnsubscribe();
    }

    public ServerManager ServerManager { get; }
    public ClientManager ClientManager { get; }
    public MessageManager MessageManager { get; }
    public GradingService GradingService { get; }

    public StartServerCommand StartServerCommand { get; }
    public TestServerCommand TestServerCommand { get; }
}