using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.ComponentModel;

namespace Orion.Engine.Data
{
    /// <summary>
    /// Binds the values of <see cref="Bindable"/> instances together.
    /// </summary>
    public sealed class Binding
    {
        #region Fields
        private readonly Bindable source;
        private readonly Bindable destination;
        private readonly BindingDirection direction;
        private Func<object, Type, object> converter;
        private bool isUpdatingValue;
        #endregion

        #region Constructors
        public Binding(Bindable source, Bindable destination, BindingDirection direction, Func<object, Type, object> converter)
        {
            Argument.EnsureNotNull(source, "source");
            Argument.EnsureNotNull(destination, "destination");

            this.source = source;
            this.destination = destination;
            this.direction = direction;
            this.converter = converter;
            if (converter == null) this.converter = (obj, type) => Convert.ChangeType(obj, type);

            if (direction == BindingDirection.SourceToDestination || direction == BindingDirection.TwoWay)
            {
                source.ValueChanged += OnSourceValueChanged;
                UpdateDestinationFromSource();
            }

            if (direction == BindingDirection.DestinationToSource || direction == BindingDirection.TwoWay)
            {
                destination.ValueChanged += OnDestinationValueChanged;
                UpdateSourceFromDestination();
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the <see cref="Bindable"/> which provides the source value of this <see cref="Binding"/>.
        /// </summary>
        public Bindable Source
        {
            get { return source; }
        }

        /// <summary>
        /// Gets the <see cref="Bindable"/> which provides the destination value of this <see cref="Binding"/>.
        /// </summary>
        public Bindable Destination
        {
            get { return destination; }
        }

        /// <summary>
        /// Accesses the converter used to convert between source and destination values.
        /// </summary>
        public Func<object, Type, object> Converter
        {
            get { return converter; }
        }

        /// <summary>
        /// Gets the direction of this <see cref="Binding"/>.
        /// </summary>
        public BindingDirection Direction
        {
            get { return direction; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Updates the destination value from the source value.
        /// </summary>
        public void UpdateDestinationFromSource()
        {
            if (isUpdatingValue) return;

            isUpdatingValue = true;

            try
            {
                object value = source.Value;
                if (converter != null) value = converter(value, destination.Type);
                destination.Value = value;
            }
            finally
            {
                isUpdatingValue = false;
            }
        }

        /// <summary>
        /// Updates the source value from the destination value.
        /// </summary>
        public void UpdateSourceFromDestination()
        {
            if (isUpdatingValue) return;

            isUpdatingValue = true;

            try
            {
                object value = destination.Value;
                if (converter != null) value = converter(value, source.Type);
                source.Value = value;
            }
            finally
            {
                isUpdatingValue = false;
            }
        }

        private void OnSourceValueChanged(Bindable sender)
        {
            UpdateDestinationFromSource();
        }

        private void OnDestinationValueChanged(Bindable sender)
        {
            UpdateSourceFromDestination();
        }
        #endregion

        #region Static Factory Methods
        public static Binding CreateOneWay<T, U>(
            Expression<Func<T>> sourcePropertyExpression,
            Expression<Func<U>> destinationPropertyExpression,
            Func<T, U> converter)
        {
            Bindable source = BindableProperty.FromExpression(sourcePropertyExpression);
            if (!source.IsReadable) throw new ArgumentException("Cannot bind from a property that cannot be read.", "source");

            Bindable destination = BindableProperty.FromExpression(destinationPropertyExpression);
            if (!destination.IsWriteable) throw new ArgumentException("Cannot bind to a property that cannot be written.", "destination");

            Func<object, Type, object> bindingConverter = null;
            if (converter != null) bindingConverter = (obj, type) => converter((T)obj);
            Binding binding = new Binding(source, destination, BindingDirection.SourceToDestination, bindingConverter);

            return binding;
        }

        public static Binding CreateOneWay<T, U>(
            Expression<Func<T>> sourcePropertyExpression,
            Expression<Func<U>> destinationPropertyExpression)
        {
            return CreateOneWay(sourcePropertyExpression, destinationPropertyExpression, null);
        }

        public static Binding CreateTwoWay<T, U>(
            Expression<Func<T>> sourcePropertyExpression,
            Expression<Func<U>> destinationPropertyExpression)
        {
            Bindable source = BindableProperty.FromExpression(sourcePropertyExpression);
            if (!source.IsReadable) throw new ArgumentException("Cannot bind from a property that cannot be read.", "source");

            Bindable destination = BindableProperty.FromExpression(destinationPropertyExpression);
            if (!destination.IsWriteable) throw new ArgumentException("Cannot bind to a property that cannot be written.", "destination");

            return new Binding(source, destination, BindingDirection.TwoWay, null);
        }
        #endregion
    }
}
