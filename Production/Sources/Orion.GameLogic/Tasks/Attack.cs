using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.GameLogic.Tasks
{
    /// <summary>
    /// A <see cref="Task"/> which make attack an enemy <see cref="Unit"/>
    /// </summary>
    public sealed class Attack : Task
    {
        #region Field
        private readonly Unit striker;
        private readonly Unit enemy; 
        #endregion

        #region Constructors
        public Attack(Unit striker, Unit enemy)
        {
            Argument.EnsureNotNull(striker, "striker");
            Argument.EnsureNotNull(enemy, "enemy");

            this.striker = striker;
            this.enemy = enemy;
        }
        #endregion

        #region Properties
        public override string Description
        {
            get { return "attacking"; }
        }
        #endregion

        #region Methods
        public override void Update(float timeDelta)
        {
            if (HasEnded) 
                return;

            Vector2 delta = destination - unit.Position;
            Vector2 direction = Vector2.Normalize(delta);

            float distance = unit.Type.MovementSpeed * timeDelta;
            if (distance < delta.Length) 
                unit.Position += direction * distance;
            else 
                unit.Position = destination;

            // http://cgp.wikidot.com/circle-to-circle-collision-detection
            // To be checked for circle collision detection. 

        }
        #endregion

    }
}

