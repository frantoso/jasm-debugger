// -----------------------------------------------------------------------
// <copyright file="StateMachine.cs">
//     Created by Frank Listing at 2026/01/07.
// </copyright>
// -----------------------------------------------------------------------

namespace JasmDebugger.Client.View;

using System.Xml.Linq;
using Microsoft.AspNetCore.Components;
using Model;

public class StateMachine(string clientId, string fsmName)
{
    /// <summary>
    ///     Gets the ID of the client.
    /// </summary>
    public string ClientId { get; } = clientId;

    /// <summary>
    ///     Gets the name of the FSM.
    /// </summary>
    // ReSharper disable once UnusedMember.Global
    public string FsmName { get; } = fsmName;

    /// <summary>
    ///     Gets the display name of the FSM (client ID + FSM name).
    /// </summary>
    public string Key { get; } = BuildKey(clientId, fsmName);

    /// <summary>
    ///     Gets the SVG document.
    /// </summary>
    public XElement? SvgDoc { get; private set; }

    /// <summary>
    ///     Gets the mark markup string generated from the contained svg document.
    /// </summary>
    public MarkupString MarkMarkupString => new($"{this.SvgDoc}");

    /// <summary>
    ///     Gets or sets the diagram.
    /// </summary>
    private Diagram? Diagram { get; set; }

    /// <summary>
    ///     Builds the key from client ID and FSM name.
    /// </summary>
    /// <param name="clientId">The client identifier.</param>
    /// <param name="fsmName">Name of the FSM.</param>
    /// <returns>Returns a string containing the key.</returns>
    public static string BuildKey(string clientId, string fsmName) => $"{clientId} - {fsmName}";

    /// <summary>
    ///     Called when a set-fsm command was received.
    /// </summary>
    /// <param name="command">The command with the data.</param>
    /// <returns>Returns a reference to this to support chaining.</returns>
    public StateMachine OnSetFsm(JasmCommand command)
    {
        try
        {
            this.Diagram = command.Payload.Deserialize<FsmInfo>()?.Let(info => new Diagram(info));
            this.SvgDoc = this.Diagram?.CalculateSvgDocument();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        return this;
    }

    /// <summary>
    ///     Called when an update-state command was received.
    /// </summary>
    /// <param name="command">The command with the data.</param>
    /// <returns>Returns a reference to this to support chaining.</returns>
    public StateMachine OnUpdateState(JasmCommand command)
    {
        var diagram = this.Diagram;
        var doc = this.SvgDoc;
        if (diagram == null || doc == null)
        {
            return this;
        }

        command.Payload.Deserialize<StateChangedInfo>()?.Run(info =>
        {
            Console.WriteLine($"{info.Fsm}: {info.OldStateName} ==> {info.NewStateName}");

            try
            {
                var result = diagram.Search(n => n.Id == info.OldStateId).FirstOrDefault();
                ResetNodeOrAll(result, info);
                diagram.Search(n => n.Id == info.NewStateId).FirstOrDefault()?.Item2.Highlight();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        });

        return this;
    }

    /// <summary>
    ///     Resets the node in the <see cref="diagramInfo" /> parameter to normal appearance. If the old state is an initial
    ///     state, all nodes of the diagram are reset.
    /// </summary>
    /// <param name="diagramInfo">The diagram information (diagram + start-node).</param>
    /// <param name="info">The state change information containing start- and end-state.</param>
    private static void ResetNodeOrAll(Tuple<Diagram, Node>? diagramInfo, StateChangedInfo info)
    {
        if (diagramInfo == null)
        {
            return;
        }

        if (info.OldStateName.StartsWith("initial", StringComparison.OrdinalIgnoreCase))
        {
            diagramInfo.Item1.StateNodes.Values.ToList().ForEach(n => n.Reset());
        }
        else
        {
            diagramInfo.Item2.Reset();
        }
    }
}