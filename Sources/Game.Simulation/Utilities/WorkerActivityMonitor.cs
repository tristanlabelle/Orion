using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Game.Simulation;
using Orion.Game.Simulation.Tasks;
using Orion.Game.Simulation.Skills;
using Orion.Game.Simulation.Components;

namespace Orion.Game.Simulation.Utilities
{
    /// <summary>
    /// Monitors the activity of workers for a given faction.
    /// </summary>
    public sealed class WorkerActivityMonitor
    {
        #region Fields
        private readonly Faction faction;
        private readonly HashSet<Unit> activeWorkers = new HashSet<Unit>();
        private readonly HashSet<Unit> inactiveWorkers = new HashSet<Unit>();
        private readonly Action<TaskQueue> workerTaskQueueChangedEventHandler;
        #endregion

        #region Constructors
        public WorkerActivityMonitor(Faction faction)
        {
            Argument.EnsureNotNull(faction, "faction");

            this.faction = faction;
            this.faction.World.EntityAdded += OnEntityAdded;
            this.faction.World.EntityRemoved += OnEntityRemoved;

            this.workerTaskQueueChangedEventHandler = OnWorkerTaskQueueChanged;

            foreach (Unit worker in faction.Units.Where(unit => IsWorker(unit)))
                AddWorker(worker);
        }
        #endregion

        #region Events
        /// <summary>
        /// Raised when a worker's activity state changed.
        /// </summary>
        public event Action<WorkerActivityMonitor, Unit> WorkerActivityStateChanged;
        #endregion

        #region Properties
        public Faction Faction
        {
            get { return faction; }
        }

        public int WorkerCount
        {
            get { return activeWorkers.Count + inactiveWorkers.Count; }
        }

        public IEnumerable<Entity> ActiveWorkers
        {
            get { return activeWorkers.Cast<Entity>(); }
        }

        [PropertyChangedEvent("WorkerActivityStateChanged")]
        public int ActiveWorkerCount
        {
            get { return activeWorkers.Count; }
        }

        public IEnumerable<Unit> InactiveWorkers
        {
            get { return inactiveWorkers; }
        }

        [PropertyChangedEvent("WorkerActivityStateChanged")]
        public int InactiveWorkerCount
        {
            get { return inactiveWorkers.Count; }
        }
        #endregion

        #region Methods
        private void AddWorker(Unit worker)
        {
            HashSet<Unit> pool = worker.TaskQueue.IsEmpty ? inactiveWorkers : activeWorkers;
            pool.Add(worker);
            worker.Components.Get<TaskQueue>().Changed += workerTaskQueueChangedEventHandler;
        }

        private void OnEntityAdded(World sender, Entity entity)
        {
            if (FactionMembership.GetFaction(entity) != faction || !IsWorker(entity)) return;

            Unit unit = (Unit)entity;
            AddWorker(unit);
            WorkerActivityStateChanged.Raise(this, unit);
        }

        private void OnEntityRemoved(World sender, Entity entity)
        {
            if (FactionMembership.GetFaction(entity) != faction || !IsWorker(entity)) return;

            Unit unit = (Unit)entity;
            inactiveWorkers.Remove(unit);
            activeWorkers.Remove(unit);
            unit.Components.Get<TaskQueue>().Changed -= workerTaskQueueChangedEventHandler;

            WorkerActivityStateChanged.Raise(this, unit);
        }

        private void OnWorkerTaskQueueChanged(TaskQueue taskQueue)
        {
            Unit worker = taskQueue.Unit;
            if (taskQueue.IsEmpty)
            {
                bool wasActive = activeWorkers.Remove(worker);
                bool inactivated = inactiveWorkers.Add(worker);
                Debug.Assert(wasActive && inactivated);
                WorkerActivityStateChanged.Raise(this, worker);
            }
            else
            {
                bool wasInactive = inactiveWorkers.Remove(worker);
                if (wasInactive)
                {
                    bool activated = activeWorkers.Add(worker);
                    Debug.Assert(wasInactive && activated);
                    WorkerActivityStateChanged.Raise(this, worker);
                }
            }
        }

        private static bool IsWorker(Entity entity)
        {
            return entity.Components.Has<Builder>();
        }
        #endregion
    }
}
