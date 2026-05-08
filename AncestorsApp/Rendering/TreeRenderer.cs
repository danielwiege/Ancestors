using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
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
                canvas.Children.Add(CreateConnector(parentPos, childPos));
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

    private Path CreateConnector(Point parentPos, Point childPos)
    {
        var start = new Point(parentPos.X + _nodeWidth / 2, parentPos.Y + _nodeHeight);
        var end = new Point(childPos.X + _nodeWidth / 2, childPos.Y);
        var midY = start.Y + (end.Y - start.Y) * 0.55;

        var figure = new PathFigure { StartPoint = start, IsClosed = false, IsFilled = false };
        figure.Segments.Add(new BezierSegment(
            new Point(start.X, midY),
            new Point(end.X, midY),
            end,
            true));

        var geometry = new PathGeometry();
        geometry.Figures.Add(figure);

        return new Path
        {
            Data = geometry,
            Stroke = new SolidColorBrush(Color.FromRgb(168, 179, 194)),
            StrokeThickness = 2.2,
            StrokeStartLineCap = PenLineCap.Round,
            StrokeEndLineCap = PenLineCap.Round
        };
    }

    private Border CreateNodeControl(PersonNode node, bool isSelected, Action<PersonNode> onNodeSelected)
    {
        var accent = new Border
        {
            Width = 6,
            Background = isSelected
                ? new SolidColorBrush(Color.FromRgb(37, 99, 235))
                : new SolidColorBrush(Color.FromRgb(96, 165, 250)),
            CornerRadius = new CornerRadius(8, 0, 0, 8)
        };

        var content = new StackPanel
        {
            Margin = new Thickness(12, 10, 12, 10),
            VerticalAlignment = VerticalAlignment.Center
        };

        content.Children.Add(new TextBlock
        {
            Text = GetDisplayName(node),
            FontSize = 15.5,
            FontWeight = FontWeights.SemiBold,
            Foreground = new SolidColorBrush(Color.FromRgb(23, 32, 51)),
            TextTrimming = TextTrimming.CharacterEllipsis
        });

        content.Children.Add(new TextBlock
        {
            Text = FormatLifeSpan(node),
            Foreground = new SolidColorBrush(Color.FromRgb(88, 99, 116)),
            FontSize = 12,
            Margin = new Thickness(0, 6, 0, 0),
            TextTrimming = TextTrimming.CharacterEllipsis
        });

        content.Children.Add(new TextBlock
        {
            Text = string.IsNullOrWhiteSpace(node.BirthPlace) ? "Ort nicht angegeben" : node.BirthPlace,
            Foreground = new SolidColorBrush(Color.FromRgb(29, 78, 216)),
            FontSize = 12,
            Margin = new Thickness(0, 4, 0, 0),
            TextTrimming = TextTrimming.CharacterEllipsis
        });

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.Children.Add(accent);
        Grid.SetColumn(content, 1);
        grid.Children.Add(content);

        var border = new Border
        {
            Width = _nodeWidth,
            Height = _nodeHeight,
            Background = isSelected
                ? new SolidColorBrush(Color.FromRgb(248, 251, 255))
                : Brushes.White,
            BorderBrush = isSelected
                ? new SolidColorBrush(Color.FromRgb(37, 99, 235))
                : new SolidColorBrush(Color.FromRgb(210, 218, 229)),
            BorderThickness = new Thickness(isSelected ? 2 : 1),
            CornerRadius = new CornerRadius(8),
            Child = grid,
            Cursor = Cursors.Hand,
            ToolTip = string.IsNullOrWhiteSpace(node.Notes) ? "Keine Notizen" : node.Notes,
            Effect = new DropShadowEffect
            {
                Color = Colors.Black,
                BlurRadius = isSelected ? 16 : 10,
                Opacity = isSelected ? 0.16 : 0.08,
                ShadowDepth = 3
            }
        };

        border.MouseLeftButtonDown += (_, args) =>
        {
            args.Handled = true;
            onNodeSelected(node);
        };

        return border;
    }

    private static string FormatLifeSpan(PersonNode node)
    {
        var birthDate = FormatDate(node.BirthDate);
        var deathDate = FormatDate(node.DeathDate);

        if (string.IsNullOrEmpty(birthDate) && string.IsNullOrEmpty(deathDate))
        {
            return "Keine Datumsangaben";
        }

        if (string.IsNullOrEmpty(deathDate))
        {
            return $"Geb. {birthDate}";
        }

        if (string.IsNullOrEmpty(birthDate))
        {
            return $"Gest. {deathDate}";
        }

        return $"{birthDate} - {deathDate}";
    }

    private static string FormatDate(DateTime? date)
    {
        return date?.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture) ?? string.Empty;
    }

    private static string GetDisplayName(PersonNode node)
    {
        return string.IsNullOrWhiteSpace(node.DisplayName) ? "Unbenannte Person" : node.DisplayName;
    }
}
