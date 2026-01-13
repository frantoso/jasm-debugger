// -----------------------------------------------------------------------
// <copyright file="Extensions.cs">
//     Created by Frank Listing at 2025/12/19.
// </copyright>
// -----------------------------------------------------------------------

namespace JasmDebugger.Client.View;

using System;
using System.Globalization;
using System.Text.Json;
using System.Xml.Linq;

public static class Extensions
{
    extension(double number)
    {
        /// <summary>
        ///     Converts a double value to a stringified number with invariant format and at maximum three decimal places.
        /// </summary>
        /// <returns>Returns the number as string.</returns>
        public string Num() => number.ToString("0.###", NumberFormatInfo.InvariantInfo);
    }

    extension(string text)
    {
        /// <summary>
        ///     Parses the text representing a single JSON value into a <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the JSON value into.</typeparam>
        /// <returns>A <typeparamref name="T" /> representation of the JSON value.</returns>
        public T? Deserialize<T>() => JsonSerializer.Deserialize<T>(text);
    }

    extension(XElement element)
    {
        /// <summary>
        ///     Sets the style attribute.
        /// </summary>
        /// <param name="style">The style to set.</param>
        public void Style(Style style)
        {
            element.Attribute("style")?.Value = $"{style}";
        }

        /// <summary>
        ///     Adds the specified <see cref="XAttribute" /> as child item.
        ///     Internally calls <see cref="XElement.Add(object?)" />. The only difference is, that this method returns the
        ///     <see cref="XElement" /> for fluent usage.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>Returns the <see cref="XElement" /> for fluent usage.</returns>
        public XElement AddNode(XAttribute item)
        {
            element.Add(item);
            return element;
        }

        /// <summary>
        ///     Adds an id attribute.
        /// </summary>
        /// <param name="id">The ID to add.</param>
        /// <returns>Returns the <see cref="XElement" /> for fluent usage.</returns>
        public XElement AddId(string id) => element.AddNode(new XAttribute("id", id));
    }

    extension<T>(T t)
    {
        /// <summary>
        ///     Makes a list from a single element.
        /// </summary>
        /// <returns>Returns a new list with the given element as member.</returns>
        public List<T> MakeList() => [t];

        /// <summary>
        ///     Functional helper: returns the result of invoking <paramref name="func" /> with <paramref name="t" />.
        ///     Useful for fluent transformations where you want to apply a function and continue with its result.
        /// </summary>
        /// <typeparam name="TResult">Type of the result value.</typeparam>
        /// <param name="func">Function to invoke.</param>
        /// <returns>The result of <paramref name="func" />(<paramref name="t" />).</returns>
        public TResult Let<TResult>(Func<T, TResult> func) => func(t);

        /// <summary>
        ///     Functional helper: executes <paramref name="action" /> with <paramref name="t" /> and returns void.
        ///     Useful for performing side effects in a fluent chain.
        /// </summary>
        /// <param name="action">Action to execute.</param>
        public void Run(Action<T> action) => action(t);
    }

    extension<T>(List<T> list)
    {
        /// <summary>
        ///     Adds the specified item to the list. Internally calls <see cref="List{T}.Add(T)" />. The only difference is,
        ///     that this method returns the list for fluent usage.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <returns>Returns the <see cref="List{T}" /> for fluent usage.</returns>
        public List<T> AddEx(T item)
        {
            list.Add(item);
            return list;
        }

        /// <summary>
        ///     Adds the specified items to the list. Internally calls <see cref="List{T}.AddRange(IEnumerable{T})" />. The only
        ///     difference is,
        ///     that this method returns the list for fluent usage.
        /// </summary>
        /// <param name="items">The items to add.</param>
        /// <returns>Returns the <see cref="List{T}" /> for fluent usage.</returns>
        public List<T> AddEx(IEnumerable<T> items)
        {
            list.AddRange(items);
            return list;
        }
    }

