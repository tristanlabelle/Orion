using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using OpenTK;
using Orion.Engine;
using Orion.Engine.Geometry;
using Orion.Engine.Graphics;
using Orion.Game.Simulation;
using Orion.Game.Simulation.Components;

namespace Orion.Game.Presentation.Renderers
{
    /// <summary>
    /// Provides functionality to draw <see cref="Entity"/>s on-screen.
    /// </summary>
    public sealed class UnitsRenderer
    {
        private struct Laser
        {
            public Entity Shooter;
            public Entity Target;
            public float Time;
        }

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
        private readonly List<Laser> lasers = new List<Laser>();
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
            this.fireAnimation = new SpriteAnimation(gameGraphics, "Fire", fireSecondsPerFrame);
            this.buildingMemoryRenderer = new BuildingMemoryRenderer(faction, gameGraphics);
            this.ruinsRenderer = new RuinsRenderer(faction, gameGraphics);

            World.Updated += OnWorldUpdated;
            World.HitOccured += OnUnitHitting;
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
            if (!args.Hitter.Components.Get<Attacker>().IsRanged) return;

            Laser laser = new Laser
            {
                Shooter = args.Hitter,
                Target = args.Target,
                Time = sender.LastSimulationStep.TimeInSeconds
            };
            lasers.Add(laser);
        }

        public void Draw(GraphicsContext graphicsContext, Rectangle viewBounds)
        {
            Argument.EnsureNotNull(graphicsContext, "graphicsContext");

            ruinsRenderer.Draw(graphicsContext, viewBounds);

            DrawRememberedBuildings(graphicsContext);

            DrawGroundEntities(graphicsContext, viewBounds);
            DrawLasers(graphicsContext, viewBounds, CollisionLayer.Ground);
            DrawAirborneEntities(graphicsContext, viewBounds);
            DrawLasers(graphicsContext, viewBounds, CollisionLayer.Air);
        }

        #region Miniature
        public void DrawMiniature(GraphicsContext context)
        {
            buildingMemoryRenderer.DrawMiniature(context, miniatureUnitSize);

            foreach (Entity entity in World.Entities)
            {
                Faction entityFaction = FactionMembership.GetFaction(entity);
                if (entityFaction == null || !faction.CanSee(entity)) continue;

                Rectangle rectangle = new Rectangle(entity.Position, (Vector2)miniatureUnitSize);
                context.Fill(rectangle, entityFaction.Color);
            }
        }
        #endregion

        #region Units
        private void DrawRememberedBuildings(GraphicsContext graphics)
        {
            buildingMemoryRenderer.Draw(graphics);
        }

        private IEnumerable<Entity> GetClippedVisibleEntities(Rectangle clippingBounds)
        {
            return World.Entities
                .Intersecting(clippingBounds)
                .Where(entity => !entity.Components.Has<Harvestable>() && faction.CanSee(entity));
        }

        private void DrawGroundEntities(GraphicsContext graphicsContext, Rectangle viewBounds)
        {
            var entities = GetClippedVisibleEntities(viewBounds)
                .Where(entity => entity.Spatial.CollisionLayer == CollisionLayer.Ground);
            foreach (Entity entity in entities) DrawEntity(graphicsContext, entity);
        }

        private void DrawAirborneEntities(GraphicsContext graphicsContext, Rectangle viewBounds)
        {
            var entities = GetClippedVisibleEntities(viewBounds)
                .Where(entity => entity.Spatial.CollisionLayer == CollisionLayer.Air);
            foreach (Entity entity in entities) DrawUnitShadow(graphicsContext, entity);
            foreach (Entity entity in entities) DrawEntity(graphicsContext, entity);
        }

        private void DrawEntity(GraphicsContext graphics, Entity entity)
        {
            Texture texture = gameGraphics.GetEntityTexture(entity);

            Vector2 center = entity.Center;
            center.Y += GetOscillation(entity) * 0.15f;

            float drawingAngle = GetDrawingAngle(entity);
            using (graphics.PushTransform(center, drawingAngle))
            {
                Rectangle localRectangle = Rectangle.FromCenterSize(0, 0, entity.Size.Width, entity.Size.Height);
                Faction faction = FactionMembership.GetFaction(entity);
                graphics.Fill(localRectangle, texture, faction == null ? Colors.White : faction.Color);
            }

            if (entity.Components.Has<BuildProgress>())
            {
                graphics.Fill(entity.BoundingRectangle, UnderConstructionTexture, Colors.White);
            }
            else
            {
                Health health = entity.Components.TryGet<Health>();
                if (health != null
                    && health.Constitution == Constitution.Mechanical
                    && health.Value / (float)entity.GetStatValue(Health.MaxValueStat) < fireHealthRatio)
                {
                    float fireTime = simulationTimeInSeconds + (entity.Handle.Value * fireSecondsPerFrame);
                    Texture fireTexture = fireAnimation.GetTextureFromTime(simulationTimeInSeconds);
                    Rectangle fireRectangle = Rectangle.FromCenterSize(entity.Center, new Vector2(fireSize, fireSize));
                    graphics.Fill(fireRectangle, fireTexture, new ColorRgba(1, 1, 1, fireAlpha));
                }
            }

            if (DrawHealthBars) HealthBarRenderer.Draw(graphics, entity);
        }

