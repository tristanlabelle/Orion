using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using OpenTK;
using Orion.Engine;
using Orion.Engine.Geometry;
using Orion.Game.Simulation;
using Orion.Game.Simulation.Components;

namespace Orion.Game.Simulation.Utilities
{
    /// <summary>
    /// Monitors the attacks to a faction and provides "under attack" warnings.
    /// </summary>
    public sealed class UnderAttackMonitor
    {
        #region Entry
        private struct Entry
        {
            #region Fields
            public readonly float SpawnTime;
            public readonly Vector2 Position;
            #endregion

            #region Constructors
            public Entry(float spawnTime, Vector2 position)
            {
                this.SpawnTime = spawnTime;
                this.Position = position;
            }
            #endregion
        }
        #endregion

        #region Fields
        private const float warningRadius = 10;
        private const float warningLifeSpan = 30;

        private readonly Faction faction;
        private readonly Queue<Entry> entries = new Queue<Entry>();
        private float time;
        #endregion

        #region Constructors
        public UnderAttackMonitor(Faction faction)
        {
            Argument.EnsureNotNull(faction, "faction");

            this.faction = faction;
            this.faction.World.UnitHitting += OnWorldUnitHit;
            this.faction.World.Updated += OnWorldUpdated;
        }
        #endregion

        #region Events
        /// <summary>
        /// Raised to warn that the monitored faction is under attack.
        /// </summary>
        public event Action<UnderAttackMonitor, Vector2> Warning;
        #endregion

        #region Properties
        public Faction Faction
        {
            get { return faction; }
        }
        #endregion

        #region Methods
        private void Update(SimulationStep step)
        {
            time = step.TimeInSeconds;

            while (entries.Count > 0 && (step.TimeInSeconds - entries.Peek().SpawnTime) >= warningLifeSpan)
                entries.Dequeue();
        }

        private void AddWarning(Vector2 position)
        {
            if (entries.Any(w => (w.Position - position).LengthFast < warningRadius))
                return;

            Entry entry = new Entry(time, position);
            entries.Enqueue(entry);

            if (Warning != null) Warning(this, position);
        }

        private void OnWorldUnitHit(World sender, HitEventArgs args)
        {
            Debug.Assert(sender == faction.World);
            Debug.Assert(args.Hitter != null);
            Debug.Assert(args.Target != null);

            if (FactionMembership.GetFaction(args.Hitter) == faction) return;
            if (FactionMembership.GetFaction(args.Target) != faction) return;

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
