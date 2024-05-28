using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using DynamicData.Kernel;
using ProtoHackersDotNet.GUI.MainView.ProtoHackerApi;
using ProtoHackersDotNet.Servers.Helpers;
using ProtoHackersDotNet.Servers.Interface.Exceptions;

namespace ProtoHackersDotNet.GUI.MainView.Messages;
public partial class MessageManager : ObservableObject
{
    readonly SourceCache<MessageVM, int> messageCache = new(messageVM => messageVM.Id);
    [SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "Binding target")]
    ReadOnlyObservableCollection<MessageVM> messages;
    public ReadOnlyObservableCollection<MessageVM> Messages => this.messages;

    public FilterManager Filter { get; } = new();

    [ObservableProperty]
    bool loggingEnabled = false;

    [ObservableProperty]
    string logFileName = string.Empty;

    FileStream? logFileStream;
    StreamWriter? logStream; 

    public IObservable<int> MessageCount => this.messageCache.CountChanged;

    public MessageManager()
    {
        this.messageCache.Connect().Bind(out this.messages)
            .Filter(Filter.FilterListUpdates).Filter(message => !Filter[message.StreamName])
            .Subscribe().DiscardDisposable();
    }

    public void ClearMessages()
    {
        this.messageCache.Clear();
        Filter.Clear();
    }

    public void SubscribeToStream<T>(IObservable<T> stream, Func<T, string, MessageVM> translator, string streamName)
    {
        Filter.Add(streamName);
        this.messageCache.PopulateFrom(stream.Select(streamEvent => translator(streamEvent, streamName)))
                         .DiscardDisposable();
        stream.Subscribe(Stub.DoNothing, exception => PostException(exception, streamName)).DiscardDisposable();
    }

    // public void PostMessage(IDisplayMessage message)
    // {        
    //     lock (postLock) {
    // 
    //         if (this.sources.Add(message.Source)) SourceFilter.Add(new(message.Source));
    // 
    //         if (LoggingEnabled) {
    //             this.logStream?.WriteLine($"{message.Source}, {message.Timestamp:s}, {message.Message}");
    //             this.logStream?.Flush();
    //         }
    //     }
    // }

    public void PostException(Exception exception, string streamName)
    {
        var source = exception switch {
            ClientException clientException => clientException.Client.ClientEndPoint.ToString(),
            ProtoHackerApiException apiException => apiException?.ToString() ?? "Unknown Api",
            _ => "Unknown",
        };
        this.messageCache.AddOrUpdate(MessageVM.FromException(exception, source, streamName));
    }

}

public class MessageManagerOptions
{
    public required string LogFilePathBase { get; init; }
}
