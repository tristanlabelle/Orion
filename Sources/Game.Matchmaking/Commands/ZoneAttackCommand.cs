using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using OpenTK;
using Orion.Engine;
using Orion.Engine.Collections;
using Orion.Game.Simulation;
using Orion.Game.Simulation.Tasks;

namespace Orion.Game.Matchmaking.Commands
{
    /// <summary>
    /// A <see cref="Command"/> which causes one or many <see cref="Entity"/>s
    /// to move to a location and attack enemies on their way.
    /// </summary>
    public sealed class ZoneAttackCommand : Command
    {
        #region Fields
        private readonly ReadOnlyCollection<Handle> attackerHandles;
        private readonly Vector2 destination; 
        #endregion

        #region Constructors
        public ZoneAttackCommand(Handle factionHandle, IEnumerable<Handle> attackerHandles, Vector2 destination)
            : base(factionHandle)
        {
            Argument.EnsureNotNullNorEmpty(attackerHandles, "attackerHandles");

            this.attackerHandles = attackerHandles.Distinct().ToList().AsReadOnly();
            this.destination = destination;
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

        public Vector2 Destination
        {
            get { return destination; }
        }
        #endregion

        #region Methods
        public override bool ValidateHandles(Match match)
        {
            Argument.EnsureNotNull(match, "match");

            return IsValidFactionHandle(match, FactionHandle)
                && attackerHandles.All(handle => IsValidEntityHandle(match, handle));
        }

        public override void Execute(Match match)
        {
            Argument.EnsureNotNull(match, "match");

            foreach (Handle attackerHandle in attackerHandles)
            {
                Unit attacker = (Unit)match.World.Entities.FromHandle(attackerHandle);
                attacker.TaskQueue.Enqueue(new ZoneAttackTask(attacker, destination));
            }
        }

        public override string ToString()
        {
            return "Faction {0} zone attacks {1} to {2}"
                .FormatInvariant(FactionHandle, attackerHandles.ToCommaSeparatedValues(), destination);
        }
                
        #region Serialization
        public static void Serialize(ZoneAttackCommand command, BinaryWriter writer)
        {
            Argument.EnsureNotNull(command, "command");
            Argument.EnsureNotNull(writer, "writer");

            WriteHandle(writer, command.FactionHandle);
            WriteLengthPrefixedHandleArray(writer, command.attackerHandles);
            writer.Write(command.destination.X);
            writer.Write(command.destination.Y);
        }

        public static ZoneAttackCommand Deserialize(BinaryReader reader)
        {
            Argument.EnsureNotNull(reader, "reader");

            Handle factionHandle = ReadHandle(reader);
            var attackerHandles = ReadLengthPrefixedHandleArray(reader);
            float x = reader.ReadSingle();
            float y = reader.ReadSingle();
            Vector2 destination = new Vector2(x, y);
            return new ZoneAttackCommand(factionHandle, attackerHandles, destination);
        }
        #endregion
        #endregion
    }
}
