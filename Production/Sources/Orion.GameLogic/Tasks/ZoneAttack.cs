using System;
using System.Linq;

using OpenTK.Math;
using Orion.Geometry;

namespace Orion.GameLogic.Tasks
{
    /// <summary>
    /// A <see cref="Task"/> which makes a<see cref="Unit"/> move to a location and attack enemies on it's way.
    /// </summary>
    [Serializable]
    public sealed class ZoneAttack : Task
    {
        #region Fields
        private readonly Unit attacker;
        private readonly Vector2 destination;
        private readonly float targetDistance;
        private Unit target = null;
        private Attack attack = null;
        private Move move;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new <see cref="ZoneAttack"/> task from the <see cref="Unit"/>
        /// that attacks and its destination.
        /// </summary>
        /// <param name="unit">The <see cref="Unit"/> who attacks.</param>
        /// <param name="destination">The destination of the unit'</param>
        public ZoneAttack(Unit striker, Vector2 destination)
        {
            Argument.EnsureNotNull(striker, "striker");
            Argument.EnsureNotNull(destination, "destination");
            
            this.attacker = striker;
            this.destination = destination;
            this.targetDistance = striker.GetStat(UnitStat.AttackRange);
            this.move = new Move(striker, destination);
        }
        #endregion

        #region Properties

        public override bool HasEnded
        {
            get { return move.HasEnded && (attack == null || attack.HasEnded); }
        }

        public override string Description
        {
            get { return "attacking while moving to {0}".FormatInvariant(destination); }
        }

        /// <summary>
        /// Gets the current distance remaining between this <see cref="Unit"/>
        /// and the followed <see cref="Unit"/>.
        /// </summary>
        public float CurrentDistance
        {
            get { return (target.Position - attacker.Position).Length; }
        }

        /// <summary>
        /// Gets a value indicating if the following <see cref="Unit"/>
        /// is within the target range of its <see cref="target"/>.
        /// </summary>
        public bool IsInRange
        {
            get { return CurrentDistance <= targetDistance; }
        }

        #endregion

        #region Methods
        /// <summary>
        /// At each update, checks if an enemy unit is in range of the striker, if so it creates an attack taks
        /// if not the units moves towards its destination. The appropriate tasks are uptated each time.
        /// </summary>
        /// <param name="timeDelta"></param>
        public override void Update(float timeDelta)
        {
            if (attack == null)
            {
                // If there's no-one in the attack range
                if (!TryAttack())
                {
                    // If we reached our destination.
                    if (move.HasEnded)
                    {
                        return;
                    }
                    else
                    {
                        move.Update(timeDelta);
                    }
                }
            }
            else // if attacking
            {
                if (attack.HasEnded)
                {
                    if (!TryAttack())
                    {
                        if (move.HasEnded)
                        {
                            return;
                        }
                        else
                        {
                            move.Update(timeDelta);
                        }
                    }
                }
                else
                {
                    attack.Update(timeDelta);
                }
            }
        }

        public bool TryAttack()
        {
            Unit target = attacker.World.Units
                .InArea(attacker.LineOfSight)
                .Where(unit => unit.Faction != attacker.Faction)
                .FirstOrDefault();

            if (target != null) attack = new Attack(attacker, target);
            return target != null;
        }
        #endregion
    }
}
