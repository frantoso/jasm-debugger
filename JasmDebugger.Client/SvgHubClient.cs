// -----------------------------------------------------------------------
// <copyright file="SvgHubClient.cs">
//     Created by Frank Listing at 2025/12/17.
// </copyright>
// -----------------------------------------------------------------------

namespace JasmDebugger.Client;

using System.Linq;
using Microsoft.AspNetCore.SignalR.Client;
using Model;
using View;

/// <summary>
///     Receiver for SVG Hub messages. Receives FSM and state update commands.
/// </summary>
public class SvgHubClient
{
    /// <summary>
    ///     The SignalR hub connection.
    /// </summary>
    private readonly HubConnection connection;

    /// <summary>
    ///     State machines managed by this client.
    /// </summary>
    private readonly Dictionary<string, StateMachine> stateMachines = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="SvgHubClient" /> class.
    /// </summary>
    /// <param name="baseUrl">The base URL of the web server.</param>
    public SvgHubClient(string baseUrl)
    {
        var hubUrl = (baseUrl.EndsWith('/') ? baseUrl[..^1] : baseUrl) + "/fsmHub";
        Console.WriteLine(hubUrl);
        this.connection = new HubConnectionBuilder()
            .WithUrl(hubUrl)
            .WithAutomaticReconnect()
            .Build();

        this.RegisterEventHandlers();
    }

    /// <summary>
    ///     Event raised when an update-state command was received.
    ///     Allows subscribers (the web page) to react to state updates.
    /// </summary>
    public event Func<StateMachine, Task>? OnUpdateStateReceived;

    /// <summary>
    ///     Event raised when a new FSM was received.
    ///     Allows subscribers (the web page) to react to new FSMs.
    /// </summary>
    public event Func<StateMachine, Task>? OnFsmReceived;

    /// <summary>
    ///     Event raised when an FSM was removed.
    ///     Allows subscribers (the web page) to react to removed FSMs.
    /// </summary>
    public event Func<StateMachine, Task>? OnFsmRemoved;

    /// <summary>
    ///     Gets a value indicating whether this instance is disconnected.
    /// </summary>
    public bool IsDisconnected => this.connection.State == HubConnectionState.Disconnected;

    /// <summary>
    ///     Starts the hub connection.
    /// </summary>
    public async Task StartAsync()
    {
        if (this.IsDisconnected)
        {
            await this.connection.StartAsync();
        }
    }

    /// <summary>
    ///     Clears all event handlers.
    /// </summary>
    public void Clear()
    {
        this.OnFsmReceived = null;
        this.OnFsmRemoved = null;
        this.OnUpdateStateReceived = null;
    }

    /// <summary>
    ///     Sends a command via SignalR to the TCP client owning the state machine.
    /// </summary>
    /// <param name="clientId">The id of the TCP client.</param>
    /// <param name="fsm">The name of the state machine.</param>
    /// <param name="command">The command to send.</param>
    /// <param name="payload">The data to send.</param>
    public async Task SendCommandAsync(string clientId, string fsm, string command, string payload = "")
    {
        if (this.connection.State == HubConnectionState.Connected)
        {
            await this.connection.InvokeAsync(
                "SendCommand",
                new JasmCommandEnvelope(clientId, new JasmCommand(fsm, command, payload)));
        }
    }

    /// <summary>
    ///     Registers the event handlers.
    /// </summary>
    private void RegisterEventHandlers()
    {
        this.connection.On<JasmCommandEnvelope>(
            Commands.SetFsmCommand,
            async command =>
                await (this.OnFsmReceived?.Let(async onFsmReceived =>
                {
                    await onFsmReceived.Invoke(this.GetFor(command).OnSetFsm(command.JasmCommand));
                    await this.SendCommandAsync(command.ClientId, command.JasmCommand.Fsm, Commands.ReceivedFsmCommand);
                    await this.SendCommandAsync(command.ClientId, command.JasmCommand.Fsm, Commands.GetStatesCommand);
                }) ?? Task.CompletedTask)
        );
        this.connection.On<JasmCommandEnvelope>(
            Commands.UpdateStateCommand,
            async command =>
                await (this.OnUpdateStateReceived?.Invoke(this.GetFor(command).OnUpdateState(command.JasmCommand)) ??
                       Task.CompletedTask));
        this.connection.On<string>(
            Commands.RemoveClientCommand,
            async clientId => await this.OnClientRemoved(clientId));
    }

    /// <summary>
    ///     Called when a TCP client was removed. Removes all state machines associated with the client.
    /// </summary>
    /// <param name="clientId">The client identifier.</param>
    private async Task OnClientRemoved(string clientId)
    {
        var entries = this.stateMachines.Where(pair => pair.Value.ClientId == clientId).ToList();
        foreach (var entry in entries)
        {
            await (this.OnFsmRemoved?.Invoke(entry.Value) ?? Task.CompletedTask);
            this.stateMachines.Remove(entry.Key);
        }
    }

    /// <summary>
    ///     Gets or creates a state machine for the given client and FSM name.
    /// </summary>
    /// <param name="clientId">The id of the TCP client.</param>
    /// <param name="fsm">The name of the state machine.</param>
    /// <returns>
    ///     Returns the <see cref="StateMachine" /> instance associated to the <see cref="clientId" />-<see cref="fsm" />
    ///     combination.
    /// </returns>
    private StateMachine GetOrCreateStateMachine(string clientId, string fsm)
    {
        var key = StateMachine.BuildKey(clientId, fsm);
        if (this.stateMachines.TryGetValue(key, out var machine))
        {
            return machine;
        }

        var newMachine = new StateMachine(clientId, fsm);
        this.stateMachines[key] = newMachine;
        return newMachine;
    }

    /// <summary>
    ///     Gets or creates a state machine for the client ID and FSM name contained in the specified envelope.
    /// </summary>
    /// <param name="envelope">The envelope containing the information.</param>
    /// <returns>
    ///     Returns the <see cref="StateMachine" /> instance associated to the <see cref="envelope" />.
    /// </returns>
    private StateMachine GetFor(JasmCommandEnvelope envelope) =>
        this.GetOrCreateStateMachine(envelope.ClientId, envelope.JasmCommand.Fsm);
}