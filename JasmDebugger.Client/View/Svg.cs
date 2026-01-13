// -----------------------------------------------------------------------
// <copyright file="Svg.cs">
//     Created by Frank Listing at 2025/12/19.
// </copyright>
// -----------------------------------------------------------------------

namespace JasmDebugger.Client.View;

using System.Drawing;
using System.Xml.Linq;

/// <summary>
///     Helper class to generate a string representation of SVG styles.
/// </summary>
public class Style(double strokeWidth, Color fill, Color stroke)
{
    /// <summary>
    ///     Gets the width of the stroke.
    /// </summary>
    public double StrokeWidth { get; } = strokeWidth;

    /// <summary>
    ///     Gets the fill color.
    /// </summary>
    public Color Fill { get; } = fill;

    /// <summary>
    ///     Gets the stroke color.
    /// </summary>
    public Color Stroke { get; } = stroke;

    /// <summary>
    ///     Returns a string that represents the current object as an SVG style.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString() =>
        $"stroke-width:{this.StrokeWidth.Num()}px;stroke:{ColorTranslator.ToHtml(this.Stroke)};fill:{ColorTranslator.ToHtml(this.Fill)}";
}

/// <summary>
///     Helper class to generate a string representation of an SVG style for a transition.
/// </summary>
public class TransitionStyle(double strokeWidth, Color fill, Color stroke) : Style(strokeWidth, fill, stroke)
{
    /// <summary>
    ///     Returns a string that represents the current object as an SVG style.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString() => $"{base.ToString()};marker-end:url(#ArrowWideRounded)";
}

/// <summary>
///     Helper class to generate a string representation of an SVG style for the background.
/// </summary>
public class BackgroundStyle(double strokeWidth, Color fill, Color stroke) : Style(strokeWidth, fill, stroke)
{
    /// <summary>
    ///     Returns a string that represents the current object as an SVG style.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString() => $"{base.ToString()};fill-opacity:.25;stroke-dasharray:0.45";
}

/// <summary>
///     Helper class to generate a string representation of an SVG style for text.
/// </summary>
public class TextStyle(int fontSize, string fontStretch = "normal")
{
    /// <summary>
    ///     Gets the font family.
    /// </summary>
    public static string FontFamily => "Arial,sans-serif";

    /// <summary>
    ///     Gets the size of the font.
    /// </summary>
    public int FontSize { get; } = fontSize;

    /// <summary>
    ///     Gets the font stretch information.
    /// </summary>
    public string FontStretch { get; } = fontStretch;

    /// <summary>
    ///     Returns a string that represents the current object as an SVG style.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString() =>
        $"font-family:{FontFamily};font-size:{this.FontSize}px;letter-spacing:0px;line-height:1.25;stroke-width:.26458;word-spacing:0px;font-stretch:{this.FontStretch}";
}

/// <summary>
///     A set of predefined styles for SVG elements.
/// </summary>
public static class Styles
{
    public static readonly Style State = new(0.2, Color.FromArgb(0xfa, 0xeb, 0xd7), Color.Black);
    public static readonly Style StateHighlighted = new(0.5, Color.FromArgb(0xfa, 0xeb, 0xd7), Color.Red);
    public static readonly Style StateFinal = new(0.2, Color.White, Color.Black);
    public static readonly Style StateFinalHighlighted = new(0.5, Color.White, Color.Red);
    public static readonly Style StateInitial = new(0.2, Color.Black, Color.Black);
    public static readonly Style StateInitialHighlighted = new(0.5, Color.Black, Color.Red);
    public static readonly Style SmallCircle = new(0.15, Color.FromArgb(0xfa, 0xeb, 0xd7), Color.Black);
    public static readonly Style Line = new(0.15, Color.Black, Color.Black);
    public static readonly Style History = new(0.15, Color.White, Color.Black);
    public static readonly TextStyle NormalText = new(2);
    public static readonly TextStyle BigText = new(3);
    public static readonly TextStyle BigTextCondensed = new(3, "condensed");
    public static readonly TransitionStyle Transition = new(0.15, Color.Black, Color.Black);
    public static readonly BackgroundStyle Background = new(0.15, Color.FromArgb(0xfa, 0xeb, 0xd7), Color.Black);
}

