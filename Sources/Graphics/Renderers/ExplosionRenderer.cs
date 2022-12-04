using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine.Graphics;
using Orion.GameLogic;
using Orion.Geometry;
using OpenTK.Math;

namespace Orion.Graphics.Renderers
{
    public sealed class ExplosionRenderer
    {
        #region Nested Types
        private struct Explosion
        {
            public readonly float SpawnTime;
            public readonly Circle Circle;

            public Explosion(float spawnTime, Circle circle)
            {
                this.SpawnTime = spawnTime;
                this.Circle = circle;
            }
        }
        #endregion

        #region Fields
        private const float lifeTime = 0.6f;
        private readonly World world;
        private readonly Texture texture;
        private readonly List<Explosion> explosions = new List<Explosion>();
        private float time;
        #endregion

        #region Constructors
        public ExplosionRenderer(World world, GameGraphics gameGraphics)
        {
            Argument.EnsureNotNull(world, "world");
            Argument.EnsureNotNull(gameGraphics, "gameGraphics");

            this.world = world;
            this.texture = gameGraphics.GetMiscTexture("ExplosionFlash");

            this.world.Updated += OnWorldUpdated;
            this.world.ExplosionOccured += OnExplosionOccured;
        }
        #endregion

        #region Methods
        public void Draw(GraphicsContext graphics)
        {
            Argument.EnsureNotNull(graphics, "graphics");
            foreach (Explosion explosion in explosions)
            {
                float age = time - explosion.SpawnTime;
                float lifePhase = age / lifeTime;
                float radius = explosion.Circle.Radius * (float)Math.Pow(lifePhase, 0.3);
                float alpha = (float)Math.Pow(1 - lifePhase, 0.3);

                Rectangle rectangle = Rectangle.FromCenterSize(
                    explosion.Circle.Center,
                    new Vector2(radius, radius));
                ColorRgba color = new ColorRgba(Colors.White, alpha);
                graphics.Fill(rectangle, texture, color);
            }
        }

        private void Update(SimulationStep step)
        {
            this.time = step.TimeInSeconds;
            explosions.RemoveAll(explosion => (step.TimeInSeconds - explosion.SpawnTime) > lifeTime);
        }

        private void OnWorldUpdated(World sender, SimulationStep step)
        {
            Update(step);
        }

        private void OnExplosionOccured(World sender, Circle circle)
        {
            explosions.Add(new Explosion(time, new Circle(circle.Center, circle.Radius * 3)));
        }
        #endregion
    }
}
