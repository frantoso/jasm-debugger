// -----------------------------------------------------------------------
// <copyright file="Node.cs">
//     Created by Frank Listing at 2025/12/18.
// </copyright>
// -----------------------------------------------------------------------

namespace JasmDebugger.Client.View;

using System.Xml.Linq;

/// <summary>
///     Base class for all nodes in the diagram.
/// </summary>
public abstract class Node(string id, string name, Point location)
{
    /// <summary>
    ///     The minimum distance to find the shortest connections.
    /// </summary>
    public const double MinDistance = 4.0;

    /// <summary>
    ///     Gets the identifier of this node.
    /// </summary>
    public string Id { get; } = id;

    /// <summary>
    ///     Gets the name of this node.
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    ///     Gets the location of this node.
    /// </summary>
    public Point Location { get; } = location;

    /// <summary>
    ///     Gets the anchor points for outgoing connectors.
    /// </summary>
    public virtual IReadOnlyList<PointWithOffset> AnchorsOut { get; } = [new(0, 0)];

    /// <summary>
    ///     Gets the XML element which describes this node.
    /// </summary>
    public XElement Element { get; } = Svg.Group($"{id}-group");

    /// <summary>
    ///     Calculates the distance between two points.
    /// </summary>
    /// <param name="points">A tuple containing start and end point.</param>
    /// <returns>Returns the distance between the two points.</returns>
    public static double Distance(Tuple<PointWithOffset, PointWithOffset> points) =>
        Distance(points.Item1.Point, points.Item2.Point);

    /// <summary>
    ///     Calculates the distance between two points.
    /// </summary>
    /// <param name="p1">The start point.</param>
    /// <param name="p2">The end point.</param>
    /// <returns>Returns the distance between the two points.</returns>
    public static double Distance(Point p1, Point p2) => Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2));

    /// <summary>
    ///     Determines whether the specified point is outside the construction circle.
    /// </summary>
    /// <param name="point">The point.</param>
    /// <param name="midPoint">The mid-point of the circle.</param>
    /// <param name="radius">The radius of the circle.</param>
    /// <returns>
    ///     Returns <c>true</c> if the specified point is outside; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsOutside(Point point, Point midPoint, double radius) =>
        Math.Sqrt(Math.Pow(midPoint.X - point.X, 2) + Math.Pow(midPoint.Y - point.Y, 2)) >= radius;

    /// <summary>
    ///     Gets the anchor points for incoming connectors.
    /// </summary>
    /// <param name="hasHistory">A value indicating whether this node contains a history end point.</param>
    /// <param name="hasDeepHistory">A value indicating whether this node contains a deep history end point.</param>
    /// <returns>Returns the anchor points for incoming connectors.</returns>
    public virtual IReadOnlyList<PointWithOffset> AnchorsIn(bool hasHistory, bool hasDeepHistory) => [new(0, 0)];

    /// <summary>
    ///     Calculates the anchors for one connection.
    /// </summary>
    /// <param name="end">The end node.</param>
    /// <param name="hasHistory">A value indicating whether this node contains a history end point.</param>
    /// <param name="hasDeepHistory">A value indicating whether this node contains a deep history end point.</param>
    /// <param name="midPoint">The mid-point.</param>
    /// <param name="radius">The radius.</param>
    /// <returns>Returns the anchors for one connection.</returns>
    public Tuple<PointWithOffset, PointWithOffset> GetAnchors(
        Node end,
        bool hasHistory,
        bool hasDeepHistory,
        Point midPoint,
        double radius) =>
        this.AllPossibleConnections(end, hasHistory, hasDeepHistory).ShortestConnections()
            .OneEndOutSideFirstDefault(midPoint, radius).BothEndsOutSideFirstDefault(midPoint, radius);

    /// <summary>
    ///     When overridden, highlights this node.
    /// </summary>
    public abstract void Highlight();

    /// <summary>
    ///     When overridden, resets this node to normal appearance.
    /// </summary>
    public abstract void Reset();
}

/// <summary>
///     Defines a normal state node.
/// </summary>
/// <seealso cref="Node" />
public class StateNode : Node
{
    /// <summary>
    ///     The standard anchor points for incoming connectors.
    /// </summary>
    private static readonly IReadOnlyList<PointWithOffset> NormalAnchorsIn =
    [
        new(0, -4, 1),
        new(10, 0, 0, 1),
        new(0, 4, -1),
        new(-10, 0, 0, -1),
    ];

    /// <summary>
    ///     Defines a normal state node.
    /// </summary>
    /// <seealso cref="Node" />
    public StateNode(string id, string name, Point location) : base(id, name, location)
    {
        this.MainElement = Svg.Rect(this.Id, this.Location, Styles.State);
        this.Element.Add(
            this.MainElement,
            Svg.CenteredText(this.Name, this.Location, Styles.NormalText));
    }

    /// <summary>
    ///     Gets the anchor points for outgoing connectors.
    /// </summary>
    public override IReadOnlyList<PointWithOffset> AnchorsOut { get; } =
    [
        new(0, -4, -1),
        new(10, 0, 0, -1),
        new(0, 4, 1),
        new(-10, 0, 0, 1),
    ];

    /// <summary>
    ///     Gets the main SVG element used to highlight the node.
    /// </summary>
    private XElement MainElement { get; }

    /// <summary>
    ///     Gets the anchor points for incoming connectors.
    /// </summary>
    /// <param name="hasHistory">A value indicating whether this node contains a history end point.</param>
    /// <param name="hasDeepHistory">A value indicating whether this node contains a deep history end point.</param>
    /// <returns>
    ///     Returns the anchor points for incoming connectors.
    /// </returns>
    public override IReadOnlyList<PointWithOffset> AnchorsIn(bool hasHistory, bool hasDeepHistory) =>
        NormalAnchorsIn;

    /// <summary>
    ///     Highlights the main element of this node.
    /// </summary>
    public override void Highlight() => this.MainElement.Style(Styles.StateHighlighted);

    /// <summary>
    ///     Resets the main element of this node to normal appearance.
    /// </summary>
    public override void Reset() => this.MainElement.Style(Styles.State);
}

/// <summary>
///     Defines a composite state node.
/// </summary>
/// <seealso cref="StateNode" />
public class CompositeNode : StateNode
{
    /// <summary>
    ///     The anchor points for incoming history connectors (left bubble).
    /// </summary>
    protected static readonly IReadOnlyList<PointWithOffset> AnchorsInHLeft = [new(-6, 6)];

    /// <summary>
    ///     Defines a composite state node.
    /// </summary>
    /// <seealso cref="StateNode" />
    public CompositeNode(string id, string name, Point location, IList<Diagram> children) : base(id, name, location)
    {
        this.Children = children;

        this.Element.Add(
            Svg.Circle(this.Location.X + 5, this.Location.Y + 2.8, .5, Styles.SmallCircle),
            Svg.Circle(this.Location.X + 7.5, this.Location.Y + 2.8, .5, Styles.SmallCircle),
            Svg.Line(
                this.Location.X + 5.5,
                this.Location.Y + 2.8,
                this.Location.X + 7,
                this.Location.Y + 2.8,
                Styles.Line));
    }

    /// <summary>
    ///     Gets the diagrams of the child machines.
    /// </summary>
    public IList<Diagram> Children { get; }
}

/// <summary>
///     Defines a state node with normal history only.
/// </summary>
/// <seealso cref="CompositeNode" />
public class HistoryNode : CompositeNode
{
    /// <summary>
    ///     Defines a state node with normal history only.
    /// </summary>
    /// <seealso cref="CompositeNode" />
    public HistoryNode(string id, string name, Point location, IList<Diagram> children) : base(
        id,
        name,
        location,
        children)
    {
        this.Element.Add(
            Svg.Circle(this.Location.X - 6, this.Location.Y + 4, 2, Styles.History),
            Svg.CenteredText("H", this.Location.X - 6, this.Location.Y + 4.4, Styles.BigText));
    }

