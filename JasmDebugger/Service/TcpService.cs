// -----------------------------------------------------------------------
// <copyright file="TcpService.cs">
//     Created by Frank Listing at 2026/01/08.
// </copyright>
// -----------------------------------------------------------------------

namespace JasmDebugger.Service;

using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Client.Model;
using Client.View;
using Microsoft.AspNetCore.SignalR;
using Model;

/// <summary>
///     A TCP background service that connects the apps containing state machines with the web pages.
/// </summary>
/// <seealso cref="Microsoft.Extensions.Hosting.BackgroundService" />
public class TcpBackgroundService(IHubContext<SvgHub> hubContext, TcpClientContainer tcpClients) : BackgroundService
{
    /// <summary>
    ///     Static counter to generate client IDs
    /// </summary>
    private static int clientCounter;

    /// <summary>
    ///     Gets the connected TCP clients.
    /// </summary>
    public TcpClientContainer TcpClients { get; } = tcpClients;

    /// <summary>
    ///     This method is called when the <see cref="T:Microsoft.Extensions.Hosting.IHostedService" /> starts. The
    ///     implementation returns a task that represents
    ///     the lifetime of the long-running operation(s) being performed.
    /// </summary>
    /// <param name="cancellationToken">
    ///     Triggered when
    ///     <see cref="M:Microsoft.Extensions.Hosting.IHostedService.StopAsync(System.Threading.CancellationToken)" /> is
    ///     called.
    /// </param>
    /// <remarks>
    ///     See <see href="https://learn.microsoft.com/dotnet/core/extensions/workers">Worker Services in .NET</see> for
    ///     implementation guidelines.
    /// </remarks>
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var listener = new TcpListener(IPAddress.Any, 4000);
        listener.Start();
        Console.WriteLine("TCP server listening on port 4000");

        while (!cancellationToken.IsCancellationRequested)
        {
            var client = await listener.AcceptTcpClientAsync(cancellationToken);
            _ = this.HandleClient(client, cancellationToken);
        }
    }

    /// <summary>
    ///     Decompresses and decodes the data received.
    /// </summary>
    /// <param name="buffer">The buffer containing the data.</param>
    /// <param name="bytesRead">The number bytes of the buffer to use.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Returns a string containing the data.</returns>
    private static async Task<string> DecodeData(byte[] buffer, int bytesRead, CancellationToken cancellationToken)
    {
        using var compressed = new MemoryStream(buffer, 0, bytesRead);
        await using var gzip = new GZipStream(compressed, CompressionMode.Decompress);

        using var decompressed = new MemoryStream();
        await gzip.CopyToAsync(decompressed, cancellationToken);
        var decompressedData = decompressed.ToArray();

        var data = Encoding.UTF8.GetString(decompressedData);
        return data;
    }

    /// <summary>
    ///     Handles the communication with one client.
    /// </summary>
    /// <param name="client">The client.</param>
    /// <param name="cancellationToken">The stopping token.</param>
    private async Task HandleClient(TcpClient client, CancellationToken cancellationToken)
    {
        var clientId = $"{Interlocked.Increment(ref clientCounter)}";
        this.TcpClients.Add(clientId, client);
        Console.WriteLine($"Client {clientId} connected.");

        await using var stream = client.GetStream();
        var buffer = new byte[10240];

        while (!cancellationToken.IsCancellationRequested && client.Connected)
        {
            int bytesRead;
            try
            {
                bytesRead = await stream.ReadAsync(buffer, cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Read from client {clientId}, error: {ex.Message}");
                break;
            }

            if (bytesRead == 0)
            {
                break;
            }

            try
            {
                var data = await DecodeData(buffer, bytesRead, cancellationToken);

                await (MessageContainer.FromMessage(data)?.Let(container =>
                    this.BroadcastMessages(clientId, container, cancellationToken)) ?? Task.CompletedTask);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling message from client {clientId}: {ex.Message}");
            }
        }

        await this.RemoveClient(clientId, $"Client {clientId} disconnected.", cancellationToken);
    }

    /// <summary>
    ///     Sends the message(s) to all connected SignalR clients.
    /// </summary>
    /// <param name="clientId">The client identifier of the TCP client.</param>
    /// <param name="container">The container with the message(s).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    private async Task BroadcastMessages(
        string clientId,
        MessageContainer container,
        CancellationToken cancellationToken)
    {
        await Task.WhenAll(
            container.Messages.AsParallel()
                .Select(command => this.SendToAllClients(command.Envelop(clientId), cancellationToken)));
    }

    /// <summary>
    ///     Removes the client, logs a debug message to the console and notifies all connected SignalR clients.
    /// </summary>
    /// <param name="clientId">The client identifier.</param>
    /// <param name="debugMessage">The debug message.</param>
    /// <param name="cancellationToken">The stopping token.</param>
    private async Task RemoveClient(string clientId, string debugMessage, CancellationToken cancellationToken)
    {
        Console.WriteLine(debugMessage);
        this.TcpClients.Remove(clientId);
        await hubContext.Clients.All.SendAsync(
            Commands.RemoveClientCommand,
            clientId,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    ///     Sends the specified message to all connected SignalR clients.
    /// </summary>
    /// <param name="envelope">The envelope containing the command and client identifier to send. Cannot be null.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    private Task SendToAllClients(JasmCommandEnvelope envelope, CancellationToken cancellationToken) =>
        hubContext.Clients.All.SendAsync(envelope.JasmCommand.Command, envelope, cancellationToken: cancellationToken);
}