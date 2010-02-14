using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.GameLogic;
using System.Diagnostics;
using OpenTK.Math;
using Orion.Geometry;
using System.Drawing;

namespace Orion.Graphics.Renderers
{
    public struct AttackWarning
    {
        #region Fields
        public readonly float SpawnTime;
        public readonly Vector2 Position;
        #endregion

        #region Constructors
        public AttackWarning(float spawnTime, Vector2 position)
        {
            this.SpawnTime = spawnTime;
            this.Position = position;
        }
        #endregion
    }

    public sealed class AttackWarningRenderer : IRenderer
    {
        #region Fields
        private const float warningCollisionRadius = 10;
        private const float warningCircleRadius = 16;
        private const int warningCircleCount = 3;
        private const float warningCircleDuration = 0.3f;
        private const float warningLifeSpan = 8;
        private static readonly ColorRgb warningCircleColor = Colors.Red;

        private readonly Faction faction;
        private readonly List<AttackWarning> warnings = new List<AttackWarning>();
        private float time;
        #endregion

        #region Constructors
        public AttackWarningRenderer(Faction faction)
        {
            Argument.EnsureNotNull(faction, "faction");
            this.faction = faction;
            this.faction.World.UnitHitting += OnWorldUnitHit;
            this.faction.World.Updated += OnWorldUpdated;
        }
        #endregion

        #region Methods
        public void Draw(GraphicsContext context)
        {
            Argument.EnsureNotNull(context, "context");

            foreach (AttackWarning warning in warnings)
            {
                float age = time - warning.SpawnTime;
                if (age >= warningCircleCount * warningCircleDuration) continue;
                float radius = (age % warningCircleDuration) / warningCircleDuration * warningCircleRadius;
                Circle circle = new Circle(warning.Position, radius);
                context.StrokeColor = warningCircleColor;
                context.Stroke(circle);
            }
        }

        private void Update(SimulationStep step)
        {
            time = step.TimeInSeconds;
            warnings.RemoveAll(warning => (step.TimeInSeconds - warning.SpawnTime) > warningLifeSpan);
        }

        private void AddWarning(Vector2 position)
        {
            if (warnings.Any(w => (w.Position - position).LengthFast < warningCollisionRadius))
                return;

            AttackWarning warning = new AttackWarning(time, position);
            warnings.Add(warning);
        }

        private void OnWorldUnitHit(World sender, HitEventArgs args)
        {
            Debug.Assert(sender == faction.World);
            Debug.Assert(args.Hitter != null);
            Debug.Assert(args.Target != null);

            if (args.Hitter.Faction == faction) return;
            if (args.Target.Faction != faction) return;

            AddWarning(args.Target.Center);
        }

        private void OnWorldUpdated(World sender, SimulationStep step)
        {
            Debug.Assert(sender == faction.World);

            Update(step);
        }
        #endregion
    }
}
