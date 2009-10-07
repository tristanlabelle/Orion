using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK.Math;

namespace Orion.GameLogic
{
    class RessourceNode
    {
        #region Fields

        private readonly int id;
        private readonly string ressourceType;
        private readonly int totalRessources;
        private int ressourcesLeft;
        private readonly Vector2 position;
        private readonly World world;

        #endregion

        #region Constructors
        public RessourceNode(int id, string ressourceType, int amountOfRessources, Vector2 position, World world)
        {
            this.id = id;
            this.ressourceType = ressourceType;
            this.totalRessources = amountOfRessources;
            this.ressourcesLeft = amountOfRessources;
            this.position = position;
            this.world = world;
        }
        #endregion

        #region Properties
        public string RessourceType
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

        #endregion

        #region Methods

        public void Harvest(int amount)
        {
            ressourcesLeft -= amount;
        }

        #endregion
    }
}
