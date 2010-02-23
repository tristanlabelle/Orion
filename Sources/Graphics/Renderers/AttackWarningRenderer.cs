﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using OpenTK.Math;
using Orion.Engine.Graphics;
using Orion.GameLogic;
using Orion.Geometry;
using Orion.Matchmaking;

namespace Orion.Graphics.Renderers
{
    public sealed class AttackWarningRenderer : IRenderer
    {
        #region AttackWarning
        private struct AttackWarning
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
        #endregion

        #region Fields
        private const float warningCircleRadius = 16;
        private const int warningCircleCount = 3;
        private const float warningCircleDuration = 0.3f;
        private static readonly ColorRgb warningCircleColor = Colors.Red;

        private readonly AttackMonitor monitor;
        private readonly List<AttackWarning> warnings = new List<AttackWarning>();
        private float time;
        #endregion

        #region Constructors
        public AttackWarningRenderer(Faction faction)
        {
            Argument.EnsureNotNull(faction, "faction");

            this.monitor = new AttackMonitor(faction);
            this.monitor.Warning += OnWarning;
            faction.World.Updated += OnWorldUpdated;
        }
        #endregion

        #region Methods
        public void Draw(GraphicsContext context)
        {
            Argument.EnsureNotNull(context, "context");

            for (int i = warnings.Count - 1; i >= 0; --i)
            {
                AttackWarning warning = warnings[i];

                float age = time - warning.SpawnTime;
                if (age < warningCircleCount * warningCircleDuration)
                {
                    float radius = (age % warningCircleDuration) / warningCircleDuration * warningCircleRadius;
                    Circle circle = new Circle(warning.Position, radius);
                    context.StrokeColor = warningCircleColor;
                    context.Stroke(circle);
                }
                else
                {
                    warnings.RemoveAt(i);
                }
            }
        }

        private void OnWarning(AttackMonitor sender, Vector2 position)
        {
            AttackWarning warning = new AttackWarning(time, position);
            warnings.Add(warning);
        }

        private void OnWorldUpdated(World sender, SimulationStep step)
        {
            time = step.TimeInSeconds;
        }
        #endregion
    }
}
