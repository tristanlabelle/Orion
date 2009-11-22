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
        #region Instance
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
        #endregion
        #endregion

        #region Serializer Class
        [Serializable]
        public sealed class Serializer : CommandSerializer<Message>
        {
            #region Instance
            #region Methods
            protected override void SerializeData(Message command, BinaryWriter writer)
            {
                WriteHandle(writer, command.FactionHandle);
                writer.Write(command.value);
            }

            protected override Message DeserializeData(BinaryReader reader)
            {
                Handle factionHandle = ReadHandle(reader);
                string text = reader.ReadString();
                return new Message(factionHandle, text);
            }
            #endregion
            #endregion

            #region Static
            #region Fields
            public static readonly Serializer Instance = new Serializer();
            #endregion
            #endregion
        }
        #endregion
    }
}
