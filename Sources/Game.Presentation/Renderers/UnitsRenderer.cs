using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;
using Orion.Engine;
using Orion.Engine.Collections;
using Orion.Engine.Geometry;
using Orion.Engine.Graphics;
using Orion.Game.Simulation;
using Orion.Game.Simulation.Pathfinding;
using Orion.Game.Simulation.Skills;
using Orion.Game.Simulation.Tasks;
using Orion.Game.Simulation.Components;
using System.Diagnostics;

namespace Orion.Game.Presentation.Renderers
{
    /// <summary>
    /// Provides functionality to draw <see cref="Unit"/>s on-screen.
    /// </summary>
    public sealed class UnitsRenderer
    {
        #region Fields
        #region Constants
        private static readonly float fireSize = 2;
        private static readonly float fireAlpha = 0.8f;
        private static readonly float fireSecondsPerFrame = 0.04f;

        /// <summary>
        /// The health ratio below which buildings are on fire.
        /// </summary>
        private static readonly float fireHealthRatio = 0.5f;

        private static readonly Size miniatureUnitSize = new Size(3, 3);

        private static readonly float shadowAlpha = 0.3f;
        private static readonly float shadowDistance = 0.7f;
        private static readonly float shadowScaling = 0.6f;

        private static readonly float meleeHitSpinTimeInSeconds = 0.25f;
        private static readonly float rangedShootTimeInSeconds = 0.25f;
        private static readonly float laserLength = 0.8f;
        #endregion

        private readonly Faction faction;
        private readonly GameGraphics gameGraphics;
        private readonly SpriteAnimation fireAnimation;
        private readonly BuildingMemoryRenderer buildingMemoryRenderer;
        private readonly RuinsRenderer ruinsRenderer;
        private float simulationTimeInSeconds;
        private bool drawHealthBars;
        private List<HitEventArgs> hitEvents;
        #endregion

        #region Constructors
        public UnitsRenderer(Faction faction, GameGraphics gameGraphics)
        {
            Argument.EnsureNotNull(faction, "faction");
            Argument.EnsureNotNull(gameGraphics, "gameGraphics");
            
            this.faction = faction;
            this.gameGraphics = gameGraphics;
            this.fireAnimation = new SpriteAnimation(gameGraphics, "Fire", fireSecondsPerFrame);
            this.buildingMemoryRenderer = new BuildingMemoryRenderer(faction, gameGraphics);
            this.ruinsRenderer = new RuinsRenderer(faction, gameGraphics);
            this.hitEvents = new List<HitEventArgs>();

            World.Updated += OnWorldUpdated;
            World.UnitHitting += OnUnitHitting;
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

        private void OnUnitHitting(World sender, HitEventArgs args)
        {     
            bool isRanged = (int)args.Hitter.GetStatValue(Attacker.RangeStat, AttackSkill.RangeStat) > 0;
            if (!isRanged) return;

            hitEvents.Add(args);
        }

        public void Draw(GraphicsContext graphicsContext, Rectangle viewBounds)
        {
            Argument.EnsureNotNull(graphicsContext, "graphicsContext");

            ruinsRenderer.Draw(graphicsContext, viewBounds);

            DrawRememberedBuildings(graphicsContext);

            DrawGroundUnits(graphicsContext, viewBounds);
            DrawLasers(graphicsContext, viewBounds, CollisionLayer.Ground);
            DrawAirborneUnits(graphicsContext, viewBounds);
            DrawLasers(graphicsContext, viewBounds, CollisionLayer.Air);
        }

        #region Miniature
        public void DrawMiniature(GraphicsContext context)
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
                .Intersecting(clippingBounds)
                .OfType<Unit>()
                .Where(unit => faction.CanSee(unit));
        }

        private void DrawGroundUnits(GraphicsContext graphicsContext, Rectangle viewBounds)
        {
            var units = GetClippedVisibleUnits(viewBounds)
                .Where(unit => unit.Components.Get<Spatial>().CollisionLayer == CollisionLayer.Ground);
            foreach (Unit unit in units) DrawUnit(graphicsContext, unit);
        }

        private void DrawAirborneUnits(GraphicsContext graphicsContext, Rectangle viewBounds)
        {
            var units = GetClippedVisibleUnits(viewBounds)
                .Where(unit => unit.Components.Get<Spatial>().CollisionLayer == CollisionLayer.Air);
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
                Rectangle localRectangle = Rectangle.FromCenterSize(0, 0, unit.Size.Width, unit.Size.Height);
                graphics.Fill(localRectangle, texture, unit.Faction.Color);
            }

