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
using Orion.Game.Simulation.Utilities;
using Orion.Game.Simulation.Tasks;

namespace Orion.Game.Presentation.Renderers
{
    /// <summary>
    /// Provides functionality to draw <see cref="Entity"/>s on-screen.
    /// </summary>
    public sealed class EntityRenderer
    {
        private struct Laser
        {
            public Entity Shooter;
            public Entity Target;
            public TimeSpan Time;
        }

        #region Fields
        #region Constants
        private static readonly float fireSize = 2;
        private static readonly float fireAlpha = 0.8f;
        private static readonly float fireSecondsPerFrame = 0.04f;

        private static readonly ColorRgb miniatureAladdiumColor = Colors.Green;
        private static readonly ColorRgb miniatureAlageneColor = Colors.LightCyan;

        /// <summary>
        /// The health ratio below which buildings are on fire.
        /// </summary>
        private static readonly float fireHealthRatio = 0.5f;

        private const float maxRuinAlpha = 0.8f;
        private static readonly TimeSpan ruinFadeDuration = TimeSpan.FromSeconds(1);

        private static readonly Size miniatureEntitySize = new Size(3, 3);

        private static readonly float shadowAlpha = 0.3f;
        private static readonly float shadowDistance = 0.7f;
        private static readonly float shadowScaling = 0.6f;

        private static readonly TimeSpan meleeHitSpinTime = TimeSpan.FromSeconds(0.25f);
        private static readonly TimeSpan rangedShootTime = TimeSpan.FromSeconds(0.25f);
        private static readonly float laserLength = 0.8f;
        #endregion

        private readonly Faction faction;
        private readonly GameGraphics gameGraphics;
        private readonly SpriteAnimation fireAnimation;
        private readonly FogOfWarMemory fogOfWarMemory;
        private readonly Action<GraphicsContext, Entity> drawEntityDelegate;
        private readonly Action<GraphicsContext, Entity> drawEntityShadowDelegate;
        private readonly List<Laser> lasers = new List<Laser>();
        private bool drawHealthBars;
        #endregion

