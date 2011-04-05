using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Engine.Collections;
using Orion.Game.Simulation;
using OpenTK;
using Orion.Game.Simulation.Components;

namespace Orion.Game.Simulation
{
    /// <summary>
    /// Generates simulation worlds.
    /// </summary>
    public abstract class WorldGenerator
    {
        #region Methods
        public abstract Terrain GenerateTerrain();
        public abstract void PrepareWorld(World world, PrototypeRegistry prototypes);

        protected Entity CreateResourceNode(World world, PrototypeRegistry prototypes, ResourceType type, Point location)
        {
            Entity entity = world.Entities.CreateEmpty();

            entity.SpecializeWithPrototype(prototypes.FromResourceType(type));
            entity.Spatial.Position = location;
            world.Entities.Add(entity);

            return entity;
        }
        #endregion
    }
}
