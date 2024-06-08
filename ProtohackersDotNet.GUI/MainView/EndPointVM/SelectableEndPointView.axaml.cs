namespace ProtoHackersDotNet.GUI.MainView.EndPoint;

public partial class SelectableEndPointView : UserControl
{
    public static readonly DirectProperty<SelectableEndPointView, SelectableEndPoint?> EndPointProperty =
        AvaloniaProperty.RegisterDirect<SelectableEndPointView, SelectableEndPoint?>(
            name: nameof(GUI.MainView.EndPoint), 
            getter: endPointView => endPointView.EndPoint, 
            setter: (endPointView, endPoint) => endPointView.EndPoint = endPoint);

    SelectableEndPoint? endPoint;

    public SelectableEndPoint? EndPoint {
        get => endPoint;
        set => SetAndRaise(EndPointProperty, ref endPoint, value);
    }

    public SelectableEndPointView() => InitializeComponent();
}