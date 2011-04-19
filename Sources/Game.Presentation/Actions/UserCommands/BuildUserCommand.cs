using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;
using Orion.Engine;
using Orion.Engine.Geometry;
using Orion.Engine.Graphics;
using Orion.Game.Matchmaking;
using Orion.Game.Simulation;
using Orion.Game.Simulation.Components;
using System.Diagnostics;

namespace Orion.Game.Presentation.Actions.UserCommands
{
    public sealed class BuildUserCommand : UserInputCommand, IRenderableUserCommand
    {
        #region Fields
        private readonly Entity prototype;
        private readonly Texture texture;
        private Point? minLocation;
        #endregion

        #region Constructors
        public BuildUserCommand(UserInputManager inputManager, GameGraphics gameGraphics,
            Entity prototype)
            : base(inputManager)
        {
            Argument.EnsureNotNull(gameGraphics, "gameGraphics");
            Argument.EnsureNotNull(prototype, "prototype");

            this.prototype = prototype;
            this.texture = gameGraphics.GetEntityTexture(prototype);

            Debug.Assert(prototype.Components.Has<Spatial>(), "Cannot position a building without a spatial component.");
        }
        #endregion

        #region Properties
        private bool IsLocationValid
        {
            get
            {
                if (!minLocation.HasValue) return false;

                int aladdiumCost = (int)LocalFaction.GetStat(prototype, Cost.AladdiumStat);
                int alageneCost = (int)LocalFaction.GetStat(prototype, Cost.AlageneStat);

                Region region = new Region(minLocation.Value, prototype.Spatial.Size);
                if (aladdiumCost > LocalFaction.AladdiumAmount
                    || alageneCost > LocalFaction.AlageneAmount
                    || !LocalFaction.HasFullySeen(region)
                    || !World.IsFree(region, CollisionLayer.Ground))
                    return false;

                if (!prototype.Components.Has<AlageneExtractor>())
                    return true;

                // Special case for alagene extractors:
                // They can only be build on alagene nodes.
                foreach (Spatial entitySpatial in World.SpatialManager.Intersecting(Rectangle.FromCenterSize(minLocation.Value, Vector2.One)))
                {
                    Entity entity = entitySpatial.Entity;
                    Harvestable harvestable = entity.Components.TryGet<Harvestable>();
                    if (harvestable != null
                        && harvestable.Type == ResourceType.Alagene
                        && entitySpatial.Position == minLocation.Value)
                    {
                        return true;
                    }
                }
                
                return false;
            }
        }
        #endregion

        #region Methods
        public override void OnMouseMoved(Vector2 location)
        {
            minLocation = GetMinLocation(location);
        }

        public override void OnClick(Vector2 location)
        {
            OnMouseMoved(location);
            if (!IsLocationValid) return;

            InputManager.LaunchBuild(minLocation.Value, prototype);
        }

        private Point GetMinLocation(Vector2 location)
        {
            Size size = prototype.Spatial.Size;

            int minX = (int)Math.Round(location.X - size.Width * 0.5f);
            int minY = (int)Math.Round(location.Y - size.Height * 0.5f);

            if (minX < 0)
                minX = 0;
            else if (minX >= World.Width - size.Width)
                minX = World.Width - size.Width;

            if (minY < 0)
                minY = 0;
            else if (minY >= World.Height - size.Height)
                minY = World.Height - size.Height;

            return new Point(minX, minY);
        }

        public void Draw(GraphicsContext context, Rectangle bounds)
        {
            if (!minLocation.HasValue) return;

            Size size = prototype.Spatial.Size;
            ColorRgb tint = IsLocationValid ? Colors.LightBlue : Colors.Red;
            Rectangle rectangle = new Rectangle(
                minLocation.Value.X, minLocation.Value.Y,
                size.Width, size.Height);
            context.Fill(rectangle, tint.ToRgba(0.4f));
            context.Fill(rectangle, texture, tint);
        }
        #endregion
    }
}
