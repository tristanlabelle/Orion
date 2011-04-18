using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using OpenTK;
using Orion.Engine;
using Orion.Engine.Geometry;
using Orion.Engine.Graphics;
using Orion.Game.Simulation;
using Orion.Game.Simulation.Utilities;

namespace Orion.Game.Presentation.Renderers
{
    /// <summary>
    /// Draws the buildings in the fog of war which have already been seen but have become hidden.
    /// </summary>
    public sealed class BuildingMemoryRenderer
    {
        #region Fields
        private readonly FogOfWarMemory memory;
        private readonly GameGraphics gameGraphics;
        #endregion

        #region Constructors
        public BuildingMemoryRenderer(Faction faction, GameGraphics gameGraphics)
        {
            Argument.EnsureNotNull(faction, "faction");
            Argument.EnsureNotNull(gameGraphics, "gameGraphics");

            this.memory = new FogOfWarMemory(faction);
            this.gameGraphics = gameGraphics;
        }
        #endregion

        #region Properties
        #endregion

        #region Methods
        public void Draw(GraphicsContext graphics)
        {
            foreach (RememberedEntity building in memory.Entities)
            {
                Texture texture = gameGraphics.GetEntityTexture(building.Prototype);
                graphics.Fill(building.GridRegion.ToRectangle(), texture, building.Faction.Color);
            }
        }

        public void DrawMiniature(GraphicsContext graphics, Size unitSize)
        {
            foreach (RememberedEntity building in memory.Entities)
            {
                Rectangle rectangle = new Rectangle(building.Position, (Vector2)unitSize);
                graphics.Fill(rectangle, building.Faction.Color);
            }
        }
        #endregion
    }
}
