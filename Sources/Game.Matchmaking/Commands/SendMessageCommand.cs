using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.IO;
using Orion.Engine;
using Orion.Game.Simulation;

namespace Orion.Game.Matchmaking.Commands
{
    /// <summary>
    /// A command which encapsulates some textual message sent by a <see cref="Faction"/>.
    /// </summary>
    public sealed class SendMessageCommand : Command
    {
        #region Fields
        private readonly ReadOnlyCollection<Handle> recipientFactionHandles;
        private readonly string text;
        #endregion

        #region Constructors
        public SendMessageCommand(Handle factionHandle, IEnumerable<Handle> recipientFactionHandles, string text)
            : base(factionHandle)
        {
            Argument.EnsureNotNull(text, "text");
            Argument.EnsureNotNull(recipientFactionHandles, "recipientFactionHandles");

            this.recipientFactionHandles = recipientFactionHandles.ToList().AsReadOnly();
            this.text = text;
        }
        #endregion

        #region Properties
        public override IEnumerable<Handle> ExecutingEntityHandles
        {
            get { yield break; }
        }

        public string Text
        {
            get { return text; }
        }
        #endregion

        #region Methods
        public override bool ValidateHandles(Match match)
        {
            Argument.EnsureNotNull(match, "match");

            return IsValidFactionHandle(match, FactionHandle);
        }

        public override void Execute(Match match)
        {
            Argument.EnsureNotNull(match, "match");

            Faction faction = match.World.FindFactionFromHandle(FactionHandle);
            FactionMessage factionMessage = new FactionMessage(faction, text);
            match.PostFactionMessage(factionMessage);
        }

        public override string ToString()
        {
            return "Faction {0} says \"{1}\"".FormatInvariant(FactionHandle, text);
        }

        #region Serialization
        public static void Serialize(SendMessageCommand command, BinaryWriter writer)
        {
            Argument.EnsureNotNull(command, "command");
            Argument.EnsureNotNull(writer, "writer");

            WriteHandle(writer, command.FactionHandle);
            writer.Write(command.recipientFactionHandles.Count);
            foreach (Handle handle in command.recipientFactionHandles) WriteHandle(writer, handle);
            writer.Write(command.text);
        }

        public static new SendMessageCommand Deserialize(BinaryReader reader)
        {
            Argument.EnsureNotNull(reader, "reader");

            Handle factionHandle = ReadHandle(reader);
            int recipientFactionsCount = reader.ReadInt32();
            var recipientFactionHandles = Enumerable.Range(0, recipientFactionsCount)
                .Select(i => ReadHandle(reader))
                .ToList();
            string text = reader.ReadString();
            return new SendMessageCommand(factionHandle, recipientFactionHandles, text);
        }
        #endregion
        #endregion
    }
}