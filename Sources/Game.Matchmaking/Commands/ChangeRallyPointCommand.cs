using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using OpenTK;
using Orion.Engine;
using Orion.Engine.Collections;
using Orion.Game.Simulation;
using Orion.Game.Simulation.Components;

namespace Orion.Game.Matchmaking.Commands
{
    /// <summary>
    /// A <see cref="Command"/> which causes the rally point of a <see cref="Entity"/> to be changed to a new value.
    /// </summary>
    [Serializable]
    public sealed class ChangeRallyPointCommand : Command
    {
        #region Fields
        private readonly ReadOnlyCollection<Handle> buildingHandles;
        private readonly Vector2 position;
        #endregion

        #region Constructors
        public ChangeRallyPointCommand(Handle factionHandle, IEnumerable<Handle> buildingHandles, Vector2 position)
            : base(factionHandle)
        {
            Argument.EnsureNotNullNorEmpty(buildingHandles, "BuildingChangeRally");

            this.buildingHandles = buildingHandles.Distinct().ToList().AsReadOnly();
            this.position = position;
        }
        #endregion

        #region Properties
        public override IEnumerable<Handle> ExecutingUnitHandles
        {
            get { return buildingHandles; }
        }
        #endregion

        #region Methods
        public override bool ValidateHandles(Match match)
        {
            Argument.EnsureNotNull(match, "match");

            return IsValidFactionHandle(match, FactionHandle)
                && buildingHandles.All(handle => IsValidEntityHandle(match, handle));
        }

        public override void Execute(Match match)
        {
            Argument.EnsureNotNull(match, "match");

            foreach (Handle buildingHandle in buildingHandles)
            {
                Entity entity = match.World.Entities.FromHandle(buildingHandle);
                Trainer trainer = entity.Components.TryGet<Trainer>();
                if (trainer != null) trainer.RallyPoint = position;
            }
        }

        public override string ToString()
        {
            return "Faction {0} set Rally Point of  {1} to {2}"
                .FormatInvariant(FactionHandle, buildingHandles.ToCommaSeparatedValues(), position);
        }

        #region Serialization
        public static void Serialize(ChangeRallyPointCommand command, BinaryWriter writer)
        {
            Argument.EnsureNotNull(command, "command");
            Argument.EnsureNotNull(writer, "writer");

            WriteHandle(writer, command.FactionHandle);
            WriteLengthPrefixedHandleArray(writer, command.buildingHandles);
            writer.Write(command.position.X);
            writer.Write(command.position.Y);
        }

        public static ChangeRallyPointCommand Deserialize(BinaryReader reader)
        {
            Argument.EnsureNotNull(reader, "reader");

            Handle factionHandle = ReadHandle(reader);
            var buildingHandles = ReadLengthPrefixedHandleArray(reader);
            float x = reader.ReadSingle();
            float y = reader.ReadSingle();
            Vector2 destination = new Vector2(x, y);
            return new ChangeRallyPointCommand(factionHandle, buildingHandles, destination);
        }
        #endregion
        #endregion
    }
}

