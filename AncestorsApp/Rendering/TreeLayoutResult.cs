using System.Windows;

namespace AncestorsApp.Rendering;

public class TreeLayoutResult
{
    public Dictionary<PersonNode, Point> Positions { get; init; } = new();
    public double CanvasWidth { get; init; }
    public double CanvasHeight { get; init; }
}
