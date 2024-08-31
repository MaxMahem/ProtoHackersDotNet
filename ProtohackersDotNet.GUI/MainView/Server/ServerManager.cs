using ProtoHackersDotNet.GUI.Serialization;
using ProtoHackersDotNet.Servers;
using ReactiveUI;
using System.Reactive.Disposables;

namespace ProtoHackersDotNet.GUI.MainView.Server;

public sealed class ServerManager : IStateSaveable, IDisposable
{
    /// <summary>Gets the currently selected problem.</summary>
    public IObservableValue<IProblem> Problem { get; }

    /// <summary>Gets the known set of problems.</summary>
    public ReadOnlyObservableCollection<IProblem> Problems { get; }

    /// <summary>Gets the currently selected ServerVM.</summary>
    public IObservableValue<ServerVM?> ServerVM { get; }

    [SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "Bind target")]
    ReadOnlyObservableCollection<ServerVM> servers;

    /// <summary>Gets the current set of available servers.</summary>
    /// <remarks>Filtered based on <see cref="SelectedProblem"/> value.</remarks>
    public ReadOnlyObservableCollection<ServerVM> Servers => this.servers;

    readonly SourceCache<ServerVM, ServerVM> serverCache = new(server => server);

    readonly CompositeDisposable subscriptions;

    public ServerManager(IEnumerable<IProblem> problems, IEnumerable<IServer> servers, ServerManagerState options)
    {
        var initialProblem = problems.FirstOrDefault(problem => problem.Name == options.Problem, problems.First());
        Problem = new ObservableValue<IProblem?>(initialProblem).GuardNotNull().GuardInSet(problems);
        Problems = new ObservableCollection<IProblem>(problems).AsReadOnlyObservableCollection();

        // build the filterable Server collection
        this.serverCache.AddOrUpdate(servers.Select(GUI.MainView.Server.ServerVM.Create));
        var filteredServersChanges = this.serverCache.Connect().Filter(Problem.Changes.Select(MakeFilter));
        var serverUpdatesSubscription = filteredServersChanges.ObserveOn(RxApp.MainThreadScheduler).Bind(out this.servers).Subscribe();

        var initialServer = Servers.FirstOrDefault(server => server?.Server.Name.Value == options.Server, Servers.FirstOrDefault());
        ServerVM = new ObservableValue<ServerVM?>(initialServer).Guard(GuardServerRunning)
                                                                .GuardInSet(Servers.Append(null));

        // update the selected server when the server set changes
        var selectedServerUpdateSubscription = filteredServersChanges.Subscribe(_ => ServerVM.Value = Servers.FirstOrDefault());

        this.subscriptions = [serverUpdatesSubscription, selectedServerUpdateSubscription];

        // trigger all our server descriptions to load lazily.
        _ = Task.WhenAll(servers.Select(server => Task.Run(() => _ = server.Solution.Description)));
    }

    Exception? GuardServerRunning(ServerVM? _) 
        => ServerVM?.Value?.Server.CurrentlyListening ?? false ? new InvalidOperationException("Server is currently running!") : null;

    static Func<ServerVM, bool> MakeFilter(IProblem? problem) 
        => serverVM => problem is not null && serverVM.Server.Solution == problem;

    public async Task StopServer()
    {
        if  (ServerVM.Value?.Server is not null) (await ServerVM.Value.Server.Stop()).DiscardUnsubscribe();
    }

    /// <summary>Called when the app exits. Save the current state out to json.</summary>
    public IState GetState() => new ServerManagerState() { 
        Server = ServerVM.Value?.Server.Name.Value,
        Problem = Problem.Value.Name,
    };

    public void Dispose() => this.subscriptions.Dispose();

    public static readonly ServerManager Mockup = new(ProtoHackersDotNet.Servers.Problem.Instances.All, [MockupServer.Default], new());
}