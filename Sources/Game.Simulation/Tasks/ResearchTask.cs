using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Game.Simulation.Technologies;
using Orion.Game.Simulation.Components;

namespace Orion.Game.Simulation.Tasks
{
    /// <summary>
    /// A <see cref="Task"/> which causes a <see cref="Entity"/>
    /// </summary>
    public sealed class ResearchTask : Task
    {
        #region Fields
        private const float researchTimeInSeconds = 5;

        private readonly Technology technology;
        private readonly Faction faction;
        private float timeElapsed;
        #endregion

        #region Constructors
        public ResearchTask(Entity researcher, Technology technology)
            : base(researcher)
        {
            Argument.EnsureNotNull(technology, "technology");

            if (!researcher.Components.Has<Researcher>())
                throw new ArgumentException("Cannot research without the researcher component.", "researcher");

            this.faction = FactionMembership.GetFaction(researcher);
            if (faction == null)
                throw new ArgumentException("Cannot research without a faction.", "researcher");

            this.technology = technology;
            faction.BeginResearch(technology);
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

        public override Type PublicType
        {
            get { return null; }
        }
        #endregion

        #region Methods
        protected override void DoUpdate(SimulationStep step)
        {
            if (FactionMembership.GetFaction(Entity) != faction || !Entity.Components.Has<Researcher>())
            {
                faction.CancelResearch(technology);
                MarkAsEnded();
                return;
            }

            timeElapsed += step.TimeDeltaInSeconds;
            if (timeElapsed >= researchTimeInSeconds)
            {
                faction.CompleteResearch(technology);
                MarkAsEnded();
            }
        }

        public override void Dispose()
        {
            if (!HasEnded) faction.CancelResearch(technology);
        }
        #endregion
    }
}
