using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.GameLogic;
using OpenTK.Math;
using System.IO;
using System.Collections.ObjectModel;

namespace Orion.Commandment.Commands
{
    [Serializable]
    public sealed class ChangeRallyPoint : Command
    {

        #region Fields
        private readonly ReadOnlyCollection<Handle> buildingHandles;
        private readonly Vector2 destination;
        #endregion

        #region Constructors
        public ChangeRallyPoint(Handle factionHandle, IEnumerable<Handle> buildingHandles, Vector2 destination)
            : base(factionHandle)
        {
            Argument.EnsureNotNullNorEmpty(buildingHandles, "BuildingChangeRally");

            this.buildingHandles = buildingHandles.Distinct().ToList().AsReadOnly();
            this.destination = destination;
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
                building.RallyPoint = destination;
            }
        }

        public override string ToString()
        {
            return "Faction {0} set Rally Point of  {1} to {2}"
                .FormatInvariant(FactionHandle, buildingHandles.ToCommaSeparatedValues(), destination);
        }

        #region Serialization
        protected override void SerializeSpecific(BinaryWriter writer)
        {
            WriteHandle(writer, FactionHandle);
            WriteLengthPrefixedHandleArray(writer, buildingHandles);
            writer.Write(destination.X);
            writer.Write(destination.Y);
        }

        public static ChangeRallyPoint DeserializeSpecific(BinaryReader reader)
        {
            Argument.EnsureNotNull(reader, "reader");

            Handle factionHandle = ReadHandle(reader);
            var buildingHandles = ReadLengthPrefixedHandleArray(reader);
            float x = reader.ReadSingle();
            float y = reader.ReadSingle();
            Vector2 destination = new Vector2(x, y);
            return new ChangeRallyPoint(factionHandle, buildingHandles, destination);
        }
        #endregion
        #endregion
    }
}

