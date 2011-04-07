using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using OpenTK;
using Orion.Engine;
using Orion.Engine.Geometry;
using Orion.Engine.Graphics;
using Orion.Game.Simulation;
using Orion.Game.Simulation.Utilities;
using Orion.Game.Matchmaking;

namespace Orion.Game.Presentation.Renderers
{
    /// <summary>
    /// Displays warning indications on the minimap when under attack.
    /// </summary>
    public sealed class UnderAttackWarningRenderer
    {
        #region AttackWarning
        private struct AttackWarning
        {
            #region Fields
            public readonly TimeSpan SpawnTime;
            public readonly Vector2 Position;
            #endregion

            #region Constructors
            public AttackWarning(TimeSpan spawnTime, Vector2 position)
            {
                this.SpawnTime = spawnTime;
                this.Position = position;
            }
            #endregion
        }
        #endregion

        #region Fields
        private const float warningCircleRadius = 16;
        private const int warningCircleCount = 3;
        private static readonly TimeSpan warningCircleDuration = TimeSpan.FromSeconds(0.3);
        private static readonly ColorRgb warningCircleColor = Colors.Red;

        private readonly UnderAttackMonitor provider;
        private readonly List<AttackWarning> warnings = new List<AttackWarning>();
        #endregion

        #region Constructors
        public UnderAttackWarningRenderer(Faction faction)
        {
            Argument.EnsureNotNull(faction, "faction");

            this.provider = new UnderAttackMonitor(faction);
            this.provider.Warning += OnWarning;
        }
        #endregion

        #region Properties
        private TimeSpan SimulationTime
        {
            get { return provider.Faction.World.SimulationTime; }
        }
        #endregion

        #region Methods
        public void Draw(GraphicsContext context)
        {
            Argument.EnsureNotNull(context, "context");

            for (int i = warnings.Count - 1; i >= 0; --i)
            {
                AttackWarning warning = warnings[i];

                TimeSpan age = SimulationTime - warning.SpawnTime;
                if (age.TotalSeconds < warningCircleCount * warningCircleDuration.TotalSeconds)
                {
                    float radius = (float)((age.TotalSeconds % warningCircleDuration.TotalSeconds)
                        / warningCircleDuration.TotalSeconds * warningCircleRadius);
                    Circle circle = new Circle(warning.Position, radius);
                    context.Stroke(circle, warningCircleColor);
                }
                else
                {
                    warnings.RemoveAt(i);
                }
            }
        }

        private void OnWarning(UnderAttackMonitor sender, Vector2 position)
        {
            warnings.Add(new AttackWarning(SimulationTime, position));
        }
        #endregion
    }
}
