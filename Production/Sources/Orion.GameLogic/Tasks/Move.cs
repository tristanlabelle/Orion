using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK.Math;

namespace Orion.GameLogic.Tasks
{
    /// <summary>
    /// A <see cref="Task"/>, which makes a <see cref="Unit"/> move to a specified destination.
    /// </summary>
    [Serializable]
    public sealed class Move : Task
    {
        #region Fields
        private readonly Unit unit;
        private readonly Vector2 destination;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new <see cref="Move"/> task from the <see cref="Unit"/>
        /// that gets moved and its destination.
        /// </summary>
        /// <param name="unit">The <see cref="Unit"/> to be moved.</param>
        /// <param name="destination">The location of the unit'</param>
        public Move(Unit unit, Vector2 destination)
        {
            Argument.EnsureNotNull(unit, "unit");

            this.unit = unit;
            this.destination = destination;
            this.unit.World.PathFinder.FindPath(unit.Position, destination);
            
        }
        #endregion

        #region Properties
        public override bool HasEnded
        {
            get { return (unit.Position - destination).Length <= 0.01f; }
        }

        public override string Description
        {
            get { return "moving to {0}".FormatInvariant(destination); }
        }
        #endregion

        #region Methods

        public override void Update(float timeDelta)
        {
            if (HasEnded) return;

            Vector2 delta = destination - unit.Position;
            Vector2 direction = Vector2.Normalize(delta);

            float distance = unit.Type.MovementSpeed * timeDelta;
            if (distance < delta.Length) unit.Position += direction * distance;
            else unit.Position = destination;

        }
        #endregion
    }
}
        