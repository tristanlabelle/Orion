using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.GameLogic.Tasks
{
    class Research:Task
    {
        #region Fields
        private Technology technology;
        private bool hasEnded = false;
        private float timeElapsed = 0;
        #endregion

        #region Constructors
        public Research(Unit researcher, Technology technology)
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
            get { return hasEnded; }
        }
        #endregion

        #region Methods

        protected override void  DoUpdate(float timeDelta)
        {
            timeElapsed += timeDelta;

            if (timeElapsed >= 5)
            {
                Unit.Faction.AcquireTechnology(technology);
                hasEnded = true;
            }
        }

        #endregion
    }
}
