using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using ProtoHackersDotNet.GUI.MainView.ProtoHackerApi;
using ProtoHackersDotNet.Servers.Helpers;
using ProtoHackersDotNet.Servers.Interface.Exceptions;
using System.Reactive.Subjects;

namespace ProtoHackersDotNet.GUI.MainView.Messages;
public partial class MessageManager : ObservableObject
{
    readonly SourceCache<MessageVM, int> messageCache = new(messageVM => messageVM.Id);
    [SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "Binding target")]
    ReadOnlyObservableCollection<MessageVM> messages;
    public ReadOnlyObservableCollection<MessageVM> Messages => this.messages;

    readonly ListFilter sourceFilter = new();
    public ListFilterManager SourceFilter { get; }

    public ObservableProperty<string?> MessageSearch { get; } = new(null);

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
            .Filter(this.sourceFilter.Updates.Select(BuildSourceFilter))
            .Filter(MessageSearch.Value.Select(BuildMessageFilter))
            .Bind(out this.messages).Subscribe().DiscardDisposable();

        Func<MessageVM, bool> BuildMessageFilter(string? search) => messageVM 
            => string.IsNullOrEmpty(search) || messageVM.Message.Contains(search, StringComparison.CurrentCulture);
        Func<MessageVM, bool> BuildSourceFilter(Unit unit) => messageVM => this.sourceFilter[messageVM.Source];
    }

    public void ClearMessages()
    {
        this.messageCache.Clear();
        this.sourceFilter.Clear();
    }

    public void SubscribeToStream<T>(IObservable<T> stream, Func<T, MessageVM> translator, params string[] streamNames)
    {
        this.sourceFilter.AddEntries(streamNames);
        stream.Subscribe(TranslateAndPost, PostException).DiscardDisposable();

        void TranslateAndPost(T streamEvent) {
            var message = translator(streamEvent);
            PostMessage(message); 
        }
    }

    void PostMessage(MessageVM message)
    {
        this.messageCache.AddOrUpdate(message);
        // if (this.sources.Add(message.Source)) SourceFilter.Add(new(message.Source));
    
        if (LoggingEnabled) {
            this.logStream?.WriteLine($"{message.Source}, {message.Timestamp:s}, {message.Message}");
            this.logStream?.Flush();
        }
    }

    void PostException(Exception exception)
    {
        var source = exception switch {
            ClientException clientException => clientException.Client.ClientEndPoint.ToString(),
            ProtoHackerApiException apiException => apiException?.ToString() ?? "Unknown Api",
            _ => "Unknown",
        };
        this.messageCache.AddOrUpdate(MessageVM.FromException(exception, source));
        this.sourceFilter.AddEntry(source);
    }

}

public sealed class ObservableProperty<T>(T initialValue)
{
    readonly BehaviorSubject<T> valueObserver = new(initialValue);
    public T LatestValue { 
        get => this.valueObserver.Value;
        set => this.valueObserver.OnNext(value);
    }
    public IObservable<T> Value => this.valueObserver.AsObservable();
}