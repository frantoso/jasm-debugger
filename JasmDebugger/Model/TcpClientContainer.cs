// -----------------------------------------------------------------------
// <copyright file="TcpClientContainer.cs">
//     Created by Frank Listing at 2026/01/08.
// </copyright>
// -----------------------------------------------------------------------

namespace JasmDebugger.Model;

using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Client.Model;

/// <summary>
///     This class holds all connected TCP clients and allows sending messages to them.
/// </summary>
public class TcpClientContainer
{
    /// <summary>
    ///     The clients, identified by their ID.
    /// </summary>
    private readonly Dictionary<string, TcpClient> clients = new();

    /// <summary>
    ///     Adds a new client.
    /// </summary>
    /// <param name="client">The client.</param>
    /// <param name="id">The identifier.</param>
    public void Add(string id, TcpClient client)
    {
        lock (this)
        {
            this.clients[id] = client;
        }

        Console.WriteLine("Client added.");
    }

    /// <summary>
    ///     Removes a client identified by the specified ID.
    /// </summary>
    /// <param name="id">The identifier.</param>
    public void Remove(string id)
    {
        lock (this)
        {
            this.clients.Remove(id);
        }

        Console.WriteLine("Client removed.");
    }

    /// <summary>
    ///     Sends the specified command envelope to a TCP client. The client is identified by the ClientId property of the
    ///     command envelope.
    /// </summary>
    /// <param name="command">The command envelope containing the command and client identifier to send. Cannot be null.</param>
    /// <returns>A task that represents the asynchronous send operation.</returns>
    public async Task SendAsync(JasmCommandEnvelope command) => await this.SendAsync(
        command.ClientId,
        JsonSerializer.Serialize(command.JasmCommand));

    /// <summary>
    ///     Sends the specified message to a TCP client. The client is identified by the specified client ID.
    /// </summary>
    /// <param name="clientId">The client identifier.</param>
    /// <param name="message">The message.</param>
    /// <returns>A task that represents the asynchronous send operation.</returns>
    public async Task SendAsync(string clientId, string message)
    {
        var buffer = Encoding.UTF8.GetBytes(message);
        Monitor.Enter(this);
        try
        {
            if (this.clients.TryGetValue(clientId, out var tcpClient))
            {
                await tcpClient.GetStream().WriteAsync(buffer);
            }
        }
        catch
        {
            // ignored
        }
        finally
        {
            Monitor.Exit(this);
        }
    }

    /// <summary>
    ///     Sends the specified command to all connected TCP clients.
    /// </summary>
    /// <param name="command">The command to send. Cannot be null.</param>
    /// <returns>A task that represents the asynchronous send operation.</returns>
    public async Task SendToAllAsync(JasmCommand command) =>
        await this.SendToAllAsync(JsonSerializer.Serialize(command));

    /// <summary>
    ///     Sends the specified message to all TCP clients.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <returns>A task that represents the asynchronous send operation.</returns>
    public async Task SendToAllAsync(string message)
    {
        var buffer = Encoding.UTF8.GetBytes(message);
        Monitor.Enter(this);
        try
        {
            foreach (var tcpClient in this.clients.Values.Where(client => client.Connected))
            {
                await tcpClient.GetStream().WriteAsync(buffer);
            }
        }
        catch
        {
            // ignored
        }
        finally
        {
            Monitor.Exit(this);
        }
    }
}