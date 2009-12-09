using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Orion.GameLogic;

namespace Orion.Commandment.Commands
{
    public class ResearchCommand:Command
    {

        #region Fields
        private Handle researcherHandle;
        private Handle factionHandle;
        private Handle technologyHandle;
        #endregion

        #region Constructors

        public ResearchCommand(Handle researcherHandle, Handle factionHandle, Handle technologyHandle)
            :base(factionHandle)
        {
            this.researcherHandle = researcherHandle;
            this.technologyHandle = technologyHandle;
        }

        #endregion

        #region Methods

        public override bool ValidateHandles(World world)
        {
            Argument.EnsureNotNull(world, "world");

            return IsValidTechnologyHandle(world, technologyHandle)
                && IsValidFactionHandle(world, factionHandle)
                && IsValidEntityHandle(world, researcherHandle);

        }

        public override void Execute(Match match)
        {
            Argument.EnsureNotNull(match, "match");

            //TODO: dispatch tasks
        }

        public override string ToString()
        {
            return "Faction {0} researches {2} with {1}"
                .FormatInvariant(FactionHandle, researcherHandle, technologyHandle);
        }

        protected override void SerializeSpecific(System.IO.BinaryWriter writer)
        {
            WriteHandle(writer, factionHandle);
            WriteHandle(writer, researcherHandle);
            WriteHandle(writer, technologyHandle);
        }

        public override IEnumerable<Handle> ExecutingEntityHandles
        {
            get { List<Handle> handles = new List<Handle>();
            handles.Add(researcherHandle);
                return handles;
            }
        }

        public static ResearchCommand DeserializeSpecific(BinaryReader reader)
        {
            Argument.EnsureNotNull(reader, "reader");

            Handle factionHandle = ReadHandle(reader);
            Handle researcherHandle = ReadHandle(reader);
            Handle technologyHandle = ReadHandle(reader);

            return new ResearchCommand(researcherHandle, factionHandle, technologyHandle);
        }
        #endregion
    }
}
