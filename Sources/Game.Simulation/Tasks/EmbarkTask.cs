using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Orion.Engine;
using Orion.Game.Simulation.Skills;

namespace Orion.Game.Simulation.Tasks
{
    /// <summary>
    /// A <see cref="Task"/> which causes a <see cref="Unit"/>
    /// to embark in another <see cref="Unit"/>.
    /// </summary>
    public sealed class EmbarkTask : Task
    {
        #region Fields
        private readonly Unit transporter;
        private readonly FollowTask followTask;
        #endregion

        #region Constructors
        public EmbarkTask(Unit unit, Unit transporter)
            : base(unit)
        {
            Argument.EnsureNotNull(transporter, "transporter");
            if (!transporter.CanTransport(unit))
                throw new ArgumentException("A {0} cannot transport a {1}.".FormatInvariant(transporter, unit));

            this.transporter = transporter;
            this.followTask = new FollowTask(unit, transporter);
        }
        #endregion

        #region Properties
        public override string Description
        {
            get { return "Embarking into {0}".FormatInvariant(transporter.Type.Name); }
        }
        #endregion

        #region Methods
        protected override void DoUpdate(SimulationStep step)
        {
            if (!transporter.IsAliveInWorld)
            {
                MarkAsEnded();
                return;
            }

            followTask.Update(step);
            if (!Region.AreAdjacentOrIntersecting(Unit.GridRegion, transporter.GridRegion))
            {
                if (followTask.HasEnded) MarkAsEnded();
                return;
            }

            if (transporter.IsTransportFull)
            {
                Unit.Faction.RaiseWarning("Pas assez de place pour embarquer l'unité.");
                MarkAsEnded();
                return;
            }

            transporter.Embark(Unit);
            MarkAsEnded();
        }
        #endregion
    }
}
