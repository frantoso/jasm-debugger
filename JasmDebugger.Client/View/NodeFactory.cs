namespace JasmDebugger.Client.View;

using Model;

/// <summary>
///     Factory class to create nodes.
/// </summary>
public static class NodeFactory
{
    /// <summary>
    ///     A list containing associations between state info predicates and node creators.
    /// </summary>
    private static readonly List<Tuple<Func<StateInfo, bool>, Func<StateInfo, Point, Node>>> Creators =
    [
        new(state => state.IsInitial, (state, location) => new InitialNode(state.Id, location)),
        new(state => state.IsFinal, (state, location) => new FinalNode(state.Id, location)),
        new(
            state => state is { HasChildren: true, HasHistory: true, HasDeepHistory: true },
            (state, location) => new HistoryDeepHistoryNode(state.Id, state.Name, location, state.ChildDiagrams)),
        new(
            state => state is { HasChildren: true, HasHistory: true, HasDeepHistory: false },
            (state, location) => new HistoryNode(state.Id, state.Name, location, state.ChildDiagrams)),
        new(
            state => state is { HasChildren: true, HasHistory: false, HasDeepHistory: true },
            (state, location) => new DeepHistoryNode(state.Id, state.Name, location, state.ChildDiagrams)),
        new(
            state => state is { HasChildren: true, HasHistory: false, HasDeepHistory: false },
            (state, location) => new CompositeNode(state.Id, state.Name, location, state.ChildDiagrams)),
        new(
            state => state is { HasChildren: false, HasHistory: false, HasDeepHistory: false },
            (state, location) => new StateNode(state.Id, state.Name, location))
    ];

    /// <summary>
    ///     Extension methods for <see cref="StateInfo" />.
    /// </summary>
    extension(StateInfo info)
    {
        /// <summary>
        ///     Gets the child diagrams.
        /// </summary>
        public IList<Diagram> ChildDiagrams => info.Children.Aggregate(
            new List<Diagram>(),
            (data, fsm) => data.Concat([new Diagram(fsm)]).ToList());

        /// <summary>
        ///     Creates a new <see cref="Node" /> from the specified information.
        /// </summary>
        /// <param name="location">The point where to place the node.</param>
        /// <returns>Returns the new node.</returns>
        public Node CreateNode(Point location) =>
            Creators.FirstOrDefault(tuple => tuple.Item1(info))?.Item2(info, location) ??
            new StateNode(info.Id, info.Name, location);
    }
}