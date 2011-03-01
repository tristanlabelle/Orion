using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Diagnostics;
using Orion.Engine;
using Orion.Game.Simulation;
using Orion.Game.Simulation.Technologies;
using Orion.Game.Simulation.Tasks;
using Orion.Game.Simulation.Components;

namespace Orion.Game.Matchmaking.Commands
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

        #region Properties
        public override IEnumerable<Handle> ExecutingUnitHandles
        {
            get { return new[] { researcherHandle }; }
        }
        #endregion

        #region Methods
        public override bool ValidateHandles(Match match)
        {
            Argument.EnsureNotNull(match, "match");

            return IsValidTechnologyHandle(match, technologyHandle)
                && IsValidFactionHandle(match, FactionHandle)
                && IsValidEntityHandle(match, researcherHandle);
        }

        public override void Execute(Match match)
        {
            Argument.EnsureNotNull(match, "match");

            Technology technology = match.TechnologyTree.FromHandle(technologyHandle);
            Faction faction = match.World.FindFactionFromHandle(FactionHandle);
            Entity researcher = match.World.Entities.FromHandle(researcherHandle);

            int aladiumCost = technology.AladdiumCost;
            int alageneCost = technology.AlageneCost;

            if (faction.AladdiumAmount >= aladiumCost && faction.AlageneAmount >= alageneCost)
            {
                TaskQueue taskQueue = researcher.Components.Get<TaskQueue>();
                if (taskQueue.IsEmpty)
                {
                    taskQueue.Enqueue(new ResearchTask(researcher, technology));
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

        #region Serialization
        public static void Serialize(ResearchCommand command, BinaryWriter writer)
        {
            Argument.EnsureNotNull(command, "command");
            Argument.EnsureNotNull(writer, "writer");

            WriteHandle(writer, command.FactionHandle);
            WriteHandle(writer, command.researcherHandle);
            WriteHandle(writer, command.technologyHandle);
        }

        public static ResearchCommand Deserialize(BinaryReader reader)
        {
            Argument.EnsureNotNull(reader, "reader");

            Handle factionHandle = ReadHandle(reader);
            Handle researcherHandle = ReadHandle(reader);
            Handle technologyHandle = ReadHandle(reader);

            return new ResearchCommand(researcherHandle, factionHandle, technologyHandle);
        }
        #endregion
        #endregion
    }
}
