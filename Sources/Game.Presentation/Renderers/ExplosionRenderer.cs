using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using Orion.Engine;
using Orion.Engine.Geometry;
using Orion.Engine.Graphics;
using Orion.Game.Simulation;

namespace Orion.Game.Presentation.Renderers
{
    public sealed class ExplosionRenderer
    {
        #region Nested Types
        private struct Explosion
        {
            public readonly TimeSpan SpawnTime;
            public readonly Circle Circle;

            public Explosion(TimeSpan spawnTime, Circle circle)
            {
                this.SpawnTime = spawnTime;
                this.Circle = circle;
            }
        }
        #endregion

        #region Fields
        private static readonly TimeSpan lifeTime = TimeSpan.FromSeconds(0.6);
        private readonly World world;
        private readonly Texture texture;
        private readonly List<Explosion> explosions = new List<Explosion>();
        #endregion

        #region Constructors
        public ExplosionRenderer(World world, GameGraphics gameGraphics)
        {
            Argument.EnsureNotNull(world, "world");
            Argument.EnsureNotNull(gameGraphics, "gameGraphics");

            this.world = world;
            this.texture = gameGraphics.GetMiscTexture("ExplosionFlash");
            this.world.ExplosionOccured += OnExplosionOccured;
        }
        #endregion

        #region Methods
        public void Draw(GraphicsContext graphics)
        {
            Argument.EnsureNotNull(graphics, "graphics");

            for (int i = explosions.Count; i >= 0; --i)
            {
                Explosion explosion = explosions[i];

                double lifePhase = (world.SimulationTime - explosion.SpawnTime).TotalSeconds / lifeTime.TotalSeconds;
                if (lifePhase > 1)
                {
                    explosions.RemoveAt(i);
                    continue;
                }

                float radius = explosion.Circle.Radius * (float)Math.Pow(lifePhase, 0.3);
                float alpha = (float)Math.Pow(1 - lifePhase, 0.3);

                Rectangle rectangle = Rectangle.FromCenterSize(
                    explosion.Circle.Center,
                    new Vector2(radius, radius));
                ColorRgba color = new ColorRgba(Colors.White, alpha);
                graphics.Fill(rectangle, texture, color);
            }
        }

        private void OnExplosionOccured(World sender, Circle circle)
        {
            explosions.Add(new Explosion(world.SimulationTime, new Circle(circle.Center, circle.Radius * 3)));
        }
        #endregion
    }
}