/// <summary>
///     Helper class to generate SVG elements.
/// </summary>
public static class Svg
{
    /// <summary>
    ///     The SVG namespace.
    /// </summary>
    public static readonly XNamespace SvgNs = "http://www.w3.org/2000/svg";

    /// <summary>
    ///     Generates a circle SVG element with the specified ID, location, radius, and style.
    /// </summary>
    /// <param name="id">The ID.</param>
    /// <param name="location">The location.</param>
    /// <param name="radius">The radius.</param>
    /// <param name="style">The style to apply.</param>
    /// <returns>Returns the circle SVG element.</returns>
    public static XElement Circle(string id, Point location, double radius, Style style) =>
        Circle(location, radius, style).AddId(id);

    /// <summary>
    ///     Generates a circle SVG element with the specified location, radius, and style.
    /// </summary>
    /// <param name="location">The location.</param>
    /// <param name="radius">The radius.</param>
    /// <param name="style">The style to apply.</param>
    /// <returns>Returns the circle SVG element.</returns>
    public static XElement Circle(Point location, double radius, Style style) =>
        Circle(location.X, location.Y, radius, style);

    /// <summary>
    ///     Generates a circle SVG element with the specified location, radius, and style.
    /// </summary>
    /// <param name="cx">The x-location of the center point.</param>
    /// <param name="cy">The y-location of the center point.</param>
    /// <param name="radius">The radius.</param>
    /// <param name="style">The style to apply.</param>
    /// <returns>Returns the circle SVG element.</returns>
    public static XElement Circle(double cx, double cy, double radius, Style style)
    {
        return new XElement(
            SvgNs + "circle",
            new XAttribute("cx", cx),
            new XAttribute("cy", cy),
            new XAttribute("r", radius),
            new XAttribute("style", style));
    }

    /// <summary>
    ///     Generates a group SVG element with the specified ID.
    /// </summary>
    /// <param name="id">The ID.</param>
    /// <returns>Returns the group SVG element.</returns>
    public static XElement Group(string id) => Group().AddId(id);

    /// <summary>
    ///     Generates a group SVG element.
    /// </summary>
    /// <returns>Returns the group SVG element.</returns>
    public static XElement Group() => new(SvgNs + "g");

    /// <summary>
    ///     Generates a group SVG element with the specified class information and location.
    /// </summary>
    /// <param name="classInfo">The class name(s) to add.</param>
    /// <param name="left">The left position.</param>
    /// <param name="top">The top position.</param>
    /// <returns>Returns the group SVG element.</returns>
    public static XElement Group(string classInfo, double left, double top) => new(
        SvgNs + "g",
        new XAttribute("class", classInfo),
        new XAttribute("transform", $"translate({left.Num()} {top.Num()})"));

    /// <summary>
    ///     Generates a fixed-size rect SVG element with the specified ID, location and style.
    /// </summary>
    /// <param name="id">The ID.</param>
    /// <param name="location">The location.</param>
    /// <param name="style">The style to apply.</param>
    /// <returns>Returns the rect SVG element.</returns>
    public static XElement Rect(string id, Point location, Style style) => Rect(location, style).AddId(id);

    /// <summary>
    ///     Generates a fixed-size rect SVG element with the specified location and style.
    /// </summary>
    /// <param name="location">The location.</param>
    /// <param name="style">The style to apply.</param>
    /// <returns>Returns the rect SVG element.</returns>
    public static XElement Rect(Point location, Style style) =>
        Rect(location.X - 10, location.Y - 4, 20, 8, 2, style);

    /// <summary>
    ///     Generates a rect SVG element with the location, size and style.
    /// </summary>
    /// <param name="x">The x-location (left).</param>
    /// <param name="y">The y-location (top).</param>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    /// <param name="r">The radius of the corners.</param>
    /// <param name="style">The style to apply.</param>
    /// <returns>Returns the rect SVG element.</returns>
    public static XElement Rect(double x, double y, double width, double height, double r, Style style)
    {
        return new XElement(
            SvgNs + "rect",
            new XAttribute("x", x),
            new XAttribute("y", y),
            new XAttribute("width", width),
            new XAttribute("height", height),
            new XAttribute("rx", r),
            new XAttribute("ry", r),
            new XAttribute("style", style));
    }

