using Avalonia.Controls.Primitives;
using Avalonia.Xaml.Interactivity;

namespace ProtoHackersDotNet.GUI.Behaviors;

public class ScrollToEndListBoxBehavior : Behavior<ListBox>
{
    Vector scrollPosition = Vector.One;

    static Vector GetScrollPercent(ScrollViewer scrollViewer)
        => new(scrollViewer.ScrollBarMaximum.X is 0 ? 0 : scrollViewer.Offset.X / scrollViewer.ScrollBarMaximum.X,
               scrollViewer.ScrollBarMaximum.Y is 0 ? 0 : scrollViewer.Offset.Y / scrollViewer.ScrollBarMaximum.Y);

    static Vector GetScrollOffset(ScrollViewer scrollViewer, Vector position)
        => new(scrollViewer.ScrollBarMaximum.X * position.X,
               scrollViewer.ScrollBarMaximum.Y * position.Y);

    protected override void OnAttachedToVisualTree()
    {
        if (AssociatedObject is not null)
            AssociatedObject.TemplateApplied += AssociatedObjectOnTemplateApplied;
    }

    protected override void OnDetachedFromVisualTree()
    {
        if (AssociatedObject is not null)
        {
            AssociatedObject.TemplateApplied -= AssociatedObjectOnTemplateApplied;
            if (AssociatedObject.FindControl<ScrollViewer>("PART_ScrollViewer") is ScrollViewer scrollViewer)
                scrollViewer.ScrollChanged -= ScrollViewerOnScrollChanged;
        }
    }

    void AssociatedObjectOnTemplateApplied(object? sender, TemplateAppliedEventArgs args)
        => args.NameScope.Get<ScrollViewer>("PART_ScrollViewer").ScrollChanged += ScrollViewerOnScrollChanged;

    void ScrollViewerOnScrollChanged(object? sender, ScrollChangedEventArgs args)
    {
        if (sender is ScrollViewer scrollViewer)
        {
            // window changed size. Scroll
            if ((args.ExtentDelta.Length is not 0 || args.ViewportDelta.Length is not 0)
                && GetScrollOffset(scrollViewer, scrollPosition) is Vector offset)
                scrollViewer.Offset = offset;
            // offset changed, set new scroll position
            else if (args.OffsetDelta.Length > 0)
                scrollPosition = GetScrollPercent(scrollViewer);
        }
    }
}