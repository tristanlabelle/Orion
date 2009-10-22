using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK.Math;
using Orion.Geometry;

namespace Orion.GameLogic.Tasks
{
    /// <summary>
    /// A <see cref="Task"/> which makes a<see cref="Unit"/> move to a location and attack enemies on it's way.
    /// </summary>
    public sealed class ZoneAttack : Task
    {
        #region Fields

        private readonly Unit striker;
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
            
            this.striker = striker;
            this.destination = destination;
            this.targetDistance = striker.Type.AttackRange;
            this.move = new Move(striker, destination);
        }

        #endregion

        #region Properties

        public override bool HasEnded
        {
            get
            {
                if (move.HasEnded)
                    if (attack == null)
                        return true;
                    else
                        if (attack.HasEnded)
                            return true;
                return false;
            }
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
            get { return Circle.SignedDistance(striker.Circle, target.Circle); }
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
                // Si personne dans le range
                if (!TryAttack())
                {
                    // si yé arrivé à destination
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
                // Si attaque Fonction
            else
            {
                // Si terminée
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
                    //si Non terminée, on update attaque
                else
               {
                        attack.Update(timeDelta);
                }
            }
        }

        public bool TryAttack()
        {
            foreach (Unit enemy in striker.Faction.World.Units.Where(unit => unit.Faction != striker.Faction))
            {
                target = enemy;

                if (IsInRange)
                {
                    attack = new Attack(striker, target);
                    return true;
                }
            }
            return false;
        }

        #endregion
    }
}