    extension(Node node)
    {
        /// <summary>
        ///     Combines the anchors of start and end node to all possible connections, ordered by the distance.
        /// </summary>
        /// <param name="other">The second node.</param>
        /// <param name="hasHistory">if set to <c>true</c> [has history].</param>
        /// <param name="hasDeepHistory">if set to <c>true</c> [has deep history].</param>
        /// <returns>Returns a list of connections, ordered by the distance.</returns>
        public IList<Tuple<PointWithOffset, PointWithOffset>> AllPossibleConnections(
            Node other,
            bool hasHistory,
            bool hasDeepHistory) =>
            node.AnchorsOut.Select(p => new PointWithOffset(
                    node.Location.X + p.Point.X,
                    node.Location.Y + p.Point.Y,
                    p.Offset))
                .SelectMany(
                    _ => other.AnchorsIn(hasHistory, hasDeepHistory)
                        .Select(p => new PointWithOffset(
                            other.Location.X + p.Point.X,
                            other.Location.Y + p.Point.Y,
                            p.Offset)),
                    Tuple.Create)
                .OrderBy(Node.Distance)
                .ToList();
    }

    extension(IList<Tuple<PointWithOffset, PointWithOffset>> connections)
    {
        /// <summary>
        ///     Gets all the shortest connections from the list of connections (less than <see cref="Node.MinDistance" /> compared
        ///     to the shortest connection).
        /// </summary>
        /// <returns>Returns a list with the shortest connections.</returns>
        public IList<Tuple<PointWithOffset, PointWithOffset>> ShortestConnections()
        {
            var shortestDistance = Node.Distance(connections.First());
            return connections.Where(t => Math.Abs(Node.Distance(t) - shortestDistance) < Node.MinDistance).ToList();
        }

        /// <summary>
        ///     Gets a list where at minimum one end point is outside the construction circle.
        /// </summary>
        /// <param name="midPoint">The mid-point of the construction circle.</param>
        /// <param name="radius">The radius of the construction circle.</param>
        /// <returns>
        ///     Returns a list where at minimum one end point is outside the construction circle. If the result list is empty,
        ///     it returns the first element of the source.
        /// </returns>
        public IList<Tuple<PointWithOffset, PointWithOffset>> OneEndOutSideFirstDefault(Point midPoint, double radius)
        {
            var oneEndOutside = connections.Where(t =>
                    Node.IsOutside(t.Item1.Point, midPoint, radius) || Node.IsOutside(t.Item2.Point, midPoint, radius))
                .ToList();
            return oneEndOutside.Count > 0 ? oneEndOutside : connections.First().MakeList();
        }

        /// <summary>
        ///     Gets a list where both end points are outside the construction circle.
        /// </summary>
        /// <param name="midPoint">The mid-point of the construction circle.</param>
        /// <param name="radius">The radius of the construction circle.</param>
        /// <returns>
        ///     Returns a list where both end points are outside the construction circle. If the result list is empty, it
        ///     returns the first element of the source.
        /// </returns>
        public Tuple<PointWithOffset, PointWithOffset> BothEndsOutSideFirstDefault(Point midPoint, double radius)
        {
            var bothEndsOutside = connections.Where(t =>
                Node.IsOutside(t.Item1.Point, midPoint, radius) && Node.IsOutside(t.Item2.Point, midPoint, radius));
            return bothEndsOutside.FirstOrDefault(connections.First());
        }
    }

    extension(CompositeNode node)
    {
        /// <summary>
        ///     Recursively searches this composite node for child nodes matching the predicate.
        /// </summary>
        /// <returns>Returns nodes matching the predicate and the owning diagram.</returns>
        public IEnumerable<Tuple<Diagram, Node>> Search(Func<Node, bool> predicate) =>
            node.Children.SelectMany(c => c.Search(predicate));
    }

    extension(Diagram diagram)
    {
        /// <summary>
        ///     Recursively searches the diagram for nodes matching the predicate.
        /// </summary>
        /// <returns>Returns nodes matching the predicate and the owning diagram.</returns>
        public IEnumerable<Tuple<Diagram, Node>> Search(Func<Node, bool> predicate)
        {
            // search direct children
            foreach (var n in diagram.StateNodes.Where(c => predicate(c.Value)))
            {
                yield return Tuple.Create(diagram, n.Value);
            }

            // Recursively search children
            foreach (var child in diagram.StateNodes.Values.OfType<CompositeNode>()
                         .SelectMany(c => c.Search(predicate)))
            {
                yield return child;
            }
        }
    }
}