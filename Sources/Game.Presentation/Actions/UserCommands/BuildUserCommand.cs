using System;
using System.Linq;
using OpenTK;
using Orion.Engine;
using Orion.Engine.Geometry;
using Orion.Engine.Graphics;
using Orion.Game.Matchmaking;
using Orion.Game.Simulation;
using Orion.Game.Simulation.Skills;
using Orion.Game.Simulation.Components;
using System.Collections.Generic;

namespace Orion.Game.Presentation.Actions.UserCommands
{
    public sealed class BuildUserCommand : UserInputCommand, IRenderableUserCommand
    {
        #region Fields
        private readonly Unit buildingType;
        private readonly Texture texture;
        private Point? minLocation;
        #endregion

        #region Constructors
        public BuildUserCommand(UserInputManager inputManager, GameGraphics gameGraphics,
            Unit buildingType)
            : base(inputManager)
        {
            Argument.EnsureNotNull(gameGraphics, "gameGraphics");
            Argument.EnsureNotNull(buildingType, "buildingType");

            this.buildingType = buildingType;
            this.texture = gameGraphics.GetUnitTexture(buildingType);
        }
        #endregion

        #region Properties
        private bool IsLocationValid
        {
            get
            {
                if (!minLocation.HasValue) return false;

                int aladdiumCost = LocalFaction.GetStat(buildingType, BasicSkill.AladdiumCostStat);
                int alageneCost = LocalFaction.GetStat(buildingType, BasicSkill.AlageneCostStat);
                if (aladdiumCost > LocalFaction.AladdiumAmount
                    || alageneCost > LocalFaction.AlageneAmount)
                    return false;

                Region region = new Region(minLocation.Value, buildingType.Size);
                if (!Match.CanBuild(region))
                    return false;

                if (!LocalFaction.HasFullySeen(region))
                    return false;

                if (!buildingType.HasComponent<AlageneExtractor, ExtractAlageneSkill>())
                    return true;

                // Special case for alagene extractors:
                // They can only be build on alagene nodes.
                IEnumerable<Entity> alageneResourceNodes = World.Entities
                    .Intersecting(Rectangle.FromCenterSize(minLocation.Value, Vector2.One))
                    .Where(e => e.Components.Has<Harvestable>())
                    .Where(e => e.Components.Get<Harvestable>().Type == ResourceType.Alagene);

                foreach (Entity entity in alageneResourceNodes)
                {
                    Spatial position = entity.Components.Get<Spatial>();
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

            InputManager.LaunchBuild(minLocation.Value, buildingType);
        }

        private Point GetMinLocation(Vector2 location)
        {
            int minX = (int)Math.Round(location.X - buildingType.Size.Width * 0.5f);
            int minY = (int)Math.Round(location.Y - buildingType.Size.Height * 0.5f);

            if (minX < 0)
                minX = 0;
            else if (minX >= World.Width - buildingType.Size.Width)
                minX = World.Width - buildingType.Size.Width;

            if (minY < 0)
                minY = 0;
            else if (minY >= World.Height - buildingType.Size.Height)
                minY = World.Height - buildingType.Size.Height;

            return new Point(minX, minY);
        }

        public void Draw(GraphicsContext context, Rectangle bounds)
        {
            if (!minLocation.HasValue) return;

            ColorRgb tint = IsLocationValid ? Colors.LightBlue : Colors.Red;
            Rectangle rectangle = new Rectangle(
                minLocation.Value.X, minLocation.Value.Y,
                buildingType.Size.Width, buildingType.Size.Height);
            context.Fill(rectangle, tint.ToRgba(0.4f));
            context.Fill(rectangle, texture, tint);
        }
        #endregion
    }
}
