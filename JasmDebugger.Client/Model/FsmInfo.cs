// -----------------------------------------------------------------------
// <copyright file="FsmInfo.cs">
//     Created by Frank Listing at 2025/12/17.
// </copyright>
// -----------------------------------------------------------------------

namespace JasmDebugger.Client.Model;

using System.Text.Json.Serialization;

/// <summary>
///     A class representing a transition in the diagram generator.
/// </summary>
public class TransitionInfo
{
    /// <summary>
    ///     Gets the end point identifier.
    /// </summary>
    public string EndPointId { get; set; } = "";

    /// <summary>
    ///     Gets a value indicating whether this transition ends in a history state.
    /// </summary>
    public bool IsHistory { get; set; }

    /// <summary>
    ///     Gets a value indicating whether this transition ends in a deep history state.
    /// </summary>
    public bool IsDeepHistory { get; set; }

    /// <summary>
    ///     Gets a value indicating whether this transition ends in the final state.
    /// </summary>
    public bool IsToFinal { get; set; }
}

/// <summary>
///     A class representing a state in the diagram generator.
/// </summary>
public class StateInfo
{
    /// <summary>
    ///     Gets the name of the state.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    ///     Gets the identifier of the state.
    /// </summary>
    public string Id { get; set; } = "";

    /// <summary>
    ///     Gets the transitions.
    /// </summary>
    public IList<TransitionInfo> Transitions { get; set; } = [];

    /// <summary>
    ///     Gets the children.
    /// </summary>
    public IList<FsmInfo> Children { get; set; } = [];

    /// <summary>
    ///     Gets a value indicating whether this instance is the initial state.
    /// </summary>
    public bool IsInitial { get; set; }

    /// <summary>
    ///     Gets a value indicating whether this instance is the final state.
    /// </summary>
    public bool IsFinal { get; set; }

    /// <summary>
    ///     Gets a value indicating whether this instance has a history end point.
    /// </summary>
    public bool HasHistory { get; set; }

    /// <summary>
    ///     Gets a value indicating whether this instance has a deep history end point.
    /// </summary>
    public bool HasDeepHistory { get; set; }

    /// <summary>
    ///     Gets a value indicating whether this instance has children.
    /// </summary>
    [JsonIgnore]
    public bool HasChildren => this.Children.Count > 0;

    /// <summary>
    ///     Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString() => $"{this.Name}";
}

/// <summary>
///     A class representing a finite state machine in the diagram generator.
/// </summary>
public class FsmInfo
{
    /// <summary>
    ///     Gets the name of the state machine.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    ///     Gets the states contained in this machine.
    /// </summary>
    public IList<StateInfo> States { get; set; } = [];

    /// <summary>
    ///     Gets the normal states (<see cref="States" /> without initial and without final states).
    /// </summary>
    [JsonIgnore]
    public IList<StateInfo> NormalStates =>
        this.States.Where(info => info is { IsInitial: false, IsFinal: false }).ToList();

    /// <summary>
    ///     Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString() => $"{this.Name}";
}