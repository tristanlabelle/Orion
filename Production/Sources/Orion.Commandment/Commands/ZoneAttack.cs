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
        public override bool ValidateHandles(World world)
        {
            Argument.EnsureNotNull(world, "world");

            return IsValidFactionHandle(world, FactionHandle)
                && attackerHandles.All(handle => IsValidEntityHandle(world, handle));
        }

        public override void Execute(Match match)
        {
            Argument.EnsureNotNull(match, "match");

            foreach (Handle attackerHandle in attackerHandles)
            {
                Unit attacker = (Unit)match.World.Entities.FromHandle(attackerHandle);
                attacker.TaskQueue.OverrideWith(new ZoneAttackTask(attacker, destination));
            }
        }

        public override string ToString()
        {
            return "Faction {0} zone attacks {1} to {2}"
                .FormatInvariant(FactionHandle, attackerHandles.ToCommaSeparatedValues(), destination);
        }
                
        #region Serialization
        protected override void SerializeSpecific(BinaryWriter writer)
        {
            WriteHandle(writer, FactionHandle);
            WriteLengthPrefixedHandleArray(writer, attackerHandles);
            writer.Write(destination.X);
            writer.Write(destination.Y);
        }

        public static ZoneAttack DeserializeSpecific(BinaryReader reader)
        {
            Argument.EnsureNotNull(reader, "reader");

            Handle factionHandle = ReadHandle(reader);
            var attackerHandles = ReadLengthPrefixedHandleArray(reader);
            float x = reader.ReadSingle();
            float y = reader.ReadSingle();
            Vector2 destination = new Vector2(x, y);
            return new ZoneAttack(factionHandle, attackerHandles, destination);
        }
        #endregion
        #endregion
    }
}
