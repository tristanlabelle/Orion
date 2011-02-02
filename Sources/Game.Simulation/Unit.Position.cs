using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using Orion.Engine;
using Orion.Game.Simulation.Components;
using Orion.Game.Simulation.Skills;

namespace Orion.Game.Simulation
{
    public partial class Unit : Entity
    {
        private void InitPosition(Vector2 position, Size size, CollisionLayer layer)
        {
            Spatial spatial = new Spatial(this);
            spatial.Position = position;
            spatial.Size = size;
            spatial.CollisionLayer = layer;
            spatial.SightRange = Type.GetBaseStat(BasicSkill.SightRangeStat);
            AddComponent(spatial);
        }

        public new Vector2 Position
        {
            get { return GetComponent<Spatial>().Position; }
            set { GetComponent<Spatial>().Position = value; }
        }

        public override Size Size
        {
            get { return GetComponent<Spatial>().Size; }
        }

        public float Angle
        {
            get { return GetComponent<Spatial>().Angle; }
            set { GetComponent<Spatial>().Angle = value; }
        }
    }
}
