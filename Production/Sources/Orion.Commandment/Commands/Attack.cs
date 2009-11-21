using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Orion.GameLogic;
using AttackTask = Orion.GameLogic.Tasks.Attack;

namespace Orion.Commandment.Commands
{
    /// <summary>
    /// A <see cref="Command"/> which causes one or many <see cref="Unit"/>s
    /// to attack another <see cref="Unit"/>.
    /// </summary>
    public sealed class Attack : Command
    {
        #region Instance
        #region Fields
        private readonly ReadOnlyCollection<Handle> attackerHandles;
        private readonly Handle targetHandle; 
        #endregion

        #region Constructors
        public Attack(Handle factionHandle, IEnumerable<Handle> attackerHandles, Handle targetHandle)
            : base(factionHandle)
        {
            Argument.EnsureNotNull(attackerHandles, "attackerHandles");

            this.attackerHandles = attackerHandles.Distinct().ToList().AsReadOnly();
            this.targetHandle = targetHandle;
        }
        #endregion

        #region Properties
        public override IEnumerable<Handle> ExecutingEntityHandles
        {
            get { return attackerHandles; }
        }
        #endregion

        #region Methods
        public override void Execute(Match match)
        {
            Argument.EnsureNotNull(match, "match");

            Unit target = (Unit)match.World.Entities.FindFromHandle(targetHandle);

            foreach (Handle attackerHandle in attackerHandles)
            {
                Unit attacker = (Unit)match.World.Entities.FindFromHandle(attackerHandle);
                attacker.Task = new AttackTask(attacker, target);
            }
        }

        public override string ToString()
        {
            return "[{0}] attack {1}".FormatInvariant(attackerHandles.ToCommaSeparatedValues(), targetHandle);
        }
        #endregion
        #endregion

        #region Serializer Class
        /// <summary>
        /// A <see cref="CommandSerializer"/> that provides serialization to the <see cref="Attack"/> command.
        /// </summary>
        [Serializable]
        public sealed class Serializer : CommandSerializer<Attack>
        {
            #region Instance
            #region Methods
            protected override void SerializeData(Attack command, BinaryWriter writer)
            {
                WriteHandle(writer, command.FactionHandle);
                WriteLengthPrefixedHandleArray(writer, command.attackerHandles);
                WriteHandle(writer, command.targetHandle);
            }

            protected override Attack DeserializeData(BinaryReader reader)
            {
                Handle factionHandle = ReadHandle(reader);
                var attackerHandles = ReadLengthPrefixedHandleArray(reader);
                Handle targetHandle = ReadHandle(reader);
                return new Attack(factionHandle, attackerHandles, targetHandle);
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