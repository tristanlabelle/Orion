﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.GameLogic;
using Orion.Geometry;
using OpenTK.Math;
using System.Diagnostics;

namespace Orion.Graphics.Renderers
{
    public sealed class BuildingMemoryRenderer
    {
        #region Fields
        private readonly Faction faction;
        private readonly TextureManager textureManager;
        private readonly HashSet<RememberedBuilding> buildings = new HashSet<RememberedBuilding>();
        private readonly HashSet<RememberedBuilding> tempSet = new HashSet<RememberedBuilding>();
        #endregion

        #region Constructors
        public BuildingMemoryRenderer(Faction faction, TextureManager textureManager)
        {
            Argument.EnsureNotNull(faction, "faction");
            Argument.EnsureNotNull(textureManager, "textureManager");

            this.faction = faction;
            this.textureManager = textureManager;
            this.faction.VisibilityChanged += OnVisibilityChanged;
            this.faction.World.Entities.Removed += OnEntityRemoved;
        }

        #endregion

        #region Properties
        #endregion

        #region Methods
        public void Draw(GraphicsContext graphics)
        {
            foreach (RememberedBuilding building in buildings)
            {
                Texture texture = textureManager.GetUnit(building.Type.Name);
                graphics.Fill(building.GridRegion.ToRectangle(), texture, building.Faction.Color);
            }
        }

        public void DrawMiniature(GraphicsContext graphics, Size unitSize)
        {
            foreach (RememberedBuilding building in buildings)
            {
                graphics.FillColor = building.Faction.Color;
                graphics.Fill(new Rectangle(building.Location, (Vector2)unitSize));
            }
        }

        private void OnVisibilityChanged(Faction sender, Region region)
        {
            Debug.Assert(sender == faction);

            tempSet.Clear();
            foreach (Entity entity in faction.World.Entities.Intersecting(region.ToRectangle()))
            {
                Unit unit = entity as Unit;
                if (unit == null || unit.Faction == faction || !unit.IsBuilding || !faction.CanSee(unit))
                    continue;

                tempSet.Add(new RememberedBuilding(unit));
            }
            buildings.UnionWith(tempSet);
            tempSet.Clear();

            buildings.RemoveWhere(rememberedBuilding =>
            {
                if (!faction.CanSee(rememberedBuilding.GridRegion)) return false;

                Unit building = faction.World.Entities.GetEntityAt(rememberedBuilding.Location, CollisionLayer.Ground) as Unit;
                return building == null || !rememberedBuilding.Matches(building);
            });
        }

        private void OnEntityRemoved(EntityManager sender, Entity entity)
        {
            Unit unit = entity as Unit;
            if (unit == null) return;

            if (unit.IsBuilding && faction.CanSee(unit))
                buildings.Remove(new RememberedBuilding(unit));
        }
        #endregion
    }
}