        private void DrawUnitShadow(GraphicsContext graphicsContext, Entity entity)
        {
            Texture texture = gameGraphics.GetEntityTexture(entity);
            ColorRgba tint = new ColorRgba(Colors.Black, shadowAlpha);

            float drawingAngle = GetDrawingAngle(entity);
            float oscillation = GetOscillation(entity);
            float distance = shadowDistance - oscillation * 0.1f;
            Vector2 center = entity.Center + new Vector2(-distance, distance);
            float scaling = shadowScaling + oscillation * 0.1f;
            using (graphicsContext.PushTransform(center, drawingAngle, scaling))
            {
                Rectangle localRectangle = Rectangle.FromCenterSize(0, 0, entity.Size.Width, entity.Size.Height);
                graphicsContext.Fill(localRectangle, texture, tint);
            }
        }

        private float GetOscillation(Entity entity)
        {
            if (entity.Spatial.CollisionLayer == CollisionLayer.Ground) return 0;

            float period = 3 + entity.Size.Area / 4.0f;
            float offset = (entity.Handle.Value % 8) / 8.0f * period;
            float progress = ((simulationTimeInSeconds + offset) % period) / period;
            float sineAngle = (float)Math.PI * 2 * progress;
            float sine = (float)Math.Sin(sineAngle);

            return sine;
        }

        private static float GetDrawingAngle(Entity entity)
        {
            // Workaround the fact that our entity textures face up,
            // and building textures are not supposed to be rotated.
            if (entity.Identity.IsBuilding) return 0;

            Debug.Assert(entity.Components.Has<Spatial>(), "An entity without a spatial component is being drawn.");
            float angle = entity.Spatial.Angle;
            float baseAngle = angle + (float)Math.PI * 0.5f;

            Attacker attacker = entity.Components.TryGet<Attacker>();
            if (attacker == null || attacker.IsRanged || attacker.TimeElapsedSinceLastHit > meleeHitSpinTimeInSeconds)
                return baseAngle;

            float spinProgress = attacker.TimeElapsedSinceLastHit / meleeHitSpinTimeInSeconds;
            float spinAngle = spinProgress * (float)Math.PI * 2;

            return baseAngle + spinAngle;
        }

        private void DrawLasers(GraphicsContext graphics, Rectangle bounds, CollisionLayer layer)
        {
            for (int i = lasers.Count() - 1; i >= 0; i--)
            {
                Laser laser = lasers[i];
                Entity shooter = laser.Shooter;
                Debug.Assert(shooter.Spatial == null, "Hitter has no Spatial component!");
                if (shooter.Spatial.CollisionLayer != layer)
                    continue;

                Attacker attacker = shooter.Components.Get<Attacker>();
                if (attacker.TimeElapsedSinceLastHit > rangedShootTimeInSeconds)
                    continue;

                float laserProgress = (World.LastSimulationStep.TimeInSeconds - laser.Time) / meleeHitSpinTimeInSeconds;

                Vector2 delta = laser.Target.Center - shooter.Center;
                if (laserProgress > 1)
                {
                    lasers.RemoveAt(i);
                    continue;
                }

                if (!Rectangle.Intersects(shooter.BoundingRectangle, bounds)
                    && !Rectangle.Intersects(laser.Target.BoundingRectangle, bounds))
                    continue;

                Vector2 normalizedDelta = Vector2.Normalize(delta);
                float distance = delta.LengthFast;

                Vector2 laserCenter = shooter.Center + normalizedDelta * laserProgress * distance;
                if (!faction.CanSee(new Region((int)laserCenter.X, (int)laserCenter.Y, 1, 1)))
                    continue;

                Vector2 laserStart = shooter.Center + (normalizedDelta
                    * Math.Max(0, laserProgress * distance - laserLength * 0.5f));
                Vector2 laserEnd = shooter.Center + (normalizedDelta
                    * Math.Min(distance, laserProgress * distance + laserLength * 0.5f));

                LineSegment lineSegment = new LineSegment(laserStart, laserEnd);
                Faction hitterFaction = FactionMembership.GetFaction(shooter);
                graphics.Stroke(lineSegment, hitterFaction == null ? Colors.White : hitterFaction.Color);
            }
        }
        #endregion
        #endregion
    }
}
