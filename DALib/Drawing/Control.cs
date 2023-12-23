using System.Collections.Generic;
using DALib.Definitions;
using SkiaSharp;

namespace DALib.Drawing;

public sealed class Control
{
    public int? ButtonResultValue { get; set; }
    public List<int>? ColorIndexes { get; set; }
    public List<(string ImageName, int FrameIndex)>? Images { get; set; }
    public string Name { get; set; } = null!;
    public SKRect? Rect { get; set; }
    public ControlType Type { get; set; }
}