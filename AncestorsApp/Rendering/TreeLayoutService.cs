using System.Windows;

namespace AncestorsApp.Rendering;

public class TreeLayoutService
{
    private const double CanvasPadding = 56;
    private const double MinimumCanvasWidth = 1040;
    private const double MinimumCanvasHeight = 680;

    private readonly double _nodeWidth;
    private readonly double _nodeHeight;
    private readonly double _horizontalSpacing;
    private readonly double _verticalSpacing;

    public TreeLayoutService(double nodeWidth, double nodeHeight, double horizontalSpacing, double verticalSpacing)
    {
        _nodeWidth = nodeWidth;
        _nodeHeight = nodeHeight;
        _horizontalSpacing = horizontalSpacing;
        _verticalSpacing = verticalSpacing;
    }

    public TreeLayoutResult Calculate(PersonNode root)
    {
        var positions = new Dictionary<PersonNode, Point>();
        var leafIndex = 0;
        CalculateLayout(root, 0, ref leafIndex, positions);

        var width = positions.Values.Select(p => p.X).DefaultIfEmpty().Max() + _nodeWidth + CanvasPadding;
        var height = positions.Values.Select(p => p.Y).DefaultIfEmpty().Max() + _nodeHeight + CanvasPadding;

        return new TreeLayoutResult
        {
            Positions = positions,
            CanvasWidth = Math.Max(width, MinimumCanvasWidth),
            CanvasHeight = Math.Max(height, MinimumCanvasHeight)
        };
    }

    private double CalculateLayout(PersonNode node, int depth, ref int leafIndex, Dictionary<PersonNode, Point> positions)
    {
        var y = CanvasPadding + depth * (_nodeHeight + _verticalSpacing);

        if (node.Children.Count == 0)
        {
            var x = CanvasPadding + leafIndex * (_nodeWidth + _horizontalSpacing);
            leafIndex++;
            positions[node] = new Point(x, y);
            return x;
        }

        var childCenters = new List<double>();
        foreach (var child in node.Children)
        {
            childCenters.Add(CalculateLayout(child, depth + 1, ref leafIndex, positions));
        }

        var centeredX = childCenters.Average();
        positions[node] = new Point(centeredX, y);
        return centeredX;
    }
}
