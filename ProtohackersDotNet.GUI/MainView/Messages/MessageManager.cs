using CommunityToolkit.Mvvm.ComponentModel;
using ReactiveUI;
using System.Reactive.Concurrency;

namespace ProtoHackersDotNet.GUI.MainView.Messages;
public partial class MessageManager : ObservableObject
{
    readonly MessageManagerOptions options;

    readonly SourceCache<MessageVM, int> messageCache = new(messageVM => messageVM.Id);
    [SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "Binding target")]
    ReadOnlyObservableCollection<MessageVM> messages;
    public ReadOnlyObservableCollection<MessageVM> Messages => this.messages;

    readonly HashSet<IEventSource> eventSources = [];

    readonly SourceCache<StringFilterEntry, string> sourceFilter 
        = new(entry => entry.Entry);
    public ListFilterManager SourceFilter { get; }

    public ObservableValue<string?> MessageSearch { get; } = new(null);

    [ObservableProperty]
    bool loggingEnabled = false;

    partial void OnLoggingEnabledChanged(bool value)
    {
        if (value) {
            LogFileName = Path.Combine(options.LogFilePathBase, DateTime.Now.ToString("yyMMdd-HHmmss") + ".log");
            this.logFileStream = File.Create(LogFileName);
            this.logStream = new StreamWriter(this.logFileStream);
        }
    }

    [ObservableProperty]
    string logFileName = string.Empty;

    FileStream? logFileStream;
    StreamWriter? logStream; 

    public IObservable<int> MessageCount => this.messageCache.CountChanged;

    public MessageManager(MessageManagerOptions options)
    {
        this.options = options;

        SourceFilter = new(this.sourceFilter);
        this.messageCache.Connect()
            .Filter(this.sourceFilter.Connect().AutoRefreshOnObservable(messageVM => messageVM.SelectedUpdates)
                                     .Select(BuildSourceFilter))
            .Filter(MessageSearch.Value.Select(BuildMessageFilter))
            .ObserveOn(RxApp.MainThreadScheduler)
            .SortAndBind(out this.messages).Subscribe().DiscardUnsubscribe();

        static Func<MessageVM, bool> BuildMessageFilter(string? search) => messageVM 
            => string.IsNullOrEmpty(search) || messageVM.Message.Contains(search, StringComparison.CurrentCulture);
        Func<MessageVM, bool> BuildSourceFilter<T>(T _) => messageVM 
            => this.sourceFilter.Lookup(messageVM.Source) is { HasValue: true } lookup 
                ? lookup.Value.Selected : true;
    }

    public void SubscribeToStream(EventSource streamSource)
    {
        bool added = this.eventSources.Add(streamSource);
        Debug.Assert(added);

        this.sourceFilter.AddOrUpdate(streamSource.SourceNames.Select(name => new StringFilterEntry(name)));
        streamSource.EventStream.Timestamp().ObserveOn(TaskPoolScheduler.Default).Subscribe(
            onNext: streamEvent => PostMessage(MessageVM.FromEvent(streamEvent)), 
            onError: exception => PostException(exception, streamSource.MessageSource, streamSource.SourceNames.First())
        ).DiscardUnsubscribe();
    }

    readonly object postGate = new();

    void PostMessage(MessageVM message)
    {
        lock (postGate) {
            // this.sourceFilter.AddOrUpdate(new StringFilterEntry(message.Value.Source));
            this.messageCache.AddOrUpdate(message);

            if (LoggingEnabled) {
                this.logStream?.WriteLine($"{message.Source}, {message.Timestamp:s}, {message.Message}");
                this.logStream?.Flush();
            }
        }
    }

    void PostException(Exception exception, MessageSource messageSource, string sourceName)
    {
        this.messageCache.AddOrUpdate(MessageVM.FromException(exception, messageSource, sourceName));
        this.sourceFilter.AddOrUpdate(new StringFilterEntry(sourceName));
    }

    /// <summary>Clears messages and removes source filters that can be removed.
    /// Sources which are not completed cannot be removed.</summary>
    public void ClearMessages()
    {
        this.messageCache.Clear();

        // remove completed sources.
        var sourcesToRemove = this.eventSources.Where(source => source.Completed.IsCompleted);
        this.eventSources.ExceptWith(sourcesToRemove);
        this.sourceFilter.RemoveKeys(sourcesToRemove.SelectMany(source => source.SourceNames));

        MessageSearch.CurrentValue = null;
    }
}