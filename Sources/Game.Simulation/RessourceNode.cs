using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK.Math;
using Orion.Geometry;

namespace Orion.GameLogic
{
    public enum RessourceType
        {
            Alladium = 1,
            Allagene = 2
        }

    public class RessourceNode
    {
        #region Fields

        private readonly int id;
        private readonly RessourceType ressourceType;
        private readonly int totalRessources;
        private int ressourcesLeft;
        private readonly Vector2 position;
        private World world;
        private bool harvestable;

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="ressourceType">Can only take the values "Allagene" or "Alladium" or the node will not be displayed</param>
        /// <param name="amountOfRessources"></param>
        /// <param name="position"></param>
        public RessourceNode(int id, RessourceType ressourceType, int amountOfRessources, Vector2 position, World world)
        {
            this.id = id;
            this.ressourceType = ressourceType;
            this.totalRessources = amountOfRessources;
            this.ressourcesLeft = amountOfRessources;
            this.position = position;
            this.world = world;

            switch (ressourceType)
            {
                case RessourceType.Alladium:
                    harvestable = true;
                    break;
                case RessourceType.Allagene:
                    harvestable = false;
                    break;
            }
        }
        #endregion

        #region Properties
        public RessourceType RessourceType
        {
            get { return ressourceType; }
        }

        public int TotalRessources
        {
            get { return totalRessources; }
        }

        public int RessourcesLeft
        {
            get { return ressourcesLeft; }
            set { ressourcesLeft = value; }
        }

        public Vector2 Position
        {
            get { return position; }
        }

        public Circle Circle
        {
            get { return new Circle(position, 2); }
        }

        public bool IsHarvestable
        {
            get { return harvestable; }
        }

        #endregion

        #region Methods

        public void Harvest(int amount)
        {
            ressourcesLeft -= amount;
            if (ressourcesLeft == 0)
            {
                world.RessourceNodes.Remove(this);
            }
        }

        #endregion
    }
}
