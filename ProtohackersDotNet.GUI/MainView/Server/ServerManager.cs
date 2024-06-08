using ProtoHackersDotNet.GUI.Serialization;
using ProtoHackersDotNet.Servers;
using ReactiveUI;
using System.ComponentModel;
using System.Reactive.Disposables;

namespace ProtoHackersDotNet.GUI.MainView.Server;

public sealed class ServerManager : IStateSaveable
{
    readonly ObservableValue<Problem> observableProblem;

    readonly ValidateableValue<ServerVM?> observableServerVM;

    /// <summary>Gets or sets the currently selected server.</summary>
    public ServerVM? SelectedServer {
        get => this.observableServerVM?.CurrentValue;
        set => this.observableServerVM.CurrentValue = observableServerVM.CurrentValue?.Server.CurrentlyListening ?? false
            ? ThrowInvalidOperationException<ServerVM>("Server is currently running!")
            : value;
    }
    public IObservable<ServerVM?> SelectedServerChanges => observableServerVM.Value;

    /// <summary>Gets or sets the currently selected problem.</summary>
    public Problem SelectedProblem {
        get => this.observableProblem.CurrentValue;
        set => this.observableProblem.CurrentValue = Problems.Contains(value) ? value
                : ThrowArgumentOutOfRangeException<Problem>();
    }

    /// <summary>Provides updates when the value of <see cref="ServerVM.Server"/> changes.</summary>
    public IObservable<IServer?> Server => this.observableServerVM.Value.Select(vm => vm?.Server);

    /// <summary>Provides updates when the validity of the server changes.</summary>
    public IObservable<bool> ServerValid => this.observableServerVM.Valid;

    /// <summary>Gets the current selected server.</summary>
    public IServer? CurrentServer => this.observableServerVM.CurrentValue?.Server;

    /// <summary>Gets the known set of problems.</summary>
    public ReadOnlyObservableCollection<Problem> Problems { get; }

    [SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "Bind target")]
    ReadOnlyObservableCollection<ServerVM> servers;

    /// <summary>Gets the current set of available servers.</summary>
    /// <remarks>Filtered based on <see cref="SelectedProblem"/> value.</remarks>
    public ReadOnlyObservableCollection<ServerVM> Servers => this.servers;

    readonly SourceCache<ServerVM, ServerVM> serverCache = new(server => server);

    readonly CompositeDisposable subscriptions;

    public ServerManager(IEnumerable<Problem> problems, IEnumerable<IServer> servers, ServerManagerState options)
    {
        var initialProblem = problems.FirstOrDefault(problem => problem.Name == options.Problem,
            defaultValue: problems.First());
        this.observableProblem = new(initialProblem);

        Problems = new ObservableCollection<Problem>(problems).AsReadOnlyObservableCollection();

        // build the filterable Server collection
        this.serverCache.AddOrUpdate(servers.Select(ServerVM.Create));
        var filteredServersChanges = this.serverCache.Connect().Filter(this.observableProblem.Value.Select(BuiltFilter));
        static Func<ServerVM, bool> BuiltFilter(Problem? problem) => serverVM
            => problem is not null && serverVM.Server.Solution == problem;
        var serverUpdatesSub = filteredServersChanges.ObserveOn(RxApp.MainThreadScheduler).Bind(out this.servers).Subscribe();

        var initialServer = Servers.FirstOrDefault(server => server?.Server.Name.Value == options.Server,
            defaultValue: Servers.FirstOrDefault()
        );
        this.observableServerVM = ValidateableValue<ServerVM?>.NotNull(initialServer);

        // update the selected server when the server set changes
        var selectedServerUpdateSub = filteredServersChanges.Subscribe(UpdateSelectedServer);
        void UpdateSelectedServer(IChangeSet<ServerVM, ServerVM> changes) 
            => SelectedServer = Servers.FirstOrDefault();

        this.subscriptions = [serverUpdatesSub, selectedServerUpdateSub];

        // this should trigger all our server descriptions to load lazily.
        _ = Task.WhenAll(servers.Select(server => Task.Run(() => _ = server.Solution.Description)));
    }

    public async Task StopServer()
    {
        if  (CurrentServer is not null) await CurrentServer.Stop();
    }

    /// <summary>Called when the app exits. Save the current state out to json.</summary>
    public IState GetState() => new ServerManagerState() { 
        Server = this.observableServerVM.CurrentValue?.Name,
        Problem = this.observableProblem.CurrentValue.Name,
    };

    public static readonly ServerManager Mockup = new(Problem.Problems, [MockupServer.Default], new());
}