    /// <summary>
    ///     Gets the anchor points for incoming connectors.
    /// </summary>
    /// <param name="hasHistory">A value indicating whether this node contains a history end point.</param>
    /// <param name="hasDeepHistory">A value indicating whether this node contains a deep history end point.</param>
    /// <returns>
    ///     Returns the anchor points for incoming connectors.
    /// </returns>
    public override IReadOnlyList<PointWithOffset> AnchorsIn(bool hasHistory, bool hasDeepHistory) =>
        hasHistory ? AnchorsInHLeft : base.AnchorsIn(true, false);
}

/// <summary>
///     Defines a state node with deep history only.
/// </summary>
/// <seealso cref="CompositeNode" />
public class DeepHistoryNode : CompositeNode
{
    /// <summary>
    ///     Defines a state node with deep history only.
    /// </summary>
    /// <seealso cref="CompositeNode" />
    public DeepHistoryNode(string id, string name, Point location, IList<Diagram> children) : base(
        id,
        name,
        location,
        children)
    {
        this.Element.Add(
            Svg.Circle(this.Location.X - 6, this.Location.Y + 4, 2, Styles.History),
            Svg.CenteredText("Hd", this.Location.X - 6, this.Location.Y + 4.4, Styles.BigTextCondensed));
    }

    /// <summary>
    ///     Gets the anchor points for incoming connectors.
    /// </summary>
    /// <param name="hasHistory">A value indicating whether this node contains a history end point.</param>
    /// <param name="hasDeepHistory">A value indicating whether this node contains a deep history end point.</param>
    /// <returns>
    ///     Returns the anchor points for incoming connectors.
    /// </returns>
    public override IReadOnlyList<PointWithOffset> AnchorsIn(bool hasHistory, bool hasDeepHistory) =>
        hasDeepHistory ? AnchorsInHLeft : base.AnchorsIn(false, true);
}

/// <summary>
///     Defines a state node with normal and deep history.
/// </summary>
/// <seealso cref="HistoryNode" />
public class HistoryDeepHistoryNode : HistoryNode
{
    /// <summary>
    ///     The anchor points for incoming history connectors (right bubble - only if normal and deep history is present).
    /// </summary>
    private static readonly IReadOnlyList<PointWithOffset> AnchorsInHRight = [new(-1, 6)];

    /// <summary>
    ///     The special anchor points for incoming connectors which do not end at a history bubble.
    /// </summary>
    private static readonly IReadOnlyList<PointWithOffset> AnchorsInNoH =
    [
        new(0, -4, 1),
        new(10, 0, 0, 1),
        new(0, 4, 3),
        new(-10, 0, 0, -1),
    ];

    /// <summary>
    ///     Defines a state node with normal and deep history.
    /// </summary>
    /// <seealso cref="HistoryNode" />
    public HistoryDeepHistoryNode(string id, string name, Point location, IList<Diagram> children) : base(
        id,
        name,
        location,
        children)
    {
        this.Element.Add(
            Svg.Circle(this.Location.X - 1, this.Location.Y + 4, 2, Styles.History),
            Svg.CenteredText("Hd", this.Location.X - 1, this.Location.Y + 4.4, Styles.BigTextCondensed));
    }

    /// <summary>
    ///     Gets the anchor points for outgoing connectors.
    /// </summary>
    public override IReadOnlyList<PointWithOffset> AnchorsOut { get; } =
    [
        new(0, -4, -1),
        new(10, 0, 0, -1),
        new(0, 4, 5),
        new(-10, 0, 0, 1),
    ];

