using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using OpenTK.Math;
using Orion.GameLogic;
using ZoneAttackTask = Orion.GameLogic.Tasks.ZoneAttack;

namespace Orion.Commandment.Commands
{
    /// <summary>
    /// A <see cref="Command"/> which causes one or many <see cref="Unit"/>s
    /// to move to a location and attack enemies on their way.
    /// </summary>
    public sealed class ZoneAttack : Command
    {
        #region Instance
        #region Fields
        private readonly ReadOnlyCollection<Handle> attackerHandles;
        private readonly Vector2 destination; 
        #endregion

        #region Constructors
        public ZoneAttack(Handle factionHandle, IEnumerable<Handle> attackerHandles, Vector2 destination)
            : base(factionHandle)
        {
            Argument.EnsureNotNullNorEmpty(attackerHandles, "attackerHandles");

            this.attackerHandles = attackerHandles.Distinct().ToList().AsReadOnly();
            this.destination = destination;
        }
        #endregion

        #region Properties
        public override IEnumerable<Handle> ExecutingEntityHandles
        {
            get { return attackerHandles; }
        }
        #endregion

        #region Methods
        public override void Execute(World world)
        {
            Argument.EnsureNotNull(world, "world");

            foreach (Handle attackerHandle in attackerHandles)
            {
                Unit attacker = (Unit)world.Entities.FindFromHandle(attackerHandle);
                attacker.Task = new ZoneAttackTask(attacker, destination);
            }
        }

        public override string ToString()
        {
            return "[{0}] zone attack to {1}".FormatInvariant(attackerHandles.ToCommaSeparatedValues(), destination);
        }
        #endregion
        #endregion

        #region Serializer Class
        /// <summary>
        /// A <see cref="CommandSerializer"/> that provides serialization to the <see cref="ZoneAttack"/> command.
        /// </summary>
        [Serializable]
        public sealed class Serializer : CommandSerializer<ZoneAttack>
        {
            #region Instance
            #region Methods
            protected override void SerializeData(ZoneAttack command, BinaryWriter writer)
            {
                WriteHandle(writer, command.FactionHandle);
                WriteLengthPrefixedHandleArray(writer, command.attackerHandles);
                writer.Write(command.destination.X);
                writer.Write(command.destination.Y);
            }

            protected override ZoneAttack DeserializeData(BinaryReader reader)
            {
                Handle factionHandle = ReadHandle(reader);
                var attackerHandles = ReadLengthPrefixedHandleArray(reader);
                float x = reader.ReadSingle();
                float y = reader.ReadSingle();
                Vector2 destination = new Vector2(x, y);
                return new ZoneAttack(factionHandle, attackerHandles, destination);
            }
            #endregion
            #endregion

            #region Static
            #region Fields
            /// <summary>
            /// A globally available static instance of this class.
            /// </summary>
            public static readonly Serializer Instance = new Serializer();
            #endregion
            #endregion
        }
        #endregion
    }
}
