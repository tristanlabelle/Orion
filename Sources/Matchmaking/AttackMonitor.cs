using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using OpenTK.Math;
using OpenTK.Graphics;
using Orion.Geometry;
using Orion.GameLogic;

namespace Orion.Matchmaking
{
    public sealed class AttackMonitor
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
        private const float warningLifeSpan = 10;

        private readonly Faction faction;
        private readonly Queue<Entry> entries = new Queue<Entry>();
        private float time;
        #endregion

        #region Constructors
        public AttackMonitor(Faction faction)
        {
            Argument.EnsureNotNull(faction, "faction");

            this.faction = faction;
            this.faction.World.UnitHitting += OnWorldUnitHit;
            this.faction.World.Updated += OnWorldUpdated;
        }
        #endregion

        #region Events
        /// <summary>
        /// Raised when the faction is under attack.
        /// </summary>
        public event Action<AttackMonitor, Vector2> Warning;
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
