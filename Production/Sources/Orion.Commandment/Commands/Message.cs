using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Orion.GameLogic;

namespace Orion.Commandment.Commands
{
    public sealed class Message : Command
    {
        #region Fields
        private readonly string value;
        private readonly Handle originatingFaction;
        #endregion

        #region Constructors
        public Message(Handle factionHandle, string message)
            : base(factionHandle)
        {
            Argument.EnsureNotNull(factionHandle, "factionHandle");
            Argument.EnsureNotNull(message, "message");
            value = message;
            originatingFaction = factionHandle;
        }
        #endregion

        #region Properties
        public override IEnumerable<Handle> ExecutingEntityHandles
        {
            get { yield break; }
        }

        public string Value
        {
            get { return value; }
        }
        #endregion

        #region Methods
        public override void Execute(Match match)
        {
            Argument.EnsureNotNull(match, "match");
            match.PostMessage(match.World.FindFactionFromHandle(originatingFaction), value);
        }

        public override string ToString()
        {
            return "\"{0}\" message".FormatInvariant(value);
        }
        
        #region Serialization
        protected override void SerializeSpecific(BinaryWriter writer)
        {
            WriteHandle(writer, FactionHandle);
            writer.Write(value);
        }

        public static Message DeserializeSpecific(BinaryReader reader)
        {
            Argument.EnsureNotNull(reader, "reader");

            Handle factionHandle = ReadHandle(reader);
            string text = reader.ReadString();
            return new Message(factionHandle, text);
        }
        #endregion
        #endregion
    }
}
