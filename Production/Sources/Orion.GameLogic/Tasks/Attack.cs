﻿using System;
using Orion.Geometry;


namespace Orion.GameLogic.Tasks
{    
    /// <summary>
    /// A <see cref="Task"/> which make attack an enemy <see cref="Unit"/>
    /// </summary>
    [Serializable]
    public sealed class Attack : Task
    {
        #region Fields
        private readonly Unit target;
        private readonly Follow follow;
        private float hitDelayInSeconds;
        #endregion

        #region Constructors
        public Attack(Unit attacker, Unit target)
            : base(attacker)
        {
            Argument.EnsureNotNull(attacker, "attacker");
            if (!attacker.HasSkill<Skills.Attack>())
                throw new ArgumentException("Cannot attack without the attack skill.", "attacker");
            Argument.EnsureNotNull(target, "target");
            
            this.target = target;
            if (attacker.HasSkill<Skills.Move>()) this.follow = new Follow(attacker, target);
            hitDelayInSeconds = attacker.GetStat(UnitStat.AttackDelay);
        }
        #endregion

        #region Properties
        public Unit Attacker
        {
            get { return Unit; }
        }

        public Unit Target
        {
            get { return target; }
        }

        public override string Description
        {
            get { return "attacking"; }
        }

        public override bool HasEnded
        {
            get
            {
                if (Unit.Faction.GetTileVisibility(target.GridRegion.Min) != TileVisibility.Visible)
                    return true;
                if (!Unit.IsInAttackRange(target))
                    return !Unit.HasSkill<Skills.Move>() || follow.HasEnded;
                return !target.IsAlive;
            }
        }
        #endregion

        #region Methods
        protected override void DoUpdate(UpdateInfo info)
        {
            if (Unit.IsInAttackRange(target))
            {
                Unit.TimeSinceLastHitInSeconds += info.TimeDeltaInSeconds;
                if (Unit.TimeSinceLastHitInSeconds > hitDelayInSeconds)
                {
                    target.Damage += Unit.GetStat(UnitStat.AttackPower);
                    Unit.TimeSinceLastHitInSeconds = 0;
                }
            }
            else if (follow != null)
            {
                follow.Update(info);
            }
        }
        #endregion
    }
}