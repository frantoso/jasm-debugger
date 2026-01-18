// -----------------------------------------------------------------------
// <copyright file="MessageContainer.cs">
//     Created by Frank Listing at 2026/01/08.
// </copyright>
// -----------------------------------------------------------------------

namespace JasmDebugger.Model;

using System.Text.Json;
using System.Text.RegularExpressions;
using Client.Model;
using Client.View;

/// <summary>
///     A helper class to deserialize multiple messages sent together over TCP.
/// </summary>
public partial class MessageContainer
{
    /// <summary>
    ///     Gets or sets the messages.
    /// </summary>
    public List<JasmCommand> Messages { get; set; } = [];

    /// <summary>
    ///     Parses the specified text message.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <returns>Returns a container with one or more <see cref="JasmCommand" /> instances.</returns>
    public static MessageContainer? FromMessage(string message)
    {
        try
        {
            // it may happen, that multiple JSON objects are sent together
            var messages = $"{{\"Messages\":[{SplitMessagesRegex().Replace(message, "},{")}]}}";
            return messages.Deserialize<MessageContainer>() ?? new MessageContainer();
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"JSON error: {ex.Message}");
        }

        return null;
    }

    /// <summary>
    ///     A precompiled regex used to split the incoming text.
    /// </summary>
    [GeneratedRegex("}\\s*{", RegexOptions.IgnoreCase)]
    private static partial Regex SplitMessagesRegex();
}