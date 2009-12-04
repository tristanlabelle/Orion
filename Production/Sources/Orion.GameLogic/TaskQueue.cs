using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Diagnostics;

namespace Orion.GameLogic
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

        #region Properties
        public int Count
        {
            get { return tasks.Count; }
        }

        public bool IsEmpty
        {
            get { return tasks.Count == 0; }
        }

        public bool IsFull
        {
            get { return tasks.Count >= maxLength; }
        }

        public Task Current
        {
            get { return IsEmpty ? null : tasks[0]; }
        }
        #endregion

        #region Indexers
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
        public void Update(UpdateInfo info)
        {
            if (IsEmpty) return;

            Task task = Current;
            task.Update(info);
            if (task.HasEnded)
            {
                task.Dispose();
                if (Current == task) tasks.RemoveAt(0);
            }
        }

        public void OverrideWith(Task task)
        {
            Argument.EnsureNotNull(task, "task");
            if (task.Unit != unit) throw new ArgumentException("Cannot enqueue a task belonging to another unit.");
            Debug.Assert(Count <= 1, "More than one task was overriden, is this voluntary?");
            Debug.Assert(!unit.IsUnderConstruction);
            Clear();
            tasks.Add(task);
        }

        public void Enqueue(Task task)
        {
            Argument.EnsureNotNull(task, "task");
            if (task.Unit != unit) throw new ArgumentException("Cannot enqueue a task belonging to another unit.");
            if (tasks.Contains(task)) throw new InvalidOperationException("Cannot add a task already present.");
            if (IsFull) throw new InvalidOperationException("Cannot enqueue a task to a full queue");
            Debug.Assert(!unit.IsUnderConstruction);
            tasks.Add(task);
        }

        public List<Task>.Enumerator GetEnumerator()
        {
            return tasks.GetEnumerator();
        }

        public void Clear()
        {
            foreach (Task task in tasks)
                task.Dispose();
            tasks.Clear();
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