    /// <summary>
    ///     Generates a text SVG element with the specified text, location and style.
    ///     The location is the center of the text.
    /// </summary>
    /// <param name="text">The text to display</param>
    /// <param name="location">The location.</param>
    /// <param name="style">The text style to apply.</param>
    /// <returns>Returns the text SVG element.</returns>
    public static XElement CenteredText(string text, Point location, TextStyle style) =>
        CenteredText(text, location.X, location.Y, style);

    /// <summary>
    ///     Generates a text SVG element with the specified text, location and style.
    ///     The location is the center of the text.
    /// </summary>
    /// <param name="text">The text to display</param>
    /// <param name="x">The x-location of the center point.</param>
    /// <param name="y">The y-location of the center point.</param>
    /// <param name="style">The text style to apply.</param>
    /// <returns>Returns the text SVG element.</returns>
    public static XElement CenteredText(string text, double x, double y, TextStyle style) =>
        new(
            SvgNs + "text",
            new XAttribute("x", x),
            new XAttribute("y", y),
            new XAttribute("style", style),
            new XAttribute("text-anchor", "middle"),
            new XAttribute("dominant-baseline", "middle"),
            text);

    /// <summary>
    ///     Generates a text SVG element with the specified text, location and style.
    /// </summary>
    /// <param name="text">The text to display</param>
    /// <param name="x">The x-location (left).</param>
    /// <param name="y">The y-location (top).</param>
    /// <param name="style">The text style to apply.</param>
    /// <returns>Returns the text SVG element.</returns>
    public static XElement Text(string text, double x, double y, TextStyle style) =>
        new(
            SvgNs + "text",
            new XAttribute("x", x),
            new XAttribute("y", y),
            new XAttribute("style", style),
            text);

    /// <summary>
    ///     Generates a line SVG element with the specified text, location and style.
    /// </summary>
    /// <param name="startX">The x-location of the start point.</param>
    /// <param name="startY">The y-location of the start point.</param>
    /// <param name="endX">The x-location of the end point.</param>
    /// <param name="endY">The y-location of the end point.</param>
    /// <param name="style">The text style to apply.</param>
    /// <returns>Returns the text SVG element.</returns>
    public static XElement Line(double startX, double startY, double endX, double endY, Style style) =>
        new(
            SvgNs + "path",
            new XAttribute("d", $"M{startX.Num()} {startY.Num()} {endX.Num()} {endY.Num()}"),
            new XAttribute("style", style));

    /// <summary>
    ///     Creates an SVG element with a predefined marker definition and the specified width and height.
    /// </summary>
    /// <remarks>
    ///     The returned SVG element includes a 'defs' section containing a marker definition with the ID 'ArrowWideRounded'.
    ///     This marker can be referenced by child elements for drawing arrowheads or similar decorations.
    /// </remarks>
    /// <param name="width">The width, in user units, to set for the SVG viewBox.</param>
    /// <param name="height">The height, in user units, to set for the SVG viewBox.</param>
    /// <returns>
    ///     Returns an <see cref="XElement" /> representing the root svg element with the specified dimensions and marker
    ///     definition.
    /// </returns>
    public static XElement Doc(double width, double height) =>
        new(
            SvgNs + "svg",
            new XAttribute("viewBox", $"0 0 {width.Num()} {height.Num()}"),
            new XElement(
                SvgNs + "defs",
                new XAttribute("id", "defs1"),
                new XElement(
                    "marker",
                    new XAttribute("id", "ArrowWideRounded"),
                    new XAttribute("style", "overflow:visible"),
                    new XAttribute("markerHeight", 1),
                    new XAttribute("markerWidth", 2),
                    new XAttribute("orient", "auto-start-reverse"),
                    new XAttribute("preserveAspectRatio", "none"),
                    new XAttribute("refX", 1),
                    new XAttribute("viewBox", "0 0 1 1"),
                    new XElement(
                        SvgNs + "path",
                        new XAttribute("id", "path2"),
                        new XAttribute("transform", "rotate(180 .125 0)"),
                        new XAttribute("d", "m3-3-3 3 3 3"),
                        new XAttribute("style", "fill:none;stroke-linecap:round;stroke:context-stroke")))));
}