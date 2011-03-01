using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Diagnostics;
using Orion.Engine;
using Orion.Engine.Collections;
using Orion.Game.Simulation.Tasks;

namespace Orion.Game.Simulation.Components
{
    /// <summary>
    /// Represents a queue of <see cref="Task"/>s to be executed by a <see cref="Entity"/>.
    /// </summary>
    public sealed class TaskQueue : Component, IList<Task>
    {
        #region Instance
        #region Fields
        private readonly List<Task> tasks = new List<Task>();

        /// <summary>
        /// Associates unique IDs to each of a <see cref="Entity"/>'s <see cref="Task"/>s
        /// so that they can be referred over the network. This is used to cancel <see cref="Task"/>s.
        /// </summary>
        private readonly BiDictionary<Task, uint> taskIDs = new BiDictionary<Task, uint>();
        private uint nextTaskID = 1;
        #endregion

        #region Constructors
        public TaskQueue(Entity entity)
            : base(entity) { }
        #endregion

        #region Events
        /// <summary>
        /// Raised when the contents of this task queue has changed.
        /// </summary>
        public event Action<TaskQueue> Changed;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the number of <see cref="Task">tasks</see> in this <see cref="TaskQueue"/>.
        /// </summary>
        public int Count
        {
            get { return tasks.Count; }
        }

        /// <summary>
        /// Gets a value indicating if this <see cref="TaskQueue"/> contains no <see cref="Task"/>.
        /// </summary>
        public bool IsEmpty
        {
            get { return tasks.Count == 0; }
        }

        /// <summary>
        /// Gets the <see cref="Task"/> currently being executed.
        /// </summary>
        public Task Current
        {
            get { return IsEmpty ? null : tasks[0]; }
        }

        /// <summary>
        /// Gets the <see cref="Entity"/> to which this <see cref="TaskQueue"/> belongs.
        /// </summary>
        public Unit Unit
        {
            get { return (Unit)Entity; }
        }
        #endregion

        #region Indexers
        /// <summary>
        /// Gets a <see cref="Task"/> of this <see cref="TaskQueue"/> from its index.
        /// </summary>
        /// <param name="index">The index of the <see cref="Task"/> to be found.</param>
        /// <returns>The <see cref="Task"/> at that index.</returns>
        public Task this[int index]
        {
            get { return tasks[index]; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Attempts to retrieve the handle to a given <see cref="Task"/> in this <see cref="TaskQueue"/>.
        /// </summary>
        /// <param name="task">The <see cref="Task"/> for which the identifier is retrieved.</param>
        /// <returns>The handle of <paramref name="task"/>, or <c>0</c> if the <see cref="Task"/> was not found.</returns>
        public Handle TryGetTaskHandle(Task task)
        {
            if (task == null) return new Handle(0);
            return new Handle(taskIDs.GetValueOrDefault(task));
        }

        /// <summary>
        /// Attempts to retrieve a <see cref="Task"/> in this <see cref="TaskQueue"/> from its handle.
        /// </summary>
        /// <param name="handle">The handle of the <see cref="Task"/>.</param>
        /// <returns>The <see cref="Task"/> with that handle, or <c>null</c> if no task has this handle.</returns>
        public Task TryResolveTask(Handle handle)
        {
            return taskIDs.GetKeyOrDefault(handle.Value);
        }

        /// <summary>
        /// Updates the current task for a frame.
        /// </summary>
        /// <param name="info">Information on the update.</param>
        public override void Update(SimulationStep step)
        {
            if (IsEmpty) return;
            Debug.Assert(tasks.Count == taskIDs.Count,
                "The task ID dictionary was not kept synched to the task list.");

            Task task = Current;
            task.Update(step);
            if (task.HasEnded)
            {
                task.Dispose();

                // As the task could involve modifying the task queue,
                // it cannot be assumed that the task is still the first one.
                if (tasks.Count > 0 && tasks[0] == task)
                {
                    tasks.RemoveAt(0);
                    taskIDs.RemoveByKey(task);

                    Changed.Raise(this);
                }
            }
        }

        /// <summary>
        /// Clears the task queue of this <see cref="Entity"/> and sets a task as the current one.
        /// </summary>
        /// <param name="task">The new task to be set.</param>
        public void OverrideWith(Task task)
        {
            Argument.EnsureNotNull(task, "task");
            if (task.Entity != Entity) throw new ArgumentException("Cannot enqueue a task belonging to another entity.");
            Debug.Assert(!Unit.IsUnderConstruction);

            foreach (Task t in tasks) task.Dispose();
            tasks.Clear();
            taskIDs.Clear();

            tasks.Add(task);
            taskIDs.Add(task, nextTaskID++);

            Changed.Raise(this);
        }

        /// <summary>
        /// Replaces the current task of this <see cref="Entity"/> with the given task.
        /// </summary>
        /// <param name="task">The new task that must now be completed.</param>
        public void ReplaceWith(Task task)
        {
            if (tasks.Count == 0) throw new InvalidOperationException("Cannot replace the first task of the queue if the queue is empty");

            taskIDs.RemoveByKey(tasks[0]);
            tasks[0] = task;
            taskIDs.Add(task, nextTaskID++);

            Changed.Raise(this);
        }

        /// <summary>
        /// Enqueues a new <see cref="Task"/> at the end of this <see cref="Entity"/>'s queue of <see cref="Task"/>s.
        /// </summary>
        /// <param name="task">The <see cref="Task"/> to be enqueued.</param>
        public void Enqueue(Task task)
        {
            Argument.EnsureNotNull(task, "task");
            if (task.Entity != Entity) throw new ArgumentException("Cannot enqueue a task belonging to another entity.");
            if (taskIDs.ContainsKey(task)) throw new InvalidOperationException("Cannot add a task already present.");
            Debug.Assert(!Unit.IsUnderConstruction);

            tasks.Add(task);
            taskIDs.Add(task, nextTaskID++);

            Changed.Raise(this);
        }

        /// <summary>
        /// Cancels a given <see cref="Task"/> from this <see cref="TaskQueue"/>.
        /// </summary>
        /// <param name="task">The <see cref="Task"/> to be cancelled.</param>
        public void CancelTask(Task task)
        {
            Argument.EnsureNotNull(task, "task");

            if (tasks.Remove(task))
            {
                taskIDs.RemoveByKey(task);

                Changed.Raise(this);
            }
        }

        /// <summary>
        /// Gets an enumerator over this <see cref="TaskQueue"/>'s <see cref="Task"/>s.
        /// </summary>
        /// <returns>A new <see cref="Task"/> enumerator.</returns>
        public List<Task>.Enumerator GetEnumerator()
        {
            return tasks.GetEnumerator();
        }

        /// <summary>
        /// Removes all <see cref="Task"/>s from this <see cref="Entity"/>'s queue.
        /// </summary>
        public void Clear()
        {
            if (tasks.Count == 0) return;

            foreach (Task task in tasks)
                task.Dispose();
            tasks.Clear();
            taskIDs.Clear();

            Changed.Raise(this);
        }
        #endregion

        #region Explicit Members
        #region IList<Task> Members
        int IList<Task>.IndexOf(Task item)
        {
            return tasks.IndexOf(item);
        }

        void IList<Task>.Insert(int index, Task item)
        {
            throw new NotSupportedException();
        }

        void IList<Task>.RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        Task IList<Task>.this[int index]
        {
            get { return this[index]; }
            set { throw new NotSupportedException(); }
        }
        #endregion

        #region ICollection<Task> Members
        void ICollection<Task>.Add(Task item)
        {
            Enqueue(item);
        }

        bool ICollection<Task>.Contains(Task item)
        {
            return taskIDs.ContainsKey(item);
        }

        void ICollection<Task>.CopyTo(Task[] array, int arrayIndex)
        {
            tasks.CopyTo(array, arrayIndex);
        }

        bool ICollection<Task>.IsReadOnly
        {
            get { throw new NotSupportedException(); }
        }

        bool ICollection<Task>.Remove(Task item)
        {
            throw new NotSupportedException();
        }
        #endregion

        #region IEnumerable<Task> Members
        IEnumerator<Task> IEnumerable<Task>.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion

        #region IEnumerable Members
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion
        #endregion
        #endregion

        #region Static
        #region Methods
        /// <summary>
        /// Tests if a given <see cref="Entity"/> has an empty <see cref="TaskQueue"/> component.
        /// </summary>
        /// <param name="entity">The <see cref="Entity"/> to be tested.</param>
        /// <returns>A value indicating if <paramref name="entity"/> has a <see cref="TaskQueue"/> that is empty.</returns>
        public static bool HasEmpty(Entity entity)
        {
            Argument.EnsureNotNull(entity, "entity");

            TaskQueue taskQueue = entity.Components.TryGet<TaskQueue>();
            return taskQueue != null && taskQueue.IsEmpty;
        }
        #endregion
        #endregion
    }
}
