using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK.Math;

namespace Orion.GameLogic
{
    /*public enum BuildingType
    {
        CommandCenter = 1,
        Barracks = 2,
        Extractor = 3,
        ResearchCenter = 4,
        PolygonFarm = 5,
        DefenseTower = 6,
        AerialDeploymentCenter = 7
    }*/

    public class Building
    {
        #region Fields
        private readonly int maxHealthPoints;
        private int healthPoints;
        private World world;
        private readonly Vector2 position;
        #endregion

        #region Constructors
        public Building(int maxHealthPoints, Vector2 position, World world)
        {
            this.maxHealthPoints = maxHealthPoints;
            healthPoints = maxHealthPoints;
            this.world = world;
            this.position = position;
        }
        #endregion

        #region Properties
        public int MaxHealthPoints
        {
            get { return maxHealthPoints; }
        }

        public int HealthPoints
        {
            get { return healthPoints; }
        }

        public Vector2 Position
        {
            get { return position; }
        }
        #endregion

        #region Methods
        public void Damage(int damageToInflict)
        {
            if (healthPoints - damageToInflict <= 0)
                Die();
            else
                healthPoints -= damageToInflict;
        }

        public void Repair(int amountRepaired)
        {
            if (healthPoints + amountRepaired <= maxHealthPoints)
                healthPoints += amountRepaired;
            else
                healthPoints = maxHealthPoints;
        }

        public void Die()
        {
            world.Buildings.Remove(this);
        }
        #endregion
    }
}
