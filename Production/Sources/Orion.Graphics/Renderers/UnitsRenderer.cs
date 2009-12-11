using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Math;
using Orion.GameLogic;
using Orion.GameLogic.Pathfinding;
using Orion.GameLogic.Tasks;
using Orion.Geometry;
using Color = System.Drawing.Color;

namespace Orion.Graphics.Renderers
{
    /// <summary>
    /// Provides functionality to draw <see cref="Unit"/>s on-screen.
    /// </summary>
    public sealed class UnitsRenderer : IRenderer
    {
        #region Fields
        private static readonly Size miniatureUnitSize = new Size(3, 3);

        private const float shadowAlpha = 0.3f;
        private const float shadowDistance = 0.7f;
        private const float shadowScaling = 0.6f;

        private const float meleeHitSpinTimeInSeconds = 0.25f;
        private const float rangedShootTimeInSeconds = 0.25f;
        private const float laserLength = 0.8f;

        private readonly Faction faction;
        private readonly TextureManager textureManager;
        private readonly Pool<Ruin> ruinPool = new Pool<Ruin>();
        private readonly List<Ruin> ruins = new List<Ruin>();
        private float simulationTimeInSeconds;
        private bool drawHealthBars;
        #endregion

        #region Constructors
        public UnitsRenderer(Faction faction, TextureManager textureManager)
        {
            Argument.EnsureNotNull(faction, "faction");
            Argument.EnsureNotNull(textureManager, "textureManager");
            
            this.faction = faction;
            this.textureManager = textureManager;

            World.Updated += OnWorldUpdated;
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

        private Texture BuildingRuinTexture
        {
            get { return textureManager.GetUnit("Ruins"); }
        }

        private Texture SkeletonTexture
        {
            get { return textureManager.GetUnit("Skeleton"); }
        }

        private World World
        {
            get { return faction.World; }
        }
        #endregion

        #region Methods
        private void OnWorldUpdated(World sender, SimulationStep args)
        {
            simulationTimeInSeconds = args.TimeInSeconds;
        }

        public void Draw(GraphicsContext graphics)
        {
            Argument.EnsureNotNull(graphics, "graphics");

            DrawRememberedBuildings(graphics);
            DrawGroundUnits(graphics);
            DrawLasers(graphics, CollisionLayer.Ground);
            DrawAirborneUnits(graphics);
            DrawLasers(graphics, CollisionLayer.Air);
        }

        #region Miniature
        public void DrawMiniature(GraphicsContext context)
        {
            DrawMiniatureUnits(context);
        }

        private void DrawMiniatureUnits(GraphicsContext context)
        {
            foreach (Unit unit in World.Entities.OfType<Unit>())
            {
                if (faction.CanSee(unit))
                {
                    context.FillColor = unit.Faction.Color;
                    context.Fill(new Rectangle(unit.Position, (Vector2)miniatureUnitSize));
                }
            }

            foreach (RememberedBuilding building in faction.BuildingMemory)
            {
                context.FillColor = building.Faction.Color;
                context.Fill(new Rectangle(building.Location, (Vector2)miniatureUnitSize));
            }
        }
        #endregion

        #region Units
        private void DrawRememberedBuildings(GraphicsContext graphics)
        {
            foreach (RememberedBuilding building in faction.BuildingMemory)
                DrawRememberedBuilding(graphics, building);
        }

        private IEnumerable<Unit> GetClippedVisibleUnits(Rectangle clippingBounds)
        {
            return World.Entities
                .OfType<Unit>()
                .Where(unit => Rectangle.Intersects(clippingBounds, unit.BoundingRectangle)
                    && faction.CanSee(unit));
        }

        private void DrawGroundUnits(GraphicsContext graphics)
        {
            var units = GetClippedVisibleUnits(graphics.CoordinateSystem)
                .Where(unit => !unit.IsAirborne);
            foreach (Unit unit in units) DrawUnit(graphics, unit);
        }

        private void DrawAirborneUnits(GraphicsContext graphics)
        {
            var units = GetClippedVisibleUnits(graphics.CoordinateSystem)
                .Where(unit => unit.IsAirborne);
            foreach (Unit unit in units) DrawUnitShadow(graphics, unit);
            foreach (Unit unit in units) DrawUnit(graphics, unit);
        }

        private void DrawUnit(GraphicsContext graphics, Unit unit)
        {
            Texture texture = textureManager.GetUnit(unit.Type.Name);

            Vector2 center = unit.Center;
            center.Y += GetOscillation(unit) * 0.15f;

            float drawingAngle = GetUnitDrawingAngle(unit);
            using (graphics.Transform(center, drawingAngle))
            {
                Rectangle localRectangle = Rectangle.FromCenterSize(0, 0, unit.Width, unit.Height);
                graphics.Fill(localRectangle, texture, unit.Faction.Color);
                if (unit.IsUnderConstruction)
                    graphics.Fill(localRectangle, UnderConstructionTexture, Color.White);
            }

            if (DrawHealthBars) HealthBarRenderer.Draw(graphics, unit);
        }

        private void DrawUnitShadow(GraphicsContext graphics, Unit unit)
        {
            Texture texture = textureManager.GetUnit(unit.Type.Name);
            Color tint = Color.FromArgb((int)(shadowAlpha * 255), Color.Black);

            float drawingAngle = GetUnitDrawingAngle(unit);
            float oscillation = GetOscillation(unit);
            float distance = shadowDistance + oscillation * 0.1f;
            Vector2 center = unit.Center - new Vector2(distance, distance);
            float scaling = shadowScaling + oscillation * -0.1f;
            using (graphics.Transform(center, drawingAngle, scaling))
            {
                Rectangle localRectangle = Rectangle.FromCenterSize(0, 0, unit.Width, unit.Height);
                graphics.Fill(localRectangle, texture, tint);
            }
        }

        private void DrawRememberedBuilding(GraphicsContext graphics, RememberedBuilding building)
        {
            Texture texture = textureManager.GetUnit(building.Type.Name);

            Rectangle buildingRectangle = building.GridRegion.ToRectangle();
            if (!Rectangle.Intersects(buildingRectangle, graphics.CoordinateSystem))
                return;

            graphics.Fill(buildingRectangle, texture, building.Faction.Color);
        }

        private float GetOscillation(Unit unit)
        {
            if (!unit.IsAirborne) return 0;

            float period = 3 + unit.Size.Area / 4.0f;
            float offset = (unit.Handle.Value % 8) / 8.0f * period;
            float progress = ((simulationTimeInSeconds + offset) % period) / period;
            float sineAngle = (float)Math.PI * 2 * progress;
            float sine = (float)Math.Sin(sineAngle);

            return sine;
        }

        private static float GetUnitDrawingAngle(Unit unit)
        {
            // Workaround the fact that our unit textures face up,
            // and building textures are not supposed to be rotated.
            if (unit.IsBuilding) return 0;

            float baseAngle = unit.Angle - (float)Math.PI * 0.5f;
            bool isMelee = unit.GetStat(UnitStat.AttackRange) == 0;
            if (!isMelee || unit.TimeElapsedSinceLastHitInSeconds > meleeHitSpinTimeInSeconds)
                return baseAngle;

            float spinProgress = unit.TimeElapsedSinceLastHitInSeconds / meleeHitSpinTimeInSeconds;
            float spinAngle = spinProgress * (float)Math.PI * 2;

            return baseAngle + spinAngle;
        }
        #endregion

        #region Debug
        private void DrawPaths(GraphicsContext graphics)
        {
            var paths = World.Entities
                .OfType<Unit>()
                .Select(unit => unit.TaskQueue.Current)
                .OfType<MoveTask>()
                .Select(task => task.Path)
                .Where(path => path != null);

            graphics.StrokeColor = Color.Gray;
            foreach (Path path in paths)
            {
                var points = path.Points;
                LineSegment segment = new LineSegment(points[0], points[points.Count - 1]);

                graphics.StrokeColor = Color.Blue;
                graphics.StrokeLineStrip(
                    segment.EndPoint1 + new Vector2(0.5f, 0.5f),
                    segment.EndPoint2 + new Vector2(0.5f, 0.5f));

                foreach (var pair in XiaolinWu.GetPoints(segment))
                {
                    graphics.StrokeColor = Color.Gray;
                    graphics.Stroke(new Rectangle(pair.Key.X, pair.Key.Y, 1, 1));
                }

                graphics.StrokeColor = Color.Red;
                graphics.StrokeLineStrip(path.Points.Select(point => (Vector2)point + new Vector2(0.5f, 0.5f)));
            }
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

        private void DrawLasers(GraphicsContext graphics, CollisionLayer layer)
        {
            var attackTasks = World.Entities
                .OfType<Unit>()
                .Where(unit => unit.CollisionLayer == layer)
                .Select(unit => unit.TaskQueue.Current as AttackTask)
                .Where(task => task != null);

            foreach (AttackTask attackTask in attackTasks)
            {
                Unit attacker = attackTask.Unit;
                bool isRanged = attacker.GetStat(UnitStat.AttackRange) > 0;
                if (!isRanged || attacker.TimeElapsedSinceLastHitInSeconds > rangedShootTimeInSeconds)
                    continue;

                Unit target = attackTask.Target;
                if (!Rectangle.Intersects(attacker.BoundingRectangle, graphics.CoordinateSystem)
                    && !Rectangle.Intersects(target.BoundingRectangle, graphics.CoordinateSystem))
                    continue;

                float laserProgress = attacker.TimeElapsedSinceLastHitInSeconds / meleeHitSpinTimeInSeconds;

                Vector2 delta = target.Center - attacker.Center;
                Vector2 normalizedDelta = Vector2.Normalize(delta);
                float distance = delta.LengthFast;

                Vector2 laserStart = attacker.Center + (normalizedDelta
                    * Math.Max(0, laserProgress * distance - laserLength * 0.5f));
                Vector2 laserEnd = attacker.Center + (normalizedDelta
                    * Math.Min(distance, laserProgress * distance + laserLength * 0.5f));

                graphics.StrokeColor = attacker.Faction.Color;
                graphics.StrokeLineStrip(laserStart, laserEnd);
            }
        }
        #endregion
        #endregion
    }
}
