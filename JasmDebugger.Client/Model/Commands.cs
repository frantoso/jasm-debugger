// -----------------------------------------------------------------------
// <copyright file="Commands.cs">
//     Created by Frank Listing at 2026/01/08.
// </copyright>
// -----------------------------------------------------------------------

namespace JasmDebugger.Client.Model;

/// <summary>
///     The available commands for the FSM client-server communication.
/// </summary>
public static class Commands
{
    public const string GetStatesCommand = "get-states";
    public const string SetFsmCommand = "set-fsm";
    public const string RemoveClientCommand = "remove-client";
    public const string UpdateStateCommand = "update-state";
    public const string ReceivedFsmCommand = "received-fsm";
}