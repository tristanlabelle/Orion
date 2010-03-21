using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Diagnostics;
using Orion.Engine;
using Orion.GameLogic;
using Orion.GameLogic.Technologies;
using Orion.GameLogic.Tasks;

namespace Orion.Matchmaking.Commands
{
    public sealed class ResearchCommand : Command
    {
        #region Fields
        private Handle researcherHandle;
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
                && IsValidFactionHandle(world, FactionHandle)
                && IsValidEntityHandle(world, researcherHandle);

        }

        public override void Execute(Match match)
        {
            Argument.EnsureNotNull(match, "match");

            Technology technology = match.World.TechnologyTree.FromHandle(technologyHandle);
            Faction faction = match.World.FindFactionFromHandle(FactionHandle);
            Unit researcher = (Unit)match.World.Entities.FromHandle(researcherHandle);

            int aladiumCost = technology.AladdiumCost;
            int alageneCost = technology.AlageneCost;

            if (faction.AladdiumAmount >= aladiumCost && faction.AlageneAmount >= alageneCost)
            {
                if (researcher.TaskQueue.IsEmpty)
                {
                    researcher.TaskQueue.Enqueue(new ResearchTask(researcher, technology));
                    faction.AladdiumAmount -= aladiumCost;
                    faction.AlageneAmount -= alageneCost;
                }
            }
            else
            {
                faction.RaiseWarning("Pas assez de ressources");
            }
        }

        public override string ToString()
        {
            return "Faction {0} researches {2} with {1}"
                .FormatInvariant(FactionHandle, researcherHandle, technologyHandle);
        }

        protected override void SerializeSpecific(System.IO.BinaryWriter writer)
        {
            WriteHandle(writer, FactionHandle);
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
