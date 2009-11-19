using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Orion.GameLogic;

namespace Orion.Commandment.Commands
{
    public class Message : Command
    {
        #region Instance
        #region Fields
        private readonly string value;
        #endregion

        #region Constructors
        public Message(Faction source, string message)
            : base(source)
        {
            Argument.EnsureNotNull(message, "message");
            value = message;
        }
        #endregion

        #region Properties
        public override IEnumerable<Entity> EntitiesInvolved
        {
            get { yield break; }
        } 
        #endregion

        #region Methods
        public override void Execute()
        {

        }

        public override string ToString()
        {
            return "<{0}> {1}".FormatInvariant(SourceFaction, value);
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
                writer.Write(command.value);
            }

            protected override Message DeserializeData(BinaryReader reader, World world)
            {
                // todo
                // somehow obtain the faction from here
                return new Message(null, reader.ReadString());
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
