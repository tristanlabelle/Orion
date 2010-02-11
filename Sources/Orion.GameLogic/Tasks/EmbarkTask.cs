using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Orion.GameLogic.Skills;

namespace Orion.GameLogic.Tasks
{
    /// <summary>
    /// A <see cref="Task"/> which causes a <see cref="Unit"/>
    /// to embark in another <see cref="Unit"/>.
    /// </summary>
    public sealed class EmbarkTask : Task
    {
        #region Fields
        private readonly Unit target;
        private readonly FollowTask followTask;
        private bool hasEmbarked;
        #endregion

        #region Constructors
        public EmbarkTask(Unit unit, Unit target)
            : base(unit)
        {
            Argument.EnsureNotNull(target, "target");
            Argument.EnsureEqual(target.IsBuilding, false, "target.IsBuilding");
            Debug.Assert(unit.HasSkill<TransportSkill>());
            Debug.Assert(!target.IsBuilding);

            this.target = target;
            this.followTask = new FollowTask(unit, target);
        }
        #endregion

        #region Properties
        public override bool HasEnded
        {
            get { return hasEmbarked || followTask.HasEnded; }
        }

        public override string Description
        {
            get { return "Embarking into {0}".FormatInvariant(target.Type.Name); }
        }
        #endregion

        #region Methods
        protected override void DoUpdate(SimulationStep step)
        {
            followTask.Update(step);

            if (Region.AreAdjacentOrIntersecting(Unit.GridRegion, target.GridRegion))
            {
                Unit.Embark(target);
                hasEmbarked = true;
            }
        }
        #endregion
    }
}
