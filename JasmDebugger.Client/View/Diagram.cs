// -----------------------------------------------------------------------
// <copyright file="Diagram.cs">
//     Created by Frank Listing at 2025/12/18.
// </copyright>
// -----------------------------------------------------------------------

namespace JasmDebugger.Client.View;

using System.Xml.Linq;
using Model;

/// <summary>
///     Generates SVG diagrams from finite state machines.
/// </summary>
public class Diagram
{
    /// <summary>
    ///     The space around the active area (partly occupied by nodes).
    /// </summary>
    private const double Border = 20.0;

    /// <summary>
    ///     The space between nodes.
    /// </summary>
    private const double StateSpace = 16.0;

    /// <summary>
    ///     The offset used to place the special nodes (initial and final).
    /// </summary>
    private const double SpecialNodeOffset = 8.0;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Diagram" /> class.
    /// </summary>
    /// <param name="fsm">The FSM to draw.</param>
    public Diagram(FsmInfo fsm)
    {
        this.Fsm = fsm;
        this.Radius = Math.Max(this.Fsm.NormalStates.Count - 1, 0) / 2.0 * Diagram.StateSpace;
        this.MidPoint = new Point(this.Radius + Diagram.Border, this.Radius + Diagram.Border);
        this.Width = 2 * this.Radius + 2 * Diagram.Border;
        this.Height = 2 * this.Radius + 2 * Diagram.Border;
        this.BaseDiagram = this.CalculateBaseDiagram();
        this.TotalWidth = this.CalculateWidth();
        this.TotalHeight = this.CalculateHeight();
    }

    /// <summary>
    ///     Gets the state machine information.
    /// </summary>
    public FsmInfo Fsm { get; }

    /// <summary>
    ///     Gets the width of the diagram.
    /// </summary>
    public double Width { get; }

    /// <summary>
    ///     Gets the height of the diagram.
    /// </summary>
    public double Height { get; }

    /// <summary>
    ///     Gets the base diagram without nested ones.
    /// </summary>
    public XElement BaseDiagram { get; }

    /// <summary>
    ///     Gets the total width of the diagram (including nested diagrams).
    /// </summary>
    public double TotalWidth { get; }

    /// <summary>
    ///     Gets the total height of the diagram (including nested diagrams).
    /// </summary>
    public double TotalHeight { get; }

    /// <summary>
    ///     Gets the radius of the construction helper.
    /// </summary>
    internal double Radius { get; }

    /// <summary>
    ///     Gets the middle point of the construction helper.
    /// </summary>
    internal Point MidPoint { get; }

    /// <summary>
    ///     Gets a dictionary associating the state information to the graphical nodes.
    /// </summary>
    internal Dictionary<StateInfo, Node> StateNodes { get; } = [];

    /// <summary>
    ///     Calculates the whole SVG document for this FSM and all nested FSM's including the SVG tag and global definitions.
    /// </summary>
    /// <returns>Returns the SVG document of the diagram.</returns>
    public XElement CalculateSvgDocument() => this.CalculateSvgGraph(Svg.Doc(this.TotalWidth, this.TotalHeight));

    /// <summary>
    ///     Calculates the SVG document for this FSM without nested FSM's excluding the SVG tag and global definitions.
    /// </summary>
    /// <returns>Returns the SVG node of the diagram.</returns>
    public XElement CalculateBaseDiagram()
    {
        var group = Svg.Group();
        group.Add(this.CalculateStates(), this.CalculateTransitions());

        return group;
    }

    /// <summary>
    ///     Calculates the SVG document for this FSM and all nested FSM's excluding the SVG tag and global definitions.
    /// </summary>
    /// <param name="parent">The parent node to add new ones to.</param>
    /// <returns>Returns the parent node to allow chaining.</returns>
    public XElement CalculateSvgGraph(XElement parent)
    {
        parent.Add(this.BaseDiagram, this.CalculateNested());
        return parent;
    }

    /// <summary>
    ///     Calculates and draws the state nodes.
    /// </summary>
    /// <returns>Returns the SVG of the nodes.</returns>
    internal List<XElement> CalculateStates() => this.RegisterInitialNode().Concat(this.RegisterNodes())
        .Concat(this.RegisterFinalNode()).ToList();

    /// <summary>
    ///     Calculates and draws the normal state nodes (excluding initial and final).
    /// </summary>
    /// <returns>Returns the SVG of the normal nodes.</returns>
    internal List<XElement> RegisterNodes()
    {
        var angle = 2 * Math.PI / this.Fsm.NormalStates.Count;
        return this.Fsm.NormalStates.Aggregate(
            Tuple.Create(new List<XElement>(), 0.0),
            (data, info) => Tuple.Create(
                data.Item1.AddEx(this.RegisterNode(info, this.CalculateNodePosition(data.Item2))),
                data.Item2 + angle)).Item1;
    }

    /// <summary>
    ///     Calculates the transitions of all states.
    /// </summary>
    /// <returns>Returns the SVG of the transitions.</returns>
    internal List<XElement> CalculateTransitions() =>
        this.StateNodes.ToList().Aggregate(
            new List<XElement>(),
            (list, pair) => list.AddEx(this.CalculateTransitions(pair)));

    /// <summary>
    ///     Creates child diagrams associated to the provided state.
    /// </summary>
    /// <param name="diagramData">The parameters.</param>
    /// <param name="node">The state to check for child diagrams.</param>
    /// <param name="top">The coordinate of the upper border.</param>
    /// <returns>Return the result of the process.</returns>
    private static DiagramData CreateChildDiagram(
        DiagramData diagramData,
        CompositeNode node,
        double top)
    {
        var result = node.Children.Aggregate(
            new DiagramData(diagramData.Left),
            (data, fsm) => Diagram.CreateChildDiagramOfGroup(fsm, data, top));

        List<XElement> background =
        [
            Svg.Rect(
                diagramData.Left + 2,
                top + 2,
                result.Left - diagramData.Left - 4,
                result.Height - 4,
                5,
                Styles.Background),
            Svg.Text(node.Name, diagramData.Left + 18, top + 6, Styles.BigText)
        ];

        return new DiagramData(
            diagramData.Elements.Concat(background.Concat(result.Elements)).ToList(),
            result.Left,
            Math.Max(diagramData.Height, result.Height));
    }

    /// <summary>
    ///     Creates a child diagram of a diagram-group associated to one state.
    /// </summary>
    /// <param name="diagram">The state machine to create the diagram for.</param>
    /// <param name="data">The parameters.</param>
    /// <param name="top">The coordinate of the upper border.</param>
    /// <returns>Return the result of the process.</returns>
    private static DiagramData CreateChildDiagramOfGroup(Diagram diagram, DiagramData data, double top) =>
        data.Combine(
            diagram.CalculateSvgGraph(Svg.Group("fsm", data.Left, top)),
            diagram.TotalWidth,
            diagram.TotalHeight);

    /// <summary>
    ///     Creates and registers a node in the <see cref="StateNodes" /> dictionary.
    /// </summary>
    /// <param name="state">The state to draw and register.</param>
    /// <param name="point">The point to draw the state.</param>
    /// <returns>Returns the SVG to draw the node.</returns>
    private XElement RegisterNode(StateInfo state, Point point)
    {
        var node = state.CreateNode(point);
        this.StateNodes.Add(state, node);

        return node.Element;
    }

    /// <summary>
    ///     Creates and registers the initial node in the <see cref="StateNodes" /> dictionary.
    /// </summary>
    /// <returns>Returns the SVG to draw the node.</returns>
    private List<XElement> RegisterInitialNode()
    {
        var state = this.Fsm.States.First(info => info.IsInitial);
        return this.RegisterNode(state, new Point(Diagram.SpecialNodeOffset, Diagram.SpecialNodeOffset)).MakeList();
    }

    /// <summary>
    ///     Creates and registers the final node in the <see cref="StateNodes" /> dictionary.
    /// </summary>
    /// <returns>Returns the SVG to draw the node.</returns>
    private List<XElement> RegisterFinalNode()
    {
        var state = this.Fsm.States.FirstOrDefault(info => info.IsFinal);
        if (state == null)
        {
            return [];
        }

        var stateInfo = this.Fsm.NormalStates.First(s => s.Transitions.Any(t => t.IsToFinal));
        var node = this.StateNodes[stateInfo];
        var x = node.Location.X < this.MidPoint.X
            ? Diagram.SpecialNodeOffset
            : this.MidPoint.X + this.Radius + Diagram.Border - Diagram.SpecialNodeOffset;
        return this.RegisterNode(
            state,
            new Point(x, this.MidPoint.Y + this.Radius + Diagram.Border - Diagram.SpecialNodeOffset)).MakeList();
    }

    /// <summary>
    ///     Calculates the node position.
    /// </summary>
    /// <param name="angle">The angle to place the node at the construction helper.</param>
    /// <returns>Returns the point where to place the node.</returns>
    private Point CalculateNodePosition(double angle) => new(
        this.MidPoint.X + this.Radius * Math.Sin(angle),
        this.MidPoint.Y - this.Radius * Math.Cos(angle));

    /// <summary>
    ///     Calculates the transitions for the specified state.
    /// </summary>
    /// <param name="pair">The state and it's node.</param>
    /// <returns>Returns the SVG of the transitions.</returns>
    private List<XElement> CalculateTransitions(KeyValuePair<StateInfo, Node> pair) =>
        pair.Key.Transitions.ToList().Aggregate(
            new List<XElement>(),
            (list, transition) =>
            {
                this.EndNode(transition)?.Run(node => list.Add(this.CreateTransition(pair.Value, transition, node)));
                return list;
            });

    /// <summary>
    ///     Looks whether the end-node of the transition is in the known states.
    /// </summary>
    /// <param name="transition">The information about the end point of the transition.</param>
    /// <returns>Returns the node, if it was found; null otherwise.</returns>
    private Node? EndNode(TransitionInfo transition) =>
        this.StateNodes.FirstOrDefault(pair => object.Equals(pair.Key.Id, transition.EndPointId)).Value;

    /// <summary>
    ///     Calculates a transition.
    /// </summary>
    /// <param name="startNode">The node where the connector starts from.</param>
    /// <param name="transition">The information about the end point of the transition.</param>
    /// <param name="endNode">The node where the connector ends.</param>
    /// <returns>Returns the SVG of the transition.</returns>
    private XElement CreateTransition(Node startNode, TransitionInfo transition, Node endNode)
    {
        var (start, end) = startNode.GetAnchors(
            endNode,
            transition.IsHistory,
            transition.IsDeepHistory,
            this.MidPoint,
            this.Radius);
        return Svg.Line(start.X, start.Y, end.X, end.Y, Styles.Transition);
    }

    /// <summary>
    ///     Calculates the nested state machines.
    /// </summary>
    /// <returns>Returns the nested state machines.</returns>
    private List<XElement> CalculateNested()
    {
        var (elements, _, _) = this.StateNodes.Values.OfType<CompositeNode>().Aggregate(
            new DiagramData(),
            (data, node) => Diagram.CreateChildDiagram(data, node, this.Height));

        return elements;
    }

    /// <summary>
    ///     Calculates the height of the whole diagram drawing including the children.
    /// </summary>
    /// <returns>Returns the height of the whole diagram drawing including the children.</returns>
    private double CalculateHeight() =>
        this.Height + this.StateNodes.Values.OfType<CompositeNode>()
            .SelectMany(node => node.Children)
            .Aggregate(0.0, (maxHeight, diagram) => Math.Max(maxHeight, diagram.TotalHeight));

    /// <summary>
    ///     Calculates the width of the whole diagram drawing including the children.
    /// </summary>
    /// <returns>Returns the width of the whole diagram drawing including the children.</returns>
    private double CalculateWidth() =>
        Math.Max(
            this.Width,
            this.StateNodes.Values.OfType<CompositeNode>()
                .SelectMany(node => node.Children)
                .Aggregate(0.0, (width, diagram) => width + diagram.TotalWidth));

    /// <summary>
    ///     Helper record to aggregate diagram data.
    /// </summary>
    internal record DiagramData(List<XElement> Elements, double Left = 0.0, double Height = 0.0)
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DiagramData" /> class.
        /// </summary>
        /// <param name="left">The point marking the left border.</param>
        /// <param name="height">The height of the diagram.</param>
        public DiagramData(double left = 0.0, double height = 0.0) : this([], left, height)
        {
        }

        /// <summary>
        ///     Combines the specified diagram data with this instance.
        /// </summary>
        public DiagramData Combine(XElement element, double left, double height) =>
            new(this.Elements.Concat(element.MakeList()).ToList(), this.Left + left, Math.Max(this.Height, height));
    }
}