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
                Region region = new Region(minLocation.Value, prototype.Size);
                if (aladdiumCost > LocalFaction.AladdiumAmount
                    || alageneCost > LocalFaction.AlageneAmount
                    || !LocalFaction.HasFullySeen(region))
                    return false;

                if (!prototype.Components.Has<AlageneExtractor>())
                    return true;

                // Special case for alagene extractors:
                // They can only be build on alagene nodes.
                IEnumerable<Entity> alageneResourceNodes = World.Entities
                    .Intersecting(Rectangle.FromCenterSize(minLocation.Value, Vector2.One))
                    .Where(e => e.Components.Has<Harvestable>())
                    .Where(e => e.Components.Get<Harvestable>().Type == ResourceType.Alagene);

                foreach (Entity entity in alageneResourceNodes)
                {
                    Spatial position = entity.Spatial;
                    if (position.Position == minLocation.Value)
                        return true;
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
            int minX = (int)Math.Round(location.X - prototype.Size.Width * 0.5f);
            int minY = (int)Math.Round(location.Y - prototype.Size.Height * 0.5f);

            if (minX < 0)
                minX = 0;
            else if (minX >= World.Width - prototype.Size.Width)
                minX = World.Width - prototype.Size.Width;

            if (minY < 0)
                minY = 0;
            else if (minY >= World.Height - prototype.Size.Height)
                minY = World.Height - prototype.Size.Height;

            return new Point(minX, minY);
        }

        public void Draw(GraphicsContext context, Rectangle bounds)
        {
            if (!minLocation.HasValue) return;

            ColorRgb tint = IsLocationValid ? Colors.LightBlue : Colors.Red;
            Rectangle rectangle = new Rectangle(
                minLocation.Value.X, minLocation.Value.Y,
                prototype.Size.Width, prototype.Size.Height);
            context.Fill(rectangle, tint.ToRgba(0.4f));
            context.Fill(rectangle, texture, tint);
        }
        #endregion
    }
}
