// -----------------------------------------------------------------------
// <copyright file="Extensions.cs">
//     Created by Frank Listing at 2025/12/19.
// </copyright>
// -----------------------------------------------------------------------

namespace JasmDebugger.Client.Model;

public static class Extensions
{
    extension(JasmCommand command)
    {
        /// <summary>
        ///     Envelops a command and associates it with the specified client identifier.
        /// </summary>
        /// <param name="clientId">The client identifier.</param>
        /// <returns>Returns the envelope.</returns>
        public JasmCommandEnvelope Envelop(string clientId) => new(clientId, command);
    }
}