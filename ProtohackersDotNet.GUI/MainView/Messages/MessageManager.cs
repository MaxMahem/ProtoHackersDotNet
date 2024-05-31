using CommunityToolkit.Mvvm.ComponentModel;
using ProtoHackersDotNet.GUI.MainView.ProtoHackerApi;
using System.Diagnostics;

namespace ProtoHackersDotNet.GUI.MainView.Messages;
public partial class MessageManager : ObservableObject
{
    readonly SourceCache<MessageVM, int> messageCache = new(messageVM => messageVM.Id);
    [SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "Binding target")]
    ReadOnlyObservableCollection<MessageVM> messages;
    public ReadOnlyObservableCollection<MessageVM> Messages => this.messages;

    readonly HashSet<IEventSource> eventSources = [];

    readonly MemberObservingDictionary<string, StringFilterEntry, bool> sourceFilter 
        = new(entry => entry.Entry, entry => entry.SelectedUpdates);
    public ListFilterManager SourceFilter { get; }

    public ObservableValue<string?> MessageSearch { get; } = new(null);

    [ObservableProperty]
    bool loggingEnabled = false;

    [ObservableProperty]
    string logFileName = string.Empty;

    FileStream? logFileStream;
    StreamWriter? logStream; 

    public IObservable<int> MessageCount => this.messageCache.CountChanged;

    public MessageManager()
    {
        SourceFilter = new(this.sourceFilter);
        this.messageCache.Connect()
            .Filter(this.sourceFilter.Updated.Select(BuildSourceFilter))
            .Filter(MessageSearch.Values.Select(BuildMessageFilter))
            .SortAndBind(out this.messages).Subscribe().DiscardDisposable();

        Func<MessageVM, bool> BuildMessageFilter(string? search) => messageVM 
            => string.IsNullOrEmpty(search) || messageVM.Message.Contains(search, StringComparison.CurrentCulture);
        Func<MessageVM, bool> BuildSourceFilter(Unit unit) => messageVM 
            => this.sourceFilter[messageVM.Source].Selected;
    }

    public void SubscribeToStream<TEvent>(EventSource<TEvent> streamSource)
        where TEvent : IDisplayEvent
    {
        bool added = this.eventSources.Add(streamSource);
        Debug.Assert(added);

        this.sourceFilter.AddEntries(streamSource.SourceNames.Select(name => new StringFilterEntry(name)));
        streamSource.EventStream.Subscribe(TranslateAndPost, PostException).DiscardDisposable();

        void TranslateAndPost(TEvent streamEvent) {
            var message = streamSource.Translator(streamEvent);
            PostMessage(message); 
        }
    }

    void PostMessage(MessageVM message)
    {
        this.messageCache.AddOrUpdate(message);
    
        if (LoggingEnabled) {
            this.logStream?.WriteLine($"{message.Source}, {message.Timestamp:s}, {message.Message}");
            this.logStream?.Flush();
        }
    }

    void PostException(Exception exception)
    {
        string source = exception switch {
            ClientException clientException => clientException.Client.ClientEndPoint.ToString(),
            ProtoHackerApiException apiException => apiException.Api?.ToString() ?? "Unknown",
            _ => "Unknown",
        };
        this.messageCache.AddOrUpdate(MessageVM.FromException(exception, source));
        this.sourceFilter.AddEntry(new StringFilterEntry(source));
    }

    /// <summary>Clears messages and removes source filters that can be removed.
    /// Sources which are not completed cannot be removed.</summary>
    public void ClearMessages()
    {
        this.messageCache.Clear();

        // remove completed sources.
        var sourcesToRemove = this.eventSources.Where(source => source.Completed.IsCompleted);
        this.eventSources.ExceptWith(sourcesToRemove);
        this.sourceFilter.Remove(sourcesToRemove.SelectMany(source => source.SourceNames));

        MessageSearch.LatestValue = null;
    }
}