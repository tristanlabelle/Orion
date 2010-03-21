using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Diagnostics;
using Orion.Engine;

namespace Orion.GameLogic.Tasks
{
    /// <summary>
    /// Represents a queue of <see cref="Task"/>s to be executed by a <see cref="Unit"/>.
    /// </summary>
    public sealed class TaskQueue : IList<Task>
    {
        #region Fields
        private const int maxLength = 8;

        private readonly Unit unit;
        private readonly List<Task> tasks = new List<Task>();
        #endregion

        #region Constructors
        public TaskQueue(Unit unit)
        {
            Argument.EnsureNotNull(unit, "unit");
            this.unit = unit;
        }
        #endregion

        #region Events
        /// <summary>
        /// Raised when the contents of this task queue has changed.
        /// </summary>
        public event Action<TaskQueue> Changed;

        private void RaiseChanged()
        {
            var handler = Changed;
            if (handler != null) handler(this);
        }
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
        /// Gets a value indicating if this <see cref="TaskQueue"/> is at its maximum capacity.
        /// </summary>
        public bool IsFull
        {
            get { return tasks.Count >= maxLength; }
        }

        /// <summary>
        /// Gets the <see cref="Task"/> currently being executed.
        /// </summary>
        public Task Current
        {
            get { return IsEmpty ? null : tasks[0]; }
        }

        /// <summary>
        /// Gets the <see cref="Unit"/> to which this <see cref="TaskQueue"/> belongs.
        /// </summary>
        public Unit Unit
        {
            get { return unit; }
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
        /// Updates the current task for a frame.
        /// </summary>
        /// <param name="info">Information on the update.</param>
        public void Update(SimulationStep step)
        {
            if (IsEmpty) return;

            Task task = Current;
            task.Update(step);
            if (task.HasEnded)
            {
                task.Dispose();
                if (Current == task) tasks.RemoveAt(0);
                RaiseChanged();
            }
        }

        /// <summary>
        /// Clears the task queue of this <see cref="Unit"/> and sets a task as the current one.
        /// </summary>
        /// <param name="task">The new task to be set.</param>
        public void OverrideWith(Task task)
        {
            Argument.EnsureNotNull(task, "task");
            if (task.Unit != unit) throw new ArgumentException("Cannot enqueue a task belonging to another unit.");
            Debug.Assert(Count <= 1, "More than one task was overriden, is this voluntary?");
            Debug.Assert(!unit.IsUnderConstruction);

            Clear();

            tasks.Add(task);

            RaiseChanged();
        }

        public void Enqueue(Task task)
        {
            Argument.EnsureNotNull(task, "task");
            if (task.Unit != unit) throw new ArgumentException("Cannot enqueue a task belonging to another unit.");
            if (tasks.Contains(task)) throw new InvalidOperationException("Cannot add a task already present.");
            if (IsFull) throw new InvalidOperationException("Cannot enqueue a task to a full queue");
            Debug.Assert(!unit.IsUnderConstruction);

            tasks.Add(task);

            RaiseChanged();
        }

        public List<Task>.Enumerator GetEnumerator()
        {
            return tasks.GetEnumerator();
        }

        public void Clear()
        {
            if (tasks.Count == 0) return;

            foreach (Task task in tasks)
                task.Dispose();
            tasks.Clear();

            RaiseChanged();
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
            return tasks.Contains(item);
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
    }
}
