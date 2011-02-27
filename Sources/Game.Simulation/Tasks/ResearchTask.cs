using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Game.Simulation.Technologies;

namespace Orion.Game.Simulation.Tasks
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
            : base(researcher)
        {
            this.technology = technology;
            researcher.Faction.BeginResearch(technology);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the <see cref="Technology"/> being researched.
        /// </summary>
        public Technology Technology
        {
            get { return technology; }
        }

        public override string Description
        {
            get { return "Researching " + technology.Name; }
        }

        public override float Progress
        {
            get { return Math.Min(timeElapsed / researchTimeInSeconds, 1); }
        }
        #endregion

        #region Methods
        protected override void DoUpdate(SimulationStep step)
        {
            timeElapsed += step.TimeDeltaInSeconds;
            if (timeElapsed >= researchTimeInSeconds)
            {
                Unit.Faction.CompleteResearch(technology);
                MarkAsEnded();
            }
        }

        public override void Dispose()
        {
            if (!HasEnded) Unit.Faction.CancelResearch(technology);
        }
        #endregion
    }
}
