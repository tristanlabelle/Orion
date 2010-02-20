using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Orion.GameLogic;
using AttackTask = Orion.GameLogic.Tasks.AttackTask;

namespace Orion.Matchmaking.Commands
{
    /// <summary>
    /// A <see cref="Command"/> which causes one or many <see cref="Unit"/>s
    /// to attack another <see cref="Unit"/>.
    /// </summary>
    public sealed class AttackCommand : Command, IMultipleExecutingEntitiesCommand
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
        public override IEnumerable<Handle> ExecutingEntityHandles
        {
            get { return attackerHandles; }
        }

        public Handle TargetHandle
        {
            get { return targetHandle; }
        }
        #endregion

        #region Methods
        public IMultipleExecutingEntitiesCommand CopyWithEntities(IEnumerable<Handle> entityHandles)
        {
            return new AttackCommand(FactionHandle, entityHandles, targetHandle);
        }

        public override bool ValidateHandles(World world)
        {
            Argument.EnsureNotNull(world, "world");

            return IsValidFactionHandle(world, FactionHandle)
                && attackerHandles.All(handle => IsValidEntityHandle(world, handle))
                && IsValidEntityHandle(world, targetHandle);
        }

        public override void Execute(Match match)
        {
            Argument.EnsureNotNull(match, "match");

            Unit target = (Unit)match.World.Entities.FromHandle(targetHandle);

            foreach (Handle attackerHandle in attackerHandles)
            {
                Unit attacker = (Unit)match.World.Entities.FromHandle(attackerHandle);
                attacker.TaskQueue.OverrideWith(new AttackTask(attacker, target));
            }
        }

        public override string ToString()
        {
            return "Faction {0} attacks {1} with {2}"
                .FormatInvariant(FactionHandle, targetHandle, attackerHandles.ToCommaSeparatedValues());
        }
        
        #region Serialization
        protected override void SerializeSpecific(BinaryWriter writer)
        {
            WriteHandle(writer, FactionHandle);
            WriteLengthPrefixedHandleArray(writer, attackerHandles);
            WriteHandle(writer, targetHandle);
        }

        public static AttackCommand DeserializeSpecific(BinaryReader reader)
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