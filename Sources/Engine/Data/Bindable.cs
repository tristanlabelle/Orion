using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Engine.Data
{
    /// <summary>
    /// Abstract base class for binding end points.
    /// This can be used with a binding to synchronize values between a source and a destination bindable.
    /// </summary>
    public abstract class Bindable
    {
        #region Events
        /// <summary>
        /// Raised when the value of this <see cref="Bindable"/> has changed.
        /// </summary>
        public event Action<Bindable> ValueChanged;
        #endregion

        #region Properties
        /// <summary>
        /// Gets a value indicating if this <see cref="Bindable"/>'s value can be obtained.
        /// </summary>
        public abstract bool IsReadable { get; }

        /// <summary>
        /// Gets a value indicating if this <see cref="Bindable"/>'s value can be set.
        /// </summary>
        public abstract bool IsWriteable { get; }

        /// <summary>
        /// Gets a value indicating if this <see cref="Bindable"/>'s supports its <see cref="ValueChanged"/> event.
        /// </summary>
        public abstract bool IsObservable { get; }

        /// <summary>
        /// Gets the type of this <see cref="Bindable"/>'s value.
        /// </summary>
        public abstract Type Type { get; }

        /// <summary>
        /// Accesses the value of this <see cref="Bindable"/>.
        /// </summary>
        /// <remarks>
        /// This is implemented through propected methods to allow shadowing.
        /// </remarks>
        public object Value
        {
            get
            {
                EnsureReadable();
                return GetValue();
            }
            set
            {
                EnsureWriteable();
                SetValue(value);
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Obtains the value of this <see cref="Bindable"/>.
        /// </summary>
        /// <returns>The value of this <see cref="Bindable"/>.</returns>
        protected abstract object GetValue();

        /// <summary>
        /// Changes the value of this <see cref="Bindable"/>.
        /// </summary>
        /// <param name="value">The new value to be set.</param>
        protected abstract void SetValue(object value);

        /// <summary>
        /// Raises the <see cref="ValueChanged"/> event.
        /// </summary>
        protected void RaiseValueChanged()
        {
            ValueChanged.Raise(this);
        }

        protected void EnsureReadable()
        {
            if (!IsReadable) throw new InvalidOperationException("The Bindable does not support reading.");
        }

        protected void EnsureWriteable()
        {
            if (!IsWriteable) throw new InvalidOperationException("The Bindable does not support writing.");
        }
        #endregion
    }
}
