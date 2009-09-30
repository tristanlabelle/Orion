using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK.Math;

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
        private const float secondsToHitEnemy = 1;
        private float secondsStored = 0; 
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

            Vector2 delta = enemy.Position - striker.Position;
            Vector2 direction = Vector2.Normalize(delta);

            float distance = striker.Type.MovementSpeed * timeDelta;
            if (distance < delta.Length)
                striker.Position += direction * distance;
            else
            {
                striker.Position = enemy.Position;
                if(InflictDamageToEnemyPossible(timeDelta))
                    enemy.Damage += 1;
            }

        }/// <summary>
        /// Calculates the number of time elapsed in seconds and 
        /// inflicts damage to the enemy; dependant of the constant 
        /// named "secondsToHitEnemy". 
        /// </summary>
        /// 
        private bool InflictDamageToEnemyPossible(float timeDelta)
        {
            if (secondsStored >= secondsToHitEnemy)
            {
                secondsStored = 0;
                return true;
            }
            else
            {
                secondsStored += timeDelta;
                return false;
            }
        }
        #endregion
    }
}