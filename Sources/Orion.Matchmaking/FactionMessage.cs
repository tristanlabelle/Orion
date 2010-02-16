using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Orion.GameLogic;

namespace Orion.Matchmaking
{
    /// <summary>
    /// Encapsulates a message sent by a faction.
    /// </summary>
    [Serializable]
    [ImmutableObject(true)]
    public sealed class FactionMessage
    {
        #region Fields
        private readonly Faction faction;
        private readonly string text;
        #endregion

        #region Constructors
        public FactionMessage(Faction faction, string text)
        {
            Argument.EnsureNotNull(faction, "faction");
            Argument.EnsureNotNull(text, "text");

            this.faction = faction;
            this.text = text;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the faction which sent this message.
        /// </summary>
        public Faction Faction
        {
            get { return faction; }
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
        public override string ToString()
        {
            return "{0}: {1}".FormatInvariant(faction, text);
        }
        #endregion
    }
}
