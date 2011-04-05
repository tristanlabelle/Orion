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
            public float Time;
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
        private const float ruinFadeDurationInSeconds = 1;

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
        private readonly FogOfWarMemory fogOfWarMemory;
        private readonly List<Laser> lasers = new List<Laser>();
        private float simulationTimeInSeconds;
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

            DrawRememberedEntities(graphicsContext);

            DrawEntities(graphicsContext, viewBounds, CollisionLayer.None, DrawEntity);
            DrawEntities(graphicsContext, viewBounds, CollisionLayer.Ground, DrawEntity);
            DrawLasers(graphicsContext, viewBounds, CollisionLayer.Ground);
            DrawEntities(graphicsContext, viewBounds, CollisionLayer.Air, DrawEntityShadow);
            DrawEntities(graphicsContext, viewBounds, CollisionLayer.Air, DrawEntity);
            DrawLasers(graphicsContext, viewBounds, CollisionLayer.Air);
        }

        #region Miniature
        public void DrawMiniature(GraphicsContext context)
        {
            foreach (RememberedEntity entity in fogOfWarMemory.Entities)
            {
                Rectangle rectangle = new Rectangle(entity.Location, (Vector2)miniatureUnitSize);

                ColorRgb color = Colors.White;
                if (entity.Faction == null)
                {
                    Harvestable harvestable = entity.Prototype.Components.TryGet<Harvestable>();
                    if (harvestable != null)
                    {
                        color = harvestable.Type == ResourceType.Aladdium ? miniatureAladdiumColor : miniatureAlageneColor;
                    }
                }
                else
                {
                    color = entity.Faction.Color;
                }

                context.Fill(rectangle, color);
            }

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
            foreach (Entity entity in World.Entities.Intersecting(viewBounds))
            {
                Spatial spatial = entity.Spatial;
                if (spatial == null
                    || spatial.CollisionLayer != collisionLayer
                    || !faction.CanSee(entity))
                {
                    continue;
                }

                DrawEntity(graphicsContext, entity);
            }
        }

        private void DrawEntity(GraphicsContext graphics, Entity entity)
        {
            Spatial spatial = entity.Spatial;
            if (spatial == null) return;

            Sprite sprite = entity.Components.TryGet<Sprite>();
            if (sprite != null)
            {
                Texture texture = gameGraphics.GetEntityTexture(sprite.Name);

                Vector2 center = spatial.Center;
                center.Y += GetOscillation(entity) * 0.15f;

                float drawingAngle = GetDrawingAngle(sprite);
                using (graphics.PushTransform(center, drawingAngle))
                {
                    Rectangle localRectangle = Rectangle.FromCenterSize(0, 0, spatial.Size.Width, spatial.Size.Height);
                    ColorRgba color = GetEntitySpriteColor(entity);
                    graphics.Fill(localRectangle, texture, color);
                }
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
                alpha = timeout.TimeLeft / ruinFadeDurationInSeconds;
                if (alpha < 0) alpha = 0;
                if (alpha > maxRuinAlpha) alpha = maxRuinAlpha;
            }

            return color.ToRgba(alpha);
        }

        private void DrawEntityShadow(GraphicsContext graphicsContext, Entity entity)
        {
            Sprite sprite = entity.Components.TryGet<Sprite>();
            if (sprite == null) return;

            Texture texture = gameGraphics.GetEntityTexture(sprite.Name);
            ColorRgba tint = new ColorRgba(Colors.Black, shadowAlpha);

            float drawingAngle = GetDrawingAngle(sprite);
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
            if (entity.Spatial.CollisionLayer != CollisionLayer.Air) return 0;

            float period = 3 + entity.Size.Area / 4.0f;
            float offset = (entity.Handle.Value % 8) / 8.0f * period;
            float progress = ((simulationTimeInSeconds + offset) % period) / period;
            float sineAngle = (float)Math.PI * 2 * progress;
            float sine = (float)Math.Sin(sineAngle);

            return sine;
        }

        private static float GetDrawingAngle(Sprite sprite)
        {
            if (!sprite.Rotates) return 0;

            Entity entity = sprite.Entity;

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
                Spatial shooterSpatial = shooter.Spatial;
                if (shooterSpatial.CollisionLayer != layer) continue;

                float laserProgress = (World.LastSimulationStep.TimeInSeconds - laser.Time) / rangedShootTimeInSeconds;

                Vector2 delta = laser.Target.Center - shooterSpatial.Center;
                if (laserProgress > 1)
                {
                    lasers.RemoveAt(i);
                    continue;
                }

                Spatial targetSpatial = laser.Target.Spatial;
                if (!Rectangle.Intersects(shooterSpatial.BoundingRectangle, bounds)
                    && !Rectangle.Intersects(targetSpatial.BoundingRectangle, bounds))
                    continue;

                Vector2 normalizedDelta = Vector2.Normalize(delta);
                float distance = delta.LengthFast;

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