        #region Constructors
        public EntityRenderer(Faction faction, GameGraphics gameGraphics)
        {
            Argument.EnsureNotNull(faction, "faction");
            Argument.EnsureNotNull(gameGraphics, "gameGraphics");
            
            this.faction = faction;
            this.gameGraphics = gameGraphics;
            this.fireAnimation = new SpriteAnimation(gameGraphics, "Fire", fireSecondsPerFrame);
            this.fogOfWarMemory = new FogOfWarMemory(faction);
            this.drawEntityDelegate = DrawEntity;
            this.drawEntityShadowDelegate = DrawEntityShadow;

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
        private void OnUnitHitting(World sender, HitEventArgs args)
        {
            if (!args.Hitter.Components.Get<Attacker>().IsRanged) return;

            Laser laser = new Laser
            {
                Shooter = args.Hitter,
                Target = args.Target,
                Time = sender.LastSimulationStep.Time
            };
            lasers.Add(laser);
        }

        public void Draw(GraphicsContext graphicsContext, Rectangle viewBounds)
        {
            Argument.EnsureNotNull(graphicsContext, "graphicsContext");

            DrawRememberedEntities(graphicsContext);

            DrawEntities(graphicsContext, viewBounds, CollisionLayer.None, drawEntityDelegate);
            DrawEntities(graphicsContext, viewBounds, CollisionLayer.Ground, drawEntityDelegate);
            DrawLasers(graphicsContext, viewBounds, CollisionLayer.Ground);
            DrawEntities(graphicsContext, viewBounds, CollisionLayer.Air, drawEntityShadowDelegate);
            DrawEntities(graphicsContext, viewBounds, CollisionLayer.Air, drawEntityDelegate);
            DrawLasers(graphicsContext, viewBounds, CollisionLayer.Air);
        }

        #region Miniature
        /// <summary>
        /// Draws a miniature version of <see cref="Entities"/>, as they appear on the minimap.
        /// </summary>
        /// <param name="graphics">The <see cref="GraphicsContext"/> used for drawing.</param>
        public void DrawMiniature(GraphicsContext graphics)
        {
            foreach (RememberedEntity entity in fogOfWarMemory.Entities)
                DrawMiniature(graphics, entity.Prototype, entity.Position, entity.Faction);

            foreach (Entity entity in World.Entities)
            {
                Spatial spatial = entity.Spatial;
                Sprite sprite = entity.Components.TryGet<Sprite>();
                if (spatial == null
                    || !faction.CanSee(entity)
                    || sprite == null
                    || !sprite.IsVisibleOnMinimap)
                {
                    continue;
                }

                Faction entityFaction = FactionMembership.GetFaction(entity);
                DrawMiniature(graphics, entity, spatial.Position, entityFaction);
            }
        }

        private static void DrawMiniature(GraphicsContext graphics, Entity prototype, Vector2 position, Faction faction)
        {
            Rectangle rectangle = new Rectangle(position, (Vector2)miniatureEntitySize);
            ColorRgb color = GetMiniatureColor(prototype, faction);
            graphics.Fill(rectangle, color);
        }

        private static ColorRgb GetMiniatureColor(Entity prototype, Faction faction)
        {
            if (faction != null) return faction.Color;

            Harvestable harvestable = prototype.Components.TryGet<Harvestable>();
            if (harvestable != null) return harvestable.Type == ResourceType.Aladdium ? miniatureAladdiumColor : miniatureAlageneColor;

            return Colors.White;
        }
        #endregion

        #region Units
        private void DrawRememberedEntities(GraphicsContext graphics)
        {
            foreach (RememberedEntity entity in fogOfWarMemory.Entities)
            {
                Texture texture = gameGraphics.GetEntityTexture(entity.Prototype);

                ColorRgb color = entity.Faction == null ? Colors.White : entity.Faction.Color;
                graphics.Fill(entity.GridRegion.ToRectangle(), texture, color);
            }
        }

        private void DrawEntities(GraphicsContext graphicsContext, Rectangle viewBounds,
            CollisionLayer collisionLayer, Action<GraphicsContext, Entity> drawDelegate)
        {
            foreach (Spatial spatial in World.SpatialManager.Intersecting(viewBounds))
            {
                Entity entity = spatial.Entity;
                if (spatial.CollisionLayer != collisionLayer || !faction.CanSee(entity))
                    continue;

                drawDelegate(graphicsContext, entity);
            }
        }

        private void DrawEntity(GraphicsContext graphics, Entity entity)
        {
            Spatial spatial = entity.Spatial;
            Sprite sprite = entity.Components.TryGet<Sprite>();
            if (sprite == null) return;
            
            Texture texture = gameGraphics.GetEntityTexture(sprite.Name);

            Vector2 center = spatial.Center;
            center.Y += GetOscillation(entity) * 0.15f;

            float drawingAngle = GetDrawingAngle(entity);
            using (graphics.PushTransform(center, drawingAngle))
            {
                Rectangle localRectangle = Rectangle.FromCenterSize(0, 0, spatial.Size.Width, spatial.Size.Height);
                ColorRgba color = GetEntitySpriteColor(entity);
                graphics.Fill(localRectangle, texture, color);
            }

            if (entity.Components.Has<BuildProgress>())
            {
                graphics.Fill(spatial.BoundingRectangle, UnderConstructionTexture, Colors.White);
            }
            else
            {
                Health health = entity.Components.TryGet<Health>();
                if (health != null
                    && health.Constitution == Constitution.Mechanical
                    && health.Value / (float)entity.GetStatValue(Health.MaxValueStat) < fireHealthRatio)
                {
                    float simulationTimeInSeconds = World.SimulationTimeInSeconds;
                    float fireTime = simulationTimeInSeconds + (entity.Handle.Value * fireSecondsPerFrame);
                    Texture fireTexture = fireAnimation.GetTextureFromTime(simulationTimeInSeconds);
                    Rectangle fireRectangle = Rectangle.FromCenterSize(spatial.Center, new Vector2(fireSize, fireSize));
                    graphics.Fill(fireRectangle, fireTexture, new ColorRgba(1, 1, 1, fireAlpha));
                }
            }

            if (DrawHealthBars) HealthBarRenderer.Draw(graphics, entity);
        }

        private ColorRgba GetEntitySpriteColor(Entity entity)
        {
            Faction faction = FactionMembership.GetFaction(entity);
            ColorRgb color = faction == null ? Colors.White : faction.Color;

            float alpha = 1;
            TimedExistence timeout = entity.Components.TryGet<TimedExistence>();
            if (timeout != null)
            {
                alpha = timeout.TimeLeft / (float)ruinFadeDuration.TotalSeconds;
                if (alpha < 0) alpha = 0;
                if (alpha > maxRuinAlpha) alpha = maxRuinAlpha;
            }

            return color.ToRgba(alpha);
        }

        private void DrawEntityShadow(GraphicsContext graphicsContext, Entity entity)
        {
            Spatial spatial = entity.Spatial;
            Sprite sprite = entity.Components.TryGet<Sprite>();
            if (spatial == null || sprite == null) return;

            Texture texture = gameGraphics.GetEntityTexture(sprite.Name);
            ColorRgba tint = new ColorRgba(Colors.Black, shadowAlpha);

            float drawingAngle = GetDrawingAngle(entity);
            float oscillation = GetOscillation(entity);
            float distance = shadowDistance - oscillation * 0.1f;
            Vector2 center = spatial.Center + new Vector2(-distance, distance);
            float scaling = shadowScaling + oscillation * 0.1f;
            using (graphicsContext.PushTransform(center, drawingAngle, scaling))
            {
                Rectangle localRectangle = Rectangle.FromCenterSize(0, 0, spatial.Size.Width, spatial.Size.Height);
                graphicsContext.Fill(localRectangle, texture, tint);
            }
        }

        private float GetOscillation(Entity entity)
        {
            Spatial spatial = entity.Spatial;
            if (spatial.CollisionLayer != CollisionLayer.Air) return 0;

            float period = 3 + spatial.Size.Area / 4.0f;
            float offset = (entity.Handle.Value % 8) / 8.0f * period;
            float progress = ((World.SimulationTimeInSeconds + offset) % period) / period;
            float sineAngle = (float)Math.PI * 2 * progress;
            float sine = (float)Math.Sin(sineAngle);

            return sine;
        }

        private static float GetDrawingAngle(Entity entity)
        {
            Sprite sprite = entity.Components.Get<Sprite>();
            if (!sprite.Rotates) return 0;

            float angle = entity.Spatial.Angle + (float)Math.PI * 0.5f;

            // Wiggle if harvesting, healing or building/repairing
            Task task = TaskQueue.GetCurrentTask(entity);
            HarvestTask harvestTask = task as HarvestTask;
            if ((harvestTask != null && harvestTask.IsExtracting) || task is HealTask || task is RepairTask)
                return angle + (float)Math.Sin(entity.World.SimulationTime.TotalSeconds * 10 + entity.Handle.Value) * 0.15f;

            // Rotate after attacks
            Attacker attacker = entity.Components.TryGet<Attacker>();
            if (attacker == null || attacker.IsRanged || attacker.TimeElapsedSinceLastHit > meleeHitSpinTime)
                return angle;

            float spinProgress = (float)attacker.TimeElapsedSinceLastHit.TotalSeconds / (float)meleeHitSpinTime.TotalSeconds;
            float spinAngle = spinProgress * (float)Math.PI * 2;

            return angle + spinAngle;
        }

        private void DrawLasers(GraphicsContext graphics, Rectangle bounds, CollisionLayer layer)
        {
#warning EntityRenderer.DrawLasers doesn't seem to properly handle the lack of spatial components and such
            for (int i = lasers.Count - 1; i >= 0; i--)
            {
                Laser laser = lasers[i];

                Entity shooter = laser.Shooter;
                Spatial shooterSpatial = shooter.Spatial;
                if (shooterSpatial.CollisionLayer != layer) continue;

                float laserProgress = (float)(World.LastSimulationStep.Time - laser.Time).TotalSeconds / (float)rangedShootTime.TotalSeconds;

                Spatial targetSpatial = laser.Target.Spatial;
                Vector2 delta = targetSpatial.Center - shooterSpatial.Center;
                if (laserProgress > 1)
                {
                    lasers.RemoveAt(i);
                    continue;
                }

                if (!Rectangle.Intersects(shooterSpatial.BoundingRectangle, bounds)
                    && !Rectangle.Intersects(targetSpatial.BoundingRectangle, bounds))
                    continue;

                float distance = delta.LengthFast;
                Vector2 normalizedDelta = distance < 0.001f ? Vector2.Zero : delta / distance;

                Vector2 laserCenter = shooterSpatial.Center + normalizedDelta * laserProgress * distance;
                if (!faction.CanSee(new Region((int)laserCenter.X, (int)laserCenter.Y, 1, 1)))
                    continue;

                Vector2 laserStart = shooterSpatial.Center + (normalizedDelta
                    * Math.Max(0, laserProgress * distance - laserLength * 0.5f));
                Vector2 laserEnd = shooterSpatial.Center + (normalizedDelta
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
