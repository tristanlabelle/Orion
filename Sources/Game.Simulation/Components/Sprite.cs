using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Game.Simulation.Components.Serialization;

namespace Orion.Game.Simulation.Components
{
    /// <summary>
    /// A <see cref="Component"/> which provides a visual representation
    /// of its host <see cref="Entity"/> as a sprite.
    /// </summary>
    public sealed class Sprite : Component
    {
        #region Fields
        private string name;
        private bool rotates = true;
        #endregion

        #region Constructors
        public Sprite(Entity entity) : base(entity) { }
        #endregion

        #region Properties
        /// <summary>
        /// Accesses the name of the sprite to be used.
        /// </summary>
        [Persistent]
        public string Name
        {
            get
            {
                if (name != null) return name;

                Identity identity = Entity.Identity;
                return identity == null ? null : identity.Name;
            }
            set { name = value; }
        }

        /// <summary>
        /// Accesses a value indicating if the sprite should be rotated with the entity.
        /// </summary>
        [Persistent]
        public bool Rotates
        {
            get { return rotates; }
            set { rotates = value; }
        }
        #endregion

        #region Methods
        public override int GetStateHashCode()
        {
            return 0;
        }
        #endregion
    }
}
