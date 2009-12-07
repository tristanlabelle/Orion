using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Math;
using Orion.GameLogic;
using Orion.GameLogic.Pathfinding;
using Orion.GameLogic.Tasks;
using Orion.Geometry;
using Color = System.Drawing.Color;

namespace Orion.Graphics
{
    /// <summary>
    /// Provides functionality to draw <see cref="Unit"/>s on-screen.
    /// </summary>
    public sealed class UnitsRenderer
    {
        #region Static
        #region Fields
        private static readonly Color lowLifeColor = Color.Red;
        private static readonly Color middleLifeColor = Color.Yellow;
        private static readonly Color fullLifeColor = Color.ForestGreen;
        private static readonly Size minimapUnitSize = new Size(3, 3);
        private const float shadowAlpha = 0.3f;
        private const float shadowDistance = 0.7f;
        private const float shadowScaling = 0.6f;
        #endregion

        #region Methods
        private static Color Interpolate(Color first, Color second, float completion)
        {
            float opposite = 1 - completion;
            return Color.FromArgb(
                (int)(first.R * opposite + second.R * completion),
                (int)(first.G * opposite + second.G * completion),
                (int)(first.B * opposite + second.B * completion));
        }
        #endregion
        #endregion

        #region Fields
        private readonly World world;
        private bool drawHealthBars;
        private Faction faction;
        private TextureManager textureManager;
        #endregion

        #region Constructors
        public UnitsRenderer(World world, Faction faction, TextureManager textureManager)
        {
            Argument.EnsureNotNull(world, "world");
            Argument.EnsureNotNull(faction, "faction");
            Argument.EnsureNotNull(textureManager, "textureManager");

            this.textureManager = textureManager;
            this.world = world;
            this.faction = faction;
        }
        #endregion

        #region Properties
        public bool DrawHealthBars
        {
            get { return drawHealthBars; }
            set { drawHealthBars = value; }
        }

        private Texture UnderConstructionTexture
        {
            get { return textureManager.Get("UnderConstruction"); }
        }
        #endregion

        #region Methods
        public Texture GetTypeTexture(UnitType type)
        {
            return textureManager.GetUnit(type.Name);
        }

        public void Draw(GraphicsContext graphics)
        {
            Argument.EnsureNotNull(graphics, "graphics");

            DrawUnits(graphics);
        }

        public void DrawMiniature(GraphicsContext context)
        {
            DrawMiniatureUnits(context);
        }

        private void DrawMiniatureUnits(GraphicsContext context)
        {
            foreach (Unit unit in world.Entities.OfType<Unit>())
            {
                if (faction.CanSee(unit))
                {
                    context.FillColor = unit.Faction.Color;
                    context.Fill(new Rectangle(unit.Position, (Vector2)minimapUnitSize));
                }
            }
        }

        private void DrawUnits(GraphicsContext graphics)
        {
            DrawRememberedBuildings(graphics);
            DrawGroundUnits(graphics);
            DrawAirborneUnits(graphics);
        }

        private void DrawRememberedBuildings(GraphicsContext graphics)
        {
            foreach (RememberedBuilding building in faction.BuildingMemory)
                DrawRememberedBuilding(graphics, building);
        }

        private void DrawGroundUnits(GraphicsContext graphics)
        {
            var units = world.Entities.OfType<Unit>().Where(unit => !unit.IsAirborne);
            foreach (Unit unit in units) DrawUnit(graphics, unit);
        }

        private void DrawAirborneUnits(GraphicsContext graphics)
        {
            var units = world.Entities.OfType<Unit>().Where(unit => unit.IsAirborne);
            foreach (Unit unit in units) DrawUnitShadow(graphics, unit);
            foreach (Unit unit in units) DrawUnit(graphics, unit);
        }

        private void DrawUnitShadow(GraphicsContext graphics, Unit unit)
        {
            if (faction.CanSee(unit))
            {
                Texture texture = GetTypeTexture(unit.Type);
                Color tint = Color.FromArgb((int)(shadowAlpha * 255), Color.Black);

                float drawingAngle = GetUnitDrawingAngle(unit);
                Vector2 shadowCenter = unit.Center - new Vector2(shadowDistance, shadowDistance);
                using (graphics.Transform(shadowCenter, drawingAngle, shadowScaling))
                {
                    Rectangle localRectangle = Rectangle.FromCenterSize(0, 0, unit.Width, unit.Height);
                    graphics.Fill(localRectangle, texture, tint);
                }
            }
        }

        private void DrawUnit(GraphicsContext graphics, Unit unit)
        {
            if (faction.CanSee(unit))
            {
                Texture texture = GetTypeTexture(unit.Type);

                float drawingAngle = GetUnitDrawingAngle(unit);
                using (graphics.Transform(unit.Center, drawingAngle))
                {
                    Rectangle localRectangle = Rectangle.FromCenterSize(0, 0, unit.Width, unit.Height);
                    graphics.Fill(localRectangle, texture, unit.Faction.Color);
                    if (unit.IsUnderConstruction)
                        graphics.Fill(localRectangle, UnderConstructionTexture, Color.White);
                }

                if (DrawHealthBars) DrawHealthBar(graphics, unit);
            }
        }

        private void DrawRememberedBuilding(GraphicsContext graphics, RememberedBuilding building)
        {
            Texture texture = GetTypeTexture(building.Type);

            using (graphics.Translate(building.GridRegion.ToRectangle().Center))
            {
                Rectangle localRectangle = Rectangle.FromCenterSize(0, 0,
                    building.Type.Size.Width, building.Type.Size.Height);
                graphics.Fill(localRectangle, texture, building.Faction.Color);
            }
        }

        private static float GetUnitDrawingAngle(Unit unit)
        {
            // Workaround the fact that our unit textures face up,
            // and building textures are not supposed to be rotated.
            return unit.IsBuilding ? 0 : unit.Angle - (float)Math.PI * 0.5f;
        }

        public void DrawHealthBar(GraphicsContext context, Unit unit)
        {
            float healthbarWidth = (float)Math.Log(unit.MaxHealth);
            Rectangle unitBoundingRectangle = unit.BoundingRectangle;
            float y = unitBoundingRectangle.CenterY + unitBoundingRectangle.Height * 0.75f;
            float x = unitBoundingRectangle.CenterX - healthbarWidth / 2f;

            if (faction.CanSee(unit))
                DrawHealthBar(context, unit, new Vector2(x, y));
        }

        public void DrawHealthBar(GraphicsContext context, Unit unit, Vector2 origin)
        {
            float healthbarWidth = (float)Math.Log(unit.MaxHealth);
            float leftHealthWidth = unit.Health * 0.1f;

            if (faction.CanSee(unit))
                DrawHealthBar(context, unit, new Rectangle(origin, new Vector2(healthbarWidth, 0.15f)));
        }

        public void DrawHealthBar(GraphicsContext context, Unit unit, Rectangle into)
        {
            float leftHealthWidth = into.Width * (unit.Health / unit.MaxHealth);
            Vector2 origin = into.Min;

            if (faction.CanSee(unit))
            {
                if (unit.Health > unit.MaxHealth / 2)
                {
                    float lifeFraction = (unit.Health - unit.MaxHealth / 2f) / (unit.MaxHealth / 2f);
                    context.FillColor = Interpolate(middleLifeColor, fullLifeColor, lifeFraction);
                }
                else
                {
                    float lifeFraction = unit.Health / (unit.MaxHealth / 2f);
                    context.FillColor = Interpolate(lowLifeColor, middleLifeColor, lifeFraction);
                }

                Rectangle lifeRect = new Rectangle(origin.X, origin.Y, leftHealthWidth, into.Height);
                context.Fill(lifeRect);
                Rectangle damageRect = new Rectangle(origin.X + leftHealthWidth, origin.Y, into.Width - leftHealthWidth, into.Height);
                context.FillColor = Color.DarkGray;
                context.Fill(damageRect);
            }
        }

        private void DrawPaths(GraphicsContext graphics)
        {
            var paths = world.Entities
                .OfType<Unit>()
                .Select(unit => unit.TaskQueue.Current)
                .OfType<MoveTask>()
                .Select(task => task.Path)
                .Where(path => path != null);

            graphics.StrokeColor = Color.Gray;
            foreach (Path path in paths)
                graphics.StrokeLineStrip(path.Points.Select(point => (Vector2)point + new Vector2(0.5f, 0.5f)));
        }

        private void DrawPathfindingDebugInfo(GraphicsContext graphics, Pathfinder pathfinder)
        {
            graphics.StrokeColor = Color.Yellow;
            foreach (PathNode node in pathfinder.ClosedNodes)
                if (node.Source != null)
                    graphics.StrokeLineStrip(node.Source.Point, node.Point);

            graphics.StrokeColor = Color.Lime;
            foreach (PathNode node in pathfinder.OpenNodes)
                if (node.Source != null)
                    graphics.StrokeLineStrip(node.Source.Point, node.Point);
        }

        private void DrawAttackLines(GraphicsContext graphics)
        {
            var attacks = world.Entities
                .OfType<Unit>()
                .Select(unit => unit.TaskQueue.Current)
                .OfType<AttackTask>();

            graphics.StrokeColor = Color.Orange;
            foreach (AttackTask attack in attacks)
                graphics.StrokeLineStrip(attack.Attacker.Position, attack.Target.Position);
        }
        #endregion
    }
}
