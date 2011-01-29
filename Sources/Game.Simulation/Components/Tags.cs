using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Game.Simulation.Components.Serialization;

namespace Orion.Game.Simulation.Components
{
    public class Tags : Component
    {
        #region Fields
        private readonly HashSet<string> tags = new HashSet<string>();
        #endregion

        #region Constructors
        public Tags(Entity entity) : base(entity) { }
        #endregion

        #region Properties
        [Persistent]
        public ICollection<string> List
        {
            get { return tags; }
        }
        #endregion
    }
}
