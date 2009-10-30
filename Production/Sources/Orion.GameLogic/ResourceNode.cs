
using OpenTK.Math;
using Orion.Geometry;

namespace Orion.GameLogic
{
    public enum ResourceType
    {
        Alladium = 1,
        Alagene = 2
    }

    public class ResourceNode
    {
        #region Fields

        private readonly int id;
        private readonly ResourceType resourceType;
        private readonly int totalResources;
        private int resourcesLeft;
        private readonly Vector2 position;
        private World world;
        private bool isHarvestable = true;

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="resourceType">Can only take the values "Allagene" or "Alladium" or the node will not be displayed</param>
        /// <param name="amountOfResources"></param>
        /// <param name="position"></param>
        public ResourceNode(int id, ResourceType resourceType, int amountOfResources, Vector2 position, World world)
        {
            this.id = id;
            this.resourceType = resourceType;
            this.totalResources = amountOfResources;
            this.resourcesLeft = amountOfResources;
            this.position = position;
            this.world = world;
            if (resourceType == ResourceType.Alagene)
                isHarvestable = false;
        }
        #endregion

        #region Properties
        public ResourceType ResourceType
        {
            get { return resourceType; }
        }

        public int TotalResources
        {
            get { return totalResources; }
        }

        public int ResourcesLeft
        {
            get { return resourcesLeft; }
            set { resourcesLeft = value; }
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
            get { return isHarvestable; }
            set { this.isHarvestable = value; }
        }
        public int ID
        {
            get { return id; }
        }

        #endregion

        #region Methods

        public void Harvest(int amount)
        {
            resourcesLeft -= amount;
            if (resourcesLeft == 0)
            {
                world.ResourceNodes.Remove(this);
            }
        }

        #endregion
    }
}
