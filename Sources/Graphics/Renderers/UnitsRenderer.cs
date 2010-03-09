using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Math;
using Orion.Engine.Graphics;
using Orion.GameLogic;
using Orion.GameLogic.Pathfinding;
using Orion.GameLogic.Tasks;
using Orion.Geometry;

namespace Orion.Graphics.Renderers
{
    /// <summary>
    /// Provides functionality to draw <see cref="Unit"/>s on-screen.
    /// </summary>
    public sealed class UnitsRenderer
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
        private readonly GameGraphics gameGraphics;
        private readonly Pool<Ruin> ruinPool = new Pool<Ruin>();
        private readonly List<Ruin> ruins = new List<Ruin>();
        private readonly BuildingMemoryRenderer buildingMemoryRenderer;
        private float simulationTimeInSeconds;
        private bool drawHealthBars;
        #endregion

        #region Constructors
        public UnitsRenderer(Faction faction, GameGraphics gameGraphics)
        {
            Argument.EnsureNotNull(faction, "faction");
            Argument.EnsureNotNull(gameGraphics, "gameGraphics");
            
            this.faction = faction;
            this.gameGraphics = gameGraphics;
            this.buildingMemoryRenderer = new BuildingMemoryRenderer(faction, gameGraphics);

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
            get { return gameGraphics.GetMiscTexture("UnderConstruction"); }
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

        public void Draw(GraphicsContext graphicsContext, Rectangle viewBounds)
        {
            Argument.EnsureNotNull(graphicsContext, "graphicsContext");

            DrawRememberedBuildings(graphicsContext);
            DrawGroundUnits(graphicsContext, viewBounds);
            DrawLasers(graphicsContext, viewBounds, CollisionLayer.Ground);
            DrawAirborneUnits(graphicsContext, viewBounds);
            DrawLasers(graphicsContext, viewBounds, CollisionLayer.Air);
        }

        #region Miniature
        public void DrawMiniature(GraphicsContext context)
        {
            DrawMiniatureUnits(context);
        }

        private void DrawMiniatureUnits(GraphicsContext context)
        {
            buildingMemoryRenderer.DrawMiniature(context, miniatureUnitSize);

            foreach (Entity entity in World.Entities)
            {
                Unit unit = entity as Unit;
                if (unit == null || !faction.CanSee(unit)) continue;
                
                Rectangle rectangle = new Rectangle(unit.Position, (Vector2)miniatureUnitSize);
                context.Fill(rectangle, unit.Faction.Color);
            }
        }
        #endregion

        #region Units
        private void DrawRememberedBuildings(GraphicsContext graphics)
        {
            buildingMemoryRenderer.Draw(graphics);
        }

        private IEnumerable<Unit> GetClippedVisibleUnits(Rectangle clippingBounds)
        {
            return World.Entities
                .OfType<Unit>()
                .Where(unit => Rectangle.Intersects(clippingBounds, unit.BoundingRectangle)
                    && faction.CanSee(unit));
        }

        private void DrawGroundUnits(GraphicsContext graphicsContext, Rectangle viewBounds)
        {
            var units = GetClippedVisibleUnits(viewBounds)
                .Where(unit => !unit.IsAirborne);
            foreach (Unit unit in units) DrawUnit(graphicsContext, unit);
        }

        private void DrawAirborneUnits(GraphicsContext graphicsContext, Rectangle viewBounds)
        {
            var units = GetClippedVisibleUnits(viewBounds)
                .Where(unit => unit.IsAirborne);
            foreach (Unit unit in units) DrawUnitShadow(graphicsContext, unit);
            foreach (Unit unit in units) DrawUnit(graphicsContext, unit);
        }

        private void DrawUnit(GraphicsContext graphics, Unit unit)
        {
            Texture texture = gameGraphics.GetUnitTexture(unit);

            Vector2 center = unit.Center;
            center.Y += GetOscillation(unit) * 0.15f;

            float drawingAngle = GetUnitDrawingAngle(unit);
            using (graphics.PushTransform(center, drawingAngle))
            {
                Rectangle localRectangle = Rectangle.FromCenterSize(0, 0, unit.Width, unit.Height);
                graphics.Fill(localRectangle, texture, unit.Faction.Color);
                if (unit.IsUnderConstruction)
                    graphics.Fill(localRectangle, UnderConstructionTexture, Colors.White);
            }

            if (DrawHealthBars) HealthBarRenderer.Draw(graphics, unit);
        }

        private void DrawUnitShadow(GraphicsContext graphicsContext, Unit unit)
        {
            Texture texture = gameGraphics.GetUnitTexture(unit);
            ColorRgba tint = new ColorRgba(Colors.Black, shadowAlpha);

            float drawingAngle = GetUnitDrawingAngle(unit);
            float oscillation = GetOscillation(unit);
            float distance = shadowDistance + oscillation * 0.1f;
            Vector2 center = unit.Center - new Vector2(distance, distance);
            float scaling = shadowScaling + oscillation * -0.1f;
            using (graphicsContext.PushTransform(center, drawingAngle, scaling))
            {
                Rectangle localRectangle = Rectangle.FromCenterSize(0, 0, unit.Width, unit.Height);
                graphicsContext.Fill(localRectangle, texture, tint);
            }
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

        private void DrawLasers(GraphicsContext graphics, Rectangle bounds, CollisionLayer layer)
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
                if (!Rectangle.Intersects(attacker.BoundingRectangle, bounds)
                    && !Rectangle.Intersects(target.BoundingRectangle, bounds))
                    continue;

                float laserProgress = attacker.TimeElapsedSinceLastHitInSeconds / meleeHitSpinTimeInSeconds;

                Vector2 delta = target.Center - attacker.Center;
                if (delta.LengthSquared < 0.001f) continue;

                Vector2 normalizedDelta = Vector2.Normalize(delta);
                float distance = delta.LengthFast;

                Vector2 laserCenter = attacker.Center + normalizedDelta * laserProgress * distance;
                if (!faction.CanSee(new Region((int)laserCenter.X, (int)laserCenter.Y, 1, 1)))
                    continue;

                Vector2 laserStart = attacker.Center + (normalizedDelta
                    * Math.Max(0, laserProgress * distance - laserLength * 0.5f));
                Vector2 laserEnd = attacker.Center + (normalizedDelta
                    * Math.Min(distance, laserProgress * distance + laserLength * 0.5f));

                LineSegment lineSegment = new LineSegment(laserStart, laserEnd);
                graphics.Stroke(lineSegment, attacker.Faction.Color);
            }
        }
        #endregion
        #endregion
    }
}
