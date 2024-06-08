namespace ProtoHackersDotNet.GUI.MainView.EndPoint;

public partial class TextEndPointView : UserControl
{
    public static readonly DirectProperty<TextEndPointView, TextEndPoint?> EndPointProperty =
        AvaloniaProperty.RegisterDirect<TextEndPointView, TextEndPoint?>(
            name: nameof(GUI.MainView.EndPoint),
            getter: endPointView => endPointView.EndPoint,
            setter: (endPointView, endPoint) => endPointView.EndPoint = endPoint);

    TextEndPoint? endPoint;

    public TextEndPoint? EndPoint {
        get => endPoint;
        set => SetAndRaise(EndPointProperty, ref endPoint, value);
    }

    public TextEndPointView() => InitializeComponent();
}