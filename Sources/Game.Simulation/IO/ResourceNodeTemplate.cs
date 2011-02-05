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
        private readonly int remainingAmount;
        #endregion

        #region Constructors
        public ResourceNodeTemplate(ResourceType type, Point location, int remainingAmount)
        {
            this.type = type;
            this.location = location;
            this.remainingAmount = remainingAmount;
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

        public int RemainingAmount
        {
            get { return remainingAmount; }
        }
        #endregion
    }
}
