// -----------------------------------------------------------------------
// <copyright file="StateChangedInfo.cs">
//     Created by Frank Listing at 2025/12/17.
// </copyright>
// -----------------------------------------------------------------------

namespace JasmDebugger.Client.Model;

public class StateChangedInfo
{
    /// <summary>
    ///     Gets or sets the name of the FSM.
    /// </summary>
    public string Fsm { get; set; } = "";

    /// <summary>
    ///     Gets or sets the name of the state before the state change.
    /// </summary>
    public string OldStateName { get; set; } = "";

    /// <summary>
    ///     Gets or sets the id of the state before the state change.
    /// </summary>
    public string OldStateId { get; set; } = "";

    /// <summary>
    ///     Gets or sets the name of the state after the state change (the current state).
    /// </summary>
    public string NewStateName { get; set; } = "";

    /// <summary>
    ///     Gets or sets the id of the state after the state change (the current state).
    /// </summary>
    public string NewStateId { get; set; } = "";
}