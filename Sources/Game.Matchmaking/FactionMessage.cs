using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Game.Simulation;
using System.Collections.ObjectModel;

namespace Orion.Game.Matchmaking
{
    /// <summary>
    /// Encapsulates a message sent by a faction.
    /// </summary>
    [Serializable]
    [ImmutableObject(true)]
    public sealed class FactionMessage
    {
        #region Fields
        private readonly Faction sender;
        private readonly ReadOnlyCollection<Faction> recipients;
        private readonly string text;
        #endregion

        #region Constructors
        public FactionMessage(Faction sender, IEnumerable<Faction> recipients, string text)
        {
            Argument.EnsureNotNull(sender, "sender");
            Argument.EnsureNotNull(recipients, "recipients");
            Argument.EnsureNotNull(text, "text");

            this.sender = sender;
            this.recipients = recipients.ToList().AsReadOnly();
            this.text = text;
        }

        public FactionMessage(Faction faction, string text)
            : this(faction, Enumerable.Empty<Faction>(), text) { }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the faction which sent this message.
        /// </summary>
        public Faction Sender
        {
            get { return sender; }
        }

        /// <summary>
        /// Gets the text of the message sent by this faction.
        /// </summary>
        public string Text
        {
            get { return text; }
        }
        #endregion

        #region Methods
        public bool IsRecipient(Faction faction)
        {
            Argument.EnsureNotNull(faction, "faction");

            return recipients.Count == 0 || recipients.Contains(faction);
        }

        public override string ToString()
        {
            return "{0}: {1}".FormatInvariant(sender, text);
        }
        #endregion
    }
}
