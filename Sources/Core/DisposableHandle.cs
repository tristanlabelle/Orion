using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Orion
{
    /// <summary>
    /// Provides a handle which locking a resource which can be disposed.
    /// Meant to be used with a C#-like using statement.
    /// </summary>
    public struct DisposableHandle : IDisposable
    {
        #region Fields
        private readonly Action disposer;
        private bool isDisposed;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new handle from a delegate to the disposing method.
        /// </summary>
        /// <param name="disposer">A delegate to the disposing method.</param>
        public DisposableHandle(Action disposer)
        {
            Argument.EnsureNotNull(disposer, "disposer");

            this.disposer = disposer;
            this.isDisposed = false;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets a value indicating if this handle has been disposed.
        /// </summary>
        public bool IsDisposed
        {
            get { return isDisposed; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Releases all resources held by this handle.
        /// </summary>
        public void Dispose()
        {
            if (isDisposed) throw new ObjectDisposedException(null);
            if (disposer == null) throw new InvalidOperationException("Cannot dispose an uninitialized handle.");

            disposer();
            isDisposed = true;
        }
        #endregion
    }
}
