using System.Collections.Generic;
using DALib.Definitions;
using SkiaSharp;

namespace DALib.Drawing;

/// <summary>
///     Represents a control parsed from a ControlFile
/// </summary>
public sealed class Control
{
    /// <summary>
    ///     The color indexes associated with the control. LI: not sure how this is used yet
    /// </summary>
    public List<int>? ColorIndexes { get; set; }

    /// <summary>
    ///     The images and frames of those images associated with the control. These are intended to be rendered in the order
    ///     given.
    /// </summary>
    public List<(string ImageName, int FrameIndex)>? Images { get; set; }

    /// <summary>
    ///     The name of the control
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    ///     The rect that represents the bounds of the control as they exist within the containing control.
    /// </summary>
    /// <remarks>
    ///     If the contaiing control's top left corner is at (100, 100) and it has a child control at (0, 0), the child control
    ///     is at (0, 0) within the parent control, so it's screen coordinates would be (100, 100)
    /// </remarks>
    public SKRect? Rect { get; set; }

    /// <summary>
    ///     The return value of the control when interacted with
    /// </summary>
    public int? ReturnValue { get; set; }

    /// <summary>
    ///     The type of the control //LI: the numbers are correct, i'm still exploring how these types are actually used
    /// </summary>
    public ControlType Type { get; set; }
}