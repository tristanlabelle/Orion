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
            public readonly TimeSpan SpawnTime;
            public readonly Vector2 Position;
            #endregion

            #region Constructors
            public Entry(TimeSpan spawnTime, Vector2 position)
            {
                this.SpawnTime = spawnTime;
                this.Position = position;
            }
            #endregion
        }
        #endregion

        #region Fields
        private const float warningRadius = 10;
        private static readonly TimeSpan warningLifeSpan = TimeSpan.FromSeconds(30);

        private readonly Faction faction;
        private readonly Queue<Entry> entries = new Queue<Entry>();
        #endregion

        #region Constructors
        public UnderAttackMonitor(Faction faction)
        {
            Argument.EnsureNotNull(faction, "faction");

            this.faction = faction;
            this.faction.World.HitOccured += OnWorldUnitHit;
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

        private TimeSpan SimulationTime
        {
            get { return faction.World.SimulationTime; }
        }
        #endregion

        #region Methods
        private void OnWorldUnitHit(World sender, HitEventArgs args)
        {
            Debug.Assert(sender == faction.World);
            Debug.Assert(args.Hitter != null);
            Debug.Assert(args.Target != null);

            if (FactionMembership.GetFaction(args.Hitter) == faction) return;
            if (FactionMembership.GetFaction(args.Target) != faction) return;

            AddWarning(args.Target.Center);
        }

        private void AddWarning(Vector2 position)
        {
            while (entries.Count > 0 && (SimulationTime - entries.Peek().SpawnTime) >= warningLifeSpan)
                entries.Dequeue();

            if (entries.Any(w => (w.Position - position).LengthFast < warningRadius)) return;

            entries.Enqueue(new Entry(SimulationTime, position));

            Warning.Raise(this, position);
        }
        #endregion
    }
}
