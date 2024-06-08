using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;
using ProtoHackersDotNet.GUI.MainView.Messages;

namespace ProtoHackersDotNet.GUI.MainView;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
        // ProblemDescription.FindLogicalDescendantOfType<ScrollViewer>()!.TemplateApplied += DescriptionScrollViewer_TemplateApplied;
    }

    ScrollContentPresenter? descriptionScrollContentPresenter;

    void DescriptionScrollViewer_TemplateApplied(object? sender, TemplateAppliedEventArgs e)
        => this.descriptionScrollContentPresenter = e.NameScope.Get<ScrollContentPresenter>("PART_ContentPresenter");

    void CarouselButton_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
        => this.descriptionScrollContentPresenter!.RaiseEvent(e);

    void SourceLabel_Tapped(object? sender, TappedEventArgs e)
    {
        if (sender != Source_Label) ThrowInvalidOperationException();
        var vm = Source_Label.DataContext as MessageManager ?? ThrowInvalidOperationException<MessageManager>();
        if (vm.SourceFilter.Entries.Count is 0) return;

        FlyoutBase.ShowAttachedFlyout(Source_Label);
    }

    void MessageLabel_Tapped(object? sender, TappedEventArgs e)
    {
        if (sender != Message_Label) ThrowInvalidOperationException();
        var vm = Message_Label.DataContext as MessageManager ?? ThrowInvalidOperationException<MessageManager>();
        if (vm.SourceFilter.Entries.Count is 0) return;

        FlyoutBase.ShowAttachedFlyout(Message_Label);
    }
}