            if (unit.IsBuilding)
            {
                if (unit.IsUnderConstruction)
                {
                    graphics.Fill(unit.BoundingRectangle, UnderConstructionTexture, Colors.White);
                }
                else if (unit.Health / unit.MaxHealth < fireHealthRatio)
                {
                    float fireTime = simulationTimeInSeconds + (unit.Handle.Value * fireSecondsPerFrame);
                    Texture fireTexture = fireAnimation.GetTextureFromTime(simulationTimeInSeconds);
                    Rectangle fireRectangle = Rectangle.FromCenterSize(unit.Center, new Vector2(fireSize, fireSize));
                    graphics.Fill(fireRectangle, fireTexture, new ColorRgba(1, 1, 1, fireAlpha));
                }
            }

            if (DrawHealthBars) HealthBarRenderer.Draw(graphics, unit);
        }

        private void DrawUnitShadow(GraphicsContext graphicsContext, Unit unit)
        {
            Texture texture = gameGraphics.GetUnitTexture(unit);
            ColorRgba tint = new ColorRgba(Colors.Black, shadowAlpha);

            float drawingAngle = GetUnitDrawingAngle(unit);
            float oscillation = GetOscillation(unit);
            float distance = shadowDistance - oscillation * 0.1f;
            Vector2 center = unit.Center + new Vector2(-distance, distance);
            float scaling = shadowScaling + oscillation * 0.1f;
            using (graphicsContext.PushTransform(center, drawingAngle, scaling))
            {
                Rectangle localRectangle = Rectangle.FromCenterSize(0, 0, unit.Size.Width, unit.Size.Height);
                graphicsContext.Fill(localRectangle, texture, tint);
            }
        }

        private float GetOscillation(Unit unit)
        {
            Debug.Assert(unit.Components.Has<Spatial>(), "Unit has no spatial component!");
            if (unit.Components.Get<Spatial>().CollisionLayer == CollisionLayer.Ground) return 0;

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

            Debug.Assert(unit.Components.Has<Spatial>(), "Unit has no spatial component!");
            float angle = unit.Components.Get<Spatial>().Angle;
            float baseAngle = angle + (float)Math.PI * 0.5f;

            bool isMelee = unit.HasComponent<Attacker, AttackSkill>()
                && (int)unit.GetStatValue(Attacker.RangeStat, AttackSkill.RangeStat) == 0;
            if (!isMelee || unit.TimeElapsedSinceLastHitInSeconds > meleeHitSpinTimeInSeconds)
                return baseAngle;

            float spinProgress = unit.TimeElapsedSinceLastHitInSeconds / meleeHitSpinTimeInSeconds;
            float spinAngle = spinProgress * (float)Math.PI * 2;

            return baseAngle + spinAngle;
        }

        private void DrawLasers(GraphicsContext graphics, Rectangle bounds, CollisionLayer layer)
        {
            for (int i = hitEvents.Count() - 1; i >= 0; i--)
            {
                HitEventArgs hit = hitEvents[i];
                Entity hitter = hit.Hitter;
                Debug.Assert(hitter.Components.Has<Spatial>(), "Hitter has no Spatial component!");
                if (hitter.Components.Get<Spatial>().CollisionLayer != layer)
                    continue;

                if (hit.Hitter.TimeElapsedSinceLastHitInSeconds > rangedShootTimeInSeconds)
                    continue;

                float laserProgress = (World.LastSimulationStep.TimeInSeconds - hit.TimeHitOccurred) / meleeHitSpinTimeInSeconds;

                Vector2 delta = hit.Target.Center - hit.Hitter.Center;
                if (laserProgress > 1)
                {
                    hitEvents.RemoveAt(i);
                    continue;
                }

                if (!Rectangle.Intersects(hit.Hitter.BoundingRectangle, bounds)
                    && !Rectangle.Intersects(hit.Target.BoundingRectangle, bounds))
                    continue;

                Vector2 normalizedDelta = Vector2.Normalize(delta);
                float distance = delta.LengthFast;

                Vector2 laserCenter = hit.Hitter.Center + normalizedDelta * laserProgress * distance;
                if (!faction.CanSee(new Region((int)laserCenter.X, (int)laserCenter.Y, 1, 1)))
                    continue;

                Vector2 laserStart = hit.Hitter.Center + (normalizedDelta
                    * Math.Max(0, laserProgress * distance - laserLength * 0.5f));
                Vector2 laserEnd = hit.Hitter.Center + (normalizedDelta
                    * Math.Min(distance, laserProgress * distance + laserLength * 0.5f));

                LineSegment lineSegment = new LineSegment(laserStart, laserEnd);
                graphics.Stroke(lineSegment, hit.Hitter.Faction.Color);
            }
        }
        #endregion
        #endregion
    }
}
