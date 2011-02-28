using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Orion.Engine;
using Orion.Engine.Collections;
using Orion.Game.Simulation;
using AttackTask = Orion.Game.Simulation.Tasks.AttackTask;

namespace Orion.Game.Matchmaking.Commands
{
    /// <summary>
    /// A <see cref="Command"/> which causes one or many <see cref="Entity"/>s
    /// to attack another <see cref="Entity"/>.
    /// </summary>
    public sealed class AttackCommand : Command
    {
        #region Fields
        private readonly ReadOnlyCollection<Handle> attackerHandles;
        private readonly Handle targetHandle; 
        #endregion

        #region Constructors
        public AttackCommand(Handle factionHandle, IEnumerable<Handle> attackerHandles, Handle targetHandle)
            : base(factionHandle)
        {
            Argument.EnsureNotNull(attackerHandles, "attackerHandles");
            if (attackerHandles.Contains(targetHandle))
                throw new ArgumentException("A unit cannot attack itself.");

            this.attackerHandles = attackerHandles.Distinct().ToList().AsReadOnly();
            this.targetHandle = targetHandle;
        }
        #endregion

        #region Properties
        public override bool IsMandatory
        {
            get { return false; }
        }

        public override IEnumerable<Handle> ExecutingUnitHandles
        {
            get { return attackerHandles; }
        }

        public Handle TargetHandle
        {
            get { return targetHandle; }
        }
        #endregion

        #region Methods
        public override bool ValidateHandles(Match match)
        {
            Argument.EnsureNotNull(match, "match");

            return IsValidFactionHandle(match, FactionHandle)
                && attackerHandles.All(handle => IsValidEntityHandle(match, handle))
                && IsValidEntityHandle(match, targetHandle);
        }

        public override void Execute(Match match)
        {
            Argument.EnsureNotNull(match, "match");

            Unit target = (Unit)match.World.Entities.FromHandle(targetHandle);

            foreach (Handle attackerHandle in attackerHandles)
            {
                Unit attacker = (Unit)match.World.Entities.FromHandle(attackerHandle);
                attacker.TaskQueue.Enqueue(new AttackTask(attacker, target));
            }
        }

        public override string ToString()
        {
            return "Faction {0} attacks {1} with {2}"
                .FormatInvariant(FactionHandle, targetHandle, attackerHandles.ToCommaSeparatedValues());
        }
        
        #region Serialization
        public static void Serialize(AttackCommand command, BinaryWriter writer)
        {
            Argument.EnsureNotNull(command, "command");
            Argument.EnsureNotNull(writer, "writer");

            WriteHandle(writer, command.FactionHandle);
            WriteLengthPrefixedHandleArray(writer, command.attackerHandles);
            WriteHandle(writer, command.targetHandle);
        }

        public static AttackCommand Deserialize(BinaryReader reader)
        {
            Argument.EnsureNotNull(reader, "reader");

            Handle factionHandle = ReadHandle(reader);
            var attackerHandles = ReadLengthPrefixedHandleArray(reader);
            Handle targetHandle = ReadHandle(reader);
            return new AttackCommand(factionHandle, attackerHandles, targetHandle);
        }
        #endregion
        #endregion
    }
}