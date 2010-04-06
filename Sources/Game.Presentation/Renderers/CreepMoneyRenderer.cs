using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine.Gui;
using Orion.Engine.Graphics;
using Orion.Engine.Geometry;
using Orion.Game.Simulation;
using Orion.Engine;
using OpenTK.Math;
using Orion.Game.Simulation.Skills;

namespace Orion.Game.Presentation.Renderers
{
    /// <summary>
    /// Draws the money given by killed creeps.
    /// </summary>
    public sealed class CreepMoneyRenderer : IViewRenderer
    {
        #region Entry Structure
        private struct Entry
        {
            public readonly Vector2 Position;
            public readonly float SpawnTime;
            public readonly string Text;

            public Entry(Vector2 position, float spawnTime, string text)
            {
                this.Position = position;
                this.SpawnTime = spawnTime;
                this.Text = text;
            }
        }
        #endregion

        #region Fields
        private static readonly float duration = 1;

        private readonly Faction localFaction;
        private readonly Queue<Entry> entries = new Queue<Entry>();
        private float time;
        #endregion

        #region Constructors
        public CreepMoneyRenderer(Faction creepFaction)
        {
            Argument.EnsureNotNull(creepFaction, "creepFaction");

            this.localFaction = creepFaction;
            this.localFaction.World.Updated += OnWorldUpdated;
            this.localFaction.World.UnitHitting += OnUnitHitting;
        }
        #endregion

        #region Properties
        #endregion

        #region Methods
        public void Draw(GraphicsContext graphicsContext, Rectangle bounds)
        {
            foreach (Entry entry in entries)
            {
                float age = time - entry.SpawnTime;
                Text text = new Text(entry.Text);
                Rectangle textFrame = text.Frame;
                Vector2 position = new Vector2(entry.Position.X - textFrame.Width * 0.5f,
                    entry.Position.Y + age);
                graphicsContext.Draw(entry.Text, Colors.Yellow);
            }
        }

        private void OnWorldUpdated(World sender, SimulationStep step)
        {
            time = step.TimeInSeconds;

            while (entries.Count > 0 && (time - entries.Peek().SpawnTime) >= duration)
                entries.Dequeue();
        }

        private void OnUnitHitting(World sender, HitEventArgs args)
        {
            if (args.Target.Faction != localFaction
                && args.Target.Health <= 0)
            {
                Entry entry = new Entry(args.Target.Center, time, "+" + args.Target.GetStat(BasicSkill.AladdiumCostStat));
                entries.Enqueue(entry);
            }
        }
        #endregion
    }
}
