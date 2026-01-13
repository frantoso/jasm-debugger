// -----------------------------------------------------------------------
// <copyright file="JasmCommand.cs">
//     Created by Frank Listing at 2025/12/17.
// </copyright>
// -----------------------------------------------------------------------

namespace JasmDebugger.Client.Model;

/// <summary>
///     A class representing a command to display information about a state machine.
/// </summary>
public class JasmCommand(string fsm, string command, string payload)
{
    /// <summary>
    ///     Gets or sets the FSM the command belongs to.
    /// </summary>
    public string Fsm { get; set; } = fsm;

    /// <summary>
    ///     Gets or sets the command.
    /// </summary>
    public string Command { get; set; } = command;

    /// <summary>
    ///     Gets or sets the data related to the command.
    /// </summary>
    public string Payload { get; set; } = payload;
}

/// <summary>
///     Associates a JasmCommand with a client ID.
/// </summary>
public class JasmCommandEnvelope(string clientId, JasmCommand jasmCommand)
{
    /// <summary>
    ///     Gets or sets the client identifier.
    /// </summary>
    public string ClientId { get; set; } = clientId;

    /// <summary>
    ///     Gets or sets the jasm command.
    /// </summary>
    public JasmCommand JasmCommand { get; set; } = jasmCommand;
}