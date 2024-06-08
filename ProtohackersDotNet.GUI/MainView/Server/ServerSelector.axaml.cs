using Avalonia.Controls.Presenters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using System.Reactive.Subjects;

namespace ProtoHackersDotNet.GUI.MainView.Server;

public partial class ServerSelector : UserControl
{
    readonly BehaviorSubject<ContentPresenter?> descriptionPresenterObserver = new(null);
    IDisposable? selectedServerChangeBinding;

    public ServerSelector()
    {
        InitializeComponent();
        var descriptionPresenterObservable = Observable.Merge(
            ProblemCarousel.GetObservable(LoadedEvent).Select(e => Unit.Default),
            ProblemCarousel.GetObservable(Carousel.SelectedItemProperty).Select(p => Unit.Default)
        ).Select(_ => ProblemCarousel.FindLogicalDescendantOfType<ScrollViewer>()?.Presenter);
        descriptionPresenterObservable.Subscribe(descriptionPresenterObserver).DiscardUnsubscribe();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        ServerManager serverManager = (ServerDropdown.DataContext as ServerManager) 
            ?? ThrowInvalidOperationException<ServerManager>($"Data context not {nameof(serverManager)}.");
        selectedServerChangeBinding = ServerDropdown.Bind(ComboBox.SelectedItemProperty, serverManager.SelectedServerChanges);
        
        base.OnLoaded(e);
    }

    void CarouselButton_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(this.descriptionPresenterObserver.Value);
        this.descriptionPresenterObserver.Value.RaiseEvent(e);
    }

    void CarouselButtonLeft_Click(object? sender, RoutedEventArgs e) => ProblemCarousel.Previous();

    void CarouselButtonRight_Click(object? sender, RoutedEventArgs e) => ProblemCarousel.Next();

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        this.descriptionPresenterObserver.Dispose();
        this.selectedServerChangeBinding?.Dispose();
    }
}