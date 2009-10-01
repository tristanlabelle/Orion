﻿using System;
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
        #region Fields
        private readonly Unit striker;
        private readonly Unit enemy; 
        private const float secondsToHitEnemy = 1;
        private float secondsStored = 0;
        private Follow follow;
        #endregion

        #region Constructors
        public Attack(Unit striker, Unit enemy)
        {
            Argument.EnsureNotNull(striker, "striker");
            Argument.EnsureNotNull(enemy, "enemy");
            
            this.striker = striker;
            this.enemy = enemy;
            this.follow = new Follow(striker, enemy, striker.Type.AttackRange);

        }
        #endregion

        #region Properties
        public override string Description
        {
            get { return "attacking"; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// At each update it check if the striker is near enough to strike if not he reupdate the follow.update method.
        /// </summary>
        /// <param name="timeDelta"></param>
        public override void Update(float timeDelta)
        {
            if (HasEnded) 
                return;

           if(follow.IsInRange)
           {
                if(InflictDamageToEnemyPossible(timeDelta))
                    enemy.Damage += 1;
            }
           else
            {
                follow.Update(timeDelta);
          
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