using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;

namespace Orion.Game.Simulation.IO
{
    public class ResourceNodeTemplate
    {
        #region Fields
        private readonly ResourceType type;
        private readonly Point location;
        private readonly int amountRemaining;
        #endregion

        #region Constructors
        public ResourceNodeTemplate(ResourceType type, Point location, int amountRemaining)
        {
            this.type = type;
            this.location = location;
            this.amountRemaining = amountRemaining
        }
        #endregion

        #region Properties
        public ResourceType ResourceType
        {
            get { return type; }
        }

        public Point Location
        {
            get { return location; }
        }

        public int AmountRemaining
        {
            get { return amountRemaining; }
        }
        #endregion
    }
}
