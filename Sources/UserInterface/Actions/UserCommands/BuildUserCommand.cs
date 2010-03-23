using System;
using System.Linq;
using OpenTK.Math;
using Orion.Engine;
using Orion.Engine.Geometry;
using Orion.Engine.Graphics;
using Orion.Engine.Gui;
using Orion.Game.Simulation;
using Orion.Game.Presentation;
using Orion.Game.Presentation.Renderers;
using Orion.Game.Matchmaking;

namespace Orion.Game.Presentation.Actions.UserCommands
{
    public sealed class BuildUserCommand : UserInputCommand, IViewRenderer
    {
        #region Fields
        private readonly UnitType buildingType;
        private readonly Texture texture;
        private Point? minLocation;
        #endregion

        #region Constructors
        public BuildUserCommand(UserInputManager inputManager, GameGraphics gameGraphics,
            UnitType buildingType)
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

                int aladdiumCost = LocalFaction.GetStat(buildingType, UnitStat.AladdiumCost);
                int alageneCost = LocalFaction.GetStat(buildingType, UnitStat.AlageneCost);
                if (aladdiumCost > LocalFaction.AladdiumAmount
                    || alageneCost > LocalFaction.AlageneAmount)
                    return false;

                Region region = new Region(minLocation.Value, buildingType.Size);
                if (!World.IsFree(region, CollisionLayer.Ground))
                    return false;

                if (!LocalFaction.HasFullySeen(region))
                    return false;

                if (!buildingType.HasSkill(UnitSkill.ExtractAlagene))
                    return true;

                // Special case for alagene extractors:
                // They can only be build on alagene nodes.
                return World.Entities
                    .OfType<ResourceNode>()
                    .Any(node => node.Type == ResourceType.Alagene
                        && (Point)node.Position == minLocation.Value);
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
            int minX = (int)Math.Round(location.X - buildingType.Width * 0.5f);
            int minY = (int)Math.Round(location.Y - buildingType.Height * 0.5f);

            if (minX < 0)
                minX = 0;
            else if (minX >= World.Width - buildingType.Width)
                minX = World.Width - buildingType.Width;

            if (minY < 0)
                minY = 0;
            else if (minY >= World.Height - buildingType.Height)
                minY = World.Height - buildingType.Height;

            return new Point(minX, minY);
        }

        public void Draw(GraphicsContext context, Rectangle bounds)
        {
            if (!minLocation.HasValue) return;

            ColorRgb tint = IsLocationValid ? Colors.LightBlue : Colors.Red;
            Rectangle rectangle = new Rectangle(
                minLocation.Value.X, minLocation.Value.Y,
                buildingType.Width, buildingType.Height);
            context.Fill(rectangle, tint.ToRgba(0.4f));
            context.Fill(rectangle, texture, tint);
        }
        #endregion
    }
}
