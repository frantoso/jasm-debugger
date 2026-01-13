// -----------------------------------------------------------------------
// <copyright file="SvgHub.cs">
//     Created by Frank Listing at 2026/01/08.
// </copyright>
// -----------------------------------------------------------------------

namespace JasmDebugger.Service;

using Client.Model;
using JetBrains.Annotations;
using Microsoft.AspNetCore.SignalR;
using Model;

/// <summary>
///     Dispatcher for SVG updates and commands.
/// </summary>
/// <param name="tcpServer">The TCP server to send messages back to the debug-clients.</param>
public class SvgHub(TcpClientContainer tcpServer) : Hub
{
    /// <summary>
    ///     Gets the TCP server service used to accept and manage outgoing TCP client connections.
    /// </summary>
    public TcpClientContainer TcpServer { get; } = tcpServer;

    /// <summary>
    ///     Sends an update message containing the specified JSON data to all connected WEB-clients.
    /// </summary>
    /// <param name="jsonData">The JSON-formatted data to broadcast to all clients. Cannot be null.</param>
    /// <returns>A task that represents the asynchronous broadcast operation.</returns>
    [UsedImplicitly]
    public async Task BroadcastUpdate(string jsonData) =>
        await this.Clients.All.SendAsync("UpdateSvg", jsonData);

    /// <summary>
    ///     Sends the specified command envelope to a TCP client. The client is identified by the ClientId property of the
    ///     command envelope.
    /// </summary>
    /// <param name="envelope">The envelope containing the command and client identifier to send. Cannot be null.</param>
    /// <returns>A task that represents the asynchronous send operation.</returns>
    [UsedImplicitly]
    public async Task SendCommand(JasmCommandEnvelope envelope) => await this.TcpServer.SendAsync(envelope);
}