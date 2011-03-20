using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Orion.Engine;
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
        private readonly HashSet<Entity> activeWorkers = new HashSet<Entity>();
        private readonly HashSet<Entity> inactiveWorkers = new HashSet<Entity>();
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

            foreach (Entity worker in faction.Entities.Where(entity => IsWorker(entity)))
                AddWorker(worker);
        }
        #endregion

        #region Events
        /// <summary>
        /// Raised when a worker's activity state changed.
        /// </summary>
        public event Action<WorkerActivityMonitor, Entity> WorkerActivityStateChanged;
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

        public IEnumerable<Entity> InactiveWorkers
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
        private void AddWorker(Entity worker)
        {
            TaskQueue taskQueue = worker.Components.Get<TaskQueue>();

            HashSet<Entity> pool = taskQueue.IsEmpty ? inactiveWorkers : activeWorkers;
            pool.Add(worker);
            taskQueue.Changed += workerTaskQueueChangedEventHandler;
        }

        private void OnEntityAdded(World sender, Entity entity)
        {
            if (FactionMembership.GetFaction(entity) != faction || !IsWorker(entity)) return;

            AddWorker(entity);
            WorkerActivityStateChanged.Raise(this, entity);
        }

        private void OnEntityRemoved(World sender, Entity entity)
        {
            if (FactionMembership.GetFaction(entity) != faction || !IsWorker(entity)) return;

            inactiveWorkers.Remove(entity);
            activeWorkers.Remove(entity);
            entity.Components.Get<TaskQueue>().Changed -= workerTaskQueueChangedEventHandler;

            WorkerActivityStateChanged.Raise(this, entity);
        }

        private void OnWorkerTaskQueueChanged(TaskQueue taskQueue)
        {
            Entity worker = taskQueue.Entity;
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
