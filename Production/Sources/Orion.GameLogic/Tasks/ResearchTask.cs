using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Orion.GameLogic.Technologies;

namespace Orion.GameLogic.Tasks
{
    /// <summary>
    /// A <see cref="Task"/> which causes a <see cref="Unit"/>
    /// </summary>
    public sealed class ResearchTask : Task
    {
        #region Fields
        private const float researchTimeInSeconds = 5;

        private readonly Technology technology;
        private float timeElapsed;
        #endregion

        #region Constructors
        public ResearchTask(Unit researcher, Technology technology)
            :base(researcher)
        {
            this.technology = technology;
        }
        #endregion

        #region Properties
        public Technology Technology
        {
            get { return technology; }
        }

        public override string Description
        {
            get { return "Researching " + technology.Name; }
        }

        public override bool HasEnded
        {
            get { return timeElapsed >= researchTimeInSeconds; }
        }
        #endregion

        #region Methods
        protected override void DoUpdate(SimulationStep step)
        {
            if (timeElapsed == 0)
            {
                bool hasEnoughResources = Unit.Faction.AladdiumAmount >= technology.Requirements.AladdiumCost
                    && Unit.Faction.AlageneAmount >= technology.Requirements.AlageneCost;
                if (!hasEnoughResources)
                {
                    Debug.WriteLine("Not enough resources to research {0}.".FormatInvariant(technology));
                    return;
                }

                Unit.Faction.AladdiumAmount -= technology.Requirements.AladdiumCost;
                Unit.Faction.AlageneAmount -= technology.Requirements.AlageneCost;
                timeElapsed = float.Epsilon;
            }

            timeElapsed += step.TimeDeltaInSeconds;
            if (timeElapsed >= researchTimeInSeconds)
                Unit.Faction.ResearchTechnology(technology);
        }
        #endregion
    }
}
