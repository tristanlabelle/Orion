using System;
using System.Linq;
using OpenTK.Math;
using Orion.Matchmaking;
using Orion.GameLogic;
using Orion.GameLogic.Skills;
using Orion.Graphics;
using Orion.Geometry;

namespace Orion.UserInterface.Actions.UserCommands
{
    public sealed class BuildUserCommand : UserInputCommand, IRenderer
    {
        #region Fields
        private readonly TextureManager textureManager;
        private readonly UnitType buildingType;
        private Point? minLocation;
        #endregion

        #region Constructors
        public BuildUserCommand(UserInputManager inputManager, TextureManager textureManager,
            UnitType buildingType)
            : base(inputManager)
        {
            Argument.EnsureNotNull(textureManager, "textureManager");
            Argument.EnsureNotNull(buildingType, "buildingType");

            this.textureManager = textureManager;
            this.buildingType = buildingType;
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

                if (!buildingType.HasSkill<ExtractAlageneSkill>())
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

        public void Draw(GraphicsContext context)
        {
            if (!minLocation.HasValue) return;

            Texture texture = textureManager.GetUnit(buildingType.Name);
            ColorRgb tint = IsLocationValid ? Colors.LightBlue : Colors.Red;
            Rectangle rectangle = new Rectangle(
                minLocation.Value.X, minLocation.Value.Y,
                buildingType.Width, buildingType.Height);
            context.FillColor = new ColorRgba(tint, 0.4f);
            context.Fill(rectangle);
            context.Fill(rectangle, texture, tint);
        }
        #endregion
    }
}