    /// <summary>
    ///     Gets the anchor points for incoming connectors.
    /// </summary>
    /// <param name="hasHistory">A value indicating whether this node contains a history end point.</param>
    /// <param name="hasDeepHistory">A value indicating whether this node contains a deep history end point.</param>
    /// <returns>
    ///     Returns the anchor points for incoming connectors.
    /// </returns>
    public override IReadOnlyList<PointWithOffset> AnchorsIn(bool hasHistory, bool hasDeepHistory) =>
        hasHistory ? AnchorsInHLeft :
        hasDeepHistory ? AnchorsInHRight : AnchorsInNoH;
}

/// <summary>
///     Base class for special nodes (initial and final).
/// </summary>
/// <seealso cref="Node" />
public abstract class SpecialNode : Node
{
    /// <summary>
    ///     The standard anchor points for incoming connectors.
    /// </summary>
    private static readonly IReadOnlyList<PointWithOffset> NormalAnchorsIn =
    [
        new(0, -2),
        new(2, 0),
        new(0, 2),
        new(-2, 0),
    ];

    private readonly Style highlightedStyle;
    private readonly Style normalStyle;

    /// <summary>
    ///     Base class for special nodes (initial and final).
    /// </summary>
    /// <seealso cref="Node" />
    protected SpecialNode(
        string id,
        Point location,
        XElement mainElement,
        Style normalStyle,
        Style highlightedStyle) : base(id, "", location)
    {
        this.normalStyle = normalStyle;
        this.highlightedStyle = highlightedStyle;
        this.MainElement = mainElement;
        this.Element.Add(this.MainElement);
    }

    /// <summary>
    ///     Gets the anchor points for outgoing connectors.
    /// </summary>
    public override IReadOnlyList<PointWithOffset> AnchorsOut { get; } =
    [
        new(0, -2),
        new(2, 0),
        new(0, 2),
        new(-2, 0),
    ];

    /// <summary>
    ///     Gets the main SVG element used to highlight the node.
    /// </summary>
    private XElement MainElement { get; }

    /// <summary>
    ///     Gets the anchor points for incoming connectors.
    /// </summary>
    /// <param name="hasHistory">A value indicating whether this node contains a history end point.</param>
    /// <param name="hasDeepHistory">A value indicating whether this node contains a deep history end point.</param>
    /// <returns>
    ///     Returns the anchor points for incoming connectors.
    /// </returns>
    public override IReadOnlyList<PointWithOffset> AnchorsIn(bool hasHistory, bool hasDeepHistory) =>
        NormalAnchorsIn;

    /// <summary>
    ///     Highlights the main element of this node.
    /// </summary>
    public override void Highlight() => this.MainElement.Style(this.highlightedStyle);

    /// <summary>
    ///     Resets the main element of this node to normal appearance.
    /// </summary>
    public override void Reset() => this.MainElement.Style(this.normalStyle);
}

/// <summary>
///     Defines an initial state node.
/// </summary>
/// <seealso cref="SpecialNode" />
public class InitialNode : SpecialNode
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="InitialNode" /> class.
    /// </summary>
    /// <seealso cref="SpecialNode" />
    public InitialNode(string id, Point location) : base(
        id,
        location,
        Svg.Circle(id, location, 2, Styles.StateInitial),
        Styles.StateInitial,
        Styles.StateInitialHighlighted)
    {
    }
}

/// <summary>
///     Defines a final state node.
/// </summary>
/// <seealso cref="SpecialNode" />
public class FinalNode : SpecialNode
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="FinalNode" /> class.
    /// </summary>
    /// <seealso cref="SpecialNode" />
    public FinalNode(string id, Point location) : base(
        id,
        location,
        Svg.Circle(id, location, 2, Styles.StateFinal),
        Styles.StateFinal,
        Styles.StateFinalHighlighted)
    {
        this.Element.Add(Svg.Circle(this.Location, 1.3, Styles.StateInitial));
    }
}