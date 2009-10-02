using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.GameLogic.Tasks
{
    /// <summary>
    /// The default <see cref="Task"/>, which consists in doing nothing.
    /// </summary>
    [Serializable]
    public sealed class Stand : Task
    {
        #region Instance
        #region Properties
        public override bool HasEnded
        {
            get
            {
                // This command acts as a null object, so the unit executing it is always idle.
                return true;
            }
        }

        public override string Description
        {
            get { return "standing"; }
        }
        #endregion

        #region Methods
        public override void Update(float timeDelta) { 
        }
        #endregion
        #endregion

        #region Static
        #region Fields
        private static readonly Stand instance = new Stand();
        #endregion

        #region Properties
        /// <summary>
        /// Gets the default instance of this class.
        /// </summary>
        public static Stand Instance
        {
            get { return instance; }
        }
        #endregion
        #endregion
    }
}
