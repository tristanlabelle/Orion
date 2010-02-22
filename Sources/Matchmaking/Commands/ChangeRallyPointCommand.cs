using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using OpenTK.Math;
using Orion.GameLogic;

namespace Orion.Matchmaking.Commands
{
    /// <summary>
    /// A <see cref="Command"/> which causes the rally point of a <see cref="Unit"/> to be changed to a new value.
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
        public override IEnumerable<Handle> ExecutingEntityHandles
        {
            get { return buildingHandles; }
        }
        #endregion

        #region Methods
        public override bool ValidateHandles(World world)
        {
            Argument.EnsureNotNull(world, "world");

            return IsValidFactionHandle(world, FactionHandle)
                && buildingHandles.All(handle => IsValidEntityHandle(world, handle));
        }

        public override void Execute(Match match)
        {
            Argument.EnsureNotNull(match, "match");

            foreach (Handle buildingHandle in buildingHandles)
            {
                Unit building = (Unit)match.World.Entities.FromHandle(buildingHandle);
                if (!building.IsUnderConstruction) building.RallyPoint = position;
            }
        }

        public override string ToString()
        {
            return "Faction {0} set Rally Point of  {1} to {2}"
                .FormatInvariant(FactionHandle, buildingHandles.ToCommaSeparatedValues(), position);
        }

        #region Serialization
        protected override void SerializeSpecific(BinaryWriter writer)
        {
            WriteHandle(writer, FactionHandle);
            WriteLengthPrefixedHandleArray(writer, buildingHandles);
            writer.Write(position.X);
            writer.Write(position.Y);
        }

        public static ChangeRallyPointCommand DeserializeSpecific(BinaryReader reader)
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

