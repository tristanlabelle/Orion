﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using OpenTK.Math;
using Orion.Engine.Graphics;
using Orion.GameLogic;
using Orion.Geometry;

namespace Orion.Graphics.Renderers
{
    public sealed class BuildingMemoryRenderer
    {
        #region Fields
        private readonly Faction faction;
        private readonly GameGraphics gameGraphics;
        private readonly HashSet<RememberedBuilding> buildings = new HashSet<RememberedBuilding>();
        private readonly HashSet<RememberedBuilding> tempSet = new HashSet<RememberedBuilding>();
        private bool hasVisibilityChanged = false;
        #endregion

        #region Constructors
        public BuildingMemoryRenderer(Faction faction, GameGraphics gameGraphics)
        {
            Argument.EnsureNotNull(faction, "faction");
            Argument.EnsureNotNull(gameGraphics, "gameGraphics");

            this.faction = faction;
            this.gameGraphics = gameGraphics;
            this.faction.VisibilityChanged += OnVisibilityChanged;
            this.faction.World.Entities.Removed += OnEntityRemoved;
        }
        #endregion

        #region Properties
        #endregion

        #region Methods
        public void Draw(GraphicsContext graphics)
        {
            RemoveDeprecatedBuildings();
            foreach (RememberedBuilding building in buildings)
            {
                Texture texture = gameGraphics.GetUnitTexture(building.Type);
                graphics.Fill(building.GridRegion.ToRectangle(), texture, building.Faction.Color);
            }
        }

        public void DrawMiniature(GraphicsContext graphics, Size unitSize)
        {
            RemoveDeprecatedBuildings();
            foreach (RememberedBuilding building in buildings)
            {
                Rectangle rectangle = new Rectangle(building.Location, (Vector2)unitSize);
                graphics.Fill(rectangle, building.Faction.Color);
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

            hasVisibilityChanged = true;
        }

        private void OnEntityRemoved(EntityManager sender, Entity entity)
        {
            Unit unit = entity as Unit;
            if (unit == null) return;

            if (unit.IsBuilding && faction.CanSee(unit))
                buildings.Remove(new RememberedBuilding(unit));
        }

        /// <summary>
        /// Cleans the memory from buildings which are visible and are not what we thought they were.
        /// </summary>
        private void RemoveDeprecatedBuildings()
        {
            if (!hasVisibilityChanged) return;
            hasVisibilityChanged = false;

            buildings.RemoveWhere(rememberedBuilding =>
            {
                if (!faction.CanSee(rememberedBuilding.GridRegion)) return false;

                Unit building = faction.World.Entities.GetEntityAt(rememberedBuilding.Location, CollisionLayer.Ground) as Unit;
                return building == null || !rememberedBuilding.Matches(building);
            });
        }
        #endregion
    }
}
