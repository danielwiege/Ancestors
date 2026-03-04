using System.Globalization;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace AncestorsApp.Rendering;

public class TreeRenderer
{
    private readonly double _nodeWidth;
    private readonly double _nodeHeight;

    public TreeRenderer(double nodeWidth, double nodeHeight)
    {
        _nodeWidth = nodeWidth;
        _nodeHeight = nodeHeight;
    }

    public void Render(Canvas canvas, TreeLayoutResult layout, PersonNode? selected, Action<PersonNode> onNodeSelected)
    {
        canvas.Children.Clear();

        foreach (var parent in layout.Positions.Keys)
        {
            foreach (var child in parent.Children)
            {
                if (!layout.Positions.TryGetValue(child, out var childPos))
                {
                    continue;
                }

                var parentPos = layout.Positions[parent];
                canvas.Children.Add(new Line
                {
                    X1 = parentPos.X + _nodeWidth / 2,
                    Y1 = parentPos.Y + _nodeHeight,
                    X2 = childPos.X + _nodeWidth / 2,
                    Y2 = childPos.Y,
                    Stroke = Brushes.SlateGray,
                    StrokeThickness = 2
                });
            }
        }

        foreach (var (node, pos) in layout.Positions)
        {
            var border = CreateNodeControl(node, selected == node, onNodeSelected);
            Canvas.SetLeft(border, pos.X);
            Canvas.SetTop(border, pos.Y);
            canvas.Children.Add(border);
        }

        canvas.Width = layout.CanvasWidth;
        canvas.Height = layout.CanvasHeight;
    }

    private Border CreateNodeControl(PersonNode node, bool isSelected, Action<PersonNode> onNodeSelected)
    {
        var panel = new StackPanel { Margin = new System.Windows.Thickness(8) };

        panel.Children.Add(new TextBlock
        {
            Text = node.DisplayName,
            FontSize = 15,
            FontWeight = System.Windows.FontWeights.SemiBold,
            TextAlignment = System.Windows.TextAlignment.Center,
            TextWrapping = System.Windows.TextWrapping.Wrap
        });

        panel.Children.Add(new TextBlock
        {
            Text = $"* {FormatDate(node.BirthDate)}  † {FormatDate(node.DeathDate)}",
            Foreground = Brushes.DimGray,
            Margin = new System.Windows.Thickness(0, 4, 0, 0),
            TextAlignment = System.Windows.TextAlignment.Center
        });

        panel.Children.Add(new TextBlock
        {
            Text = node.BirthPlace,
            Foreground = Brushes.SteelBlue,
            FontStyle = System.Windows.FontStyles.Italic,
            Margin = new System.Windows.Thickness(0, 4, 0, 0),
            TextAlignment = System.Windows.TextAlignment.Center
        });

        var border = new Border
        {
            Width = _nodeWidth,
            Height = _nodeHeight,
            Background = isSelected ? new SolidColorBrush(System.Windows.Media.Color.FromRgb(222, 239, 255)) : Brushes.White,
            BorderBrush = isSelected ? Brushes.DodgerBlue : Brushes.LightGray,
            BorderThickness = new System.Windows.Thickness(isSelected ? 3 : 1.3),
            CornerRadius = new CornerRadius(8),
            Child = panel,
            Cursor = Cursors.Hand,
            ToolTip = string.IsNullOrWhiteSpace(node.Notes) ? "Keine Notizen" : node.Notes
        };

        border.MouseLeftButtonDown += (_, _) => onNodeSelected(node);
        return border;
    }

    private static string FormatDate(DateTime? date)
    {
        return date?.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture) ?? string.Empty;
    }
}
