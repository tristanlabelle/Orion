using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;
using System.ComponentModel;

namespace Orion.Engine.Gui2
{
    /// <summary>
    /// Provides utility methods to data bind properties.
    /// </summary>
    public static class DataBinding
    {
        #region Fields
        #endregion

        #region Constructors
        #endregion

        #region Properties
        #endregion

        #region Methods
        public static void BindOneWay<T, U>(Expression<Func<T>> sourcePropertyExpression, Expression<Func<U>> destinationPropertyExpression, Func<T, U> converter = null)
        {
            if (!typeof(U).IsAssignableFrom(typeof(T))) throw new ArgumentException("A converter must be specified if types do not match.");

            object sourceObject;
            PropertyInfo sourceProperty;
            GetTargetAndProperty(sourcePropertyExpression, "sourcePropertyExpression", out sourceObject, out sourceProperty);

            object destinationObject;
            PropertyInfo destinationProperty;
            GetTargetAndProperty(destinationPropertyExpression, "destinationPropertyExpression", out destinationObject, out destinationProperty);

            SetValue(sourceObject, sourceProperty, destinationObject, destinationProperty, converter);

            // Check for a corresponding strongly-typed changed event
            PropertyChangedEventAttribute changedEventAttribute = (PropertyChangedEventAttribute)sourceProperty
                .GetCustomAttributes(typeof(PropertyChangedEventAttribute), true)
                .FirstOrDefault();
            string changedEventName = changedEventAttribute == null ? sourceProperty.Name + "Changed" : changedEventAttribute.EventName;
            EventInfo changedEventInfo = sourceProperty.ReflectedType.GetEvent(changedEventName);
            if (changedEventInfo != null)
            {
                throw new NotImplementedException();
            }

            // Checked for IPropertyChangedNotifier implementation
            IPropertyChangedNotifier propertyChangedNotifier = sourceObject as IPropertyChangedNotifier;
            if (propertyChangedNotifier != null)
            {
                propertyChangedNotifier.PropertyChanged += (sender, propertyName) =>
                {
                    if (propertyName != sourceProperty.Name) return;
                    SetValue(sourceObject, sourceProperty, destinationObject, destinationProperty, converter);
                };
                return;
            }

            // Checked for INotifyPropertyChanged implementation
            INotifyPropertyChanged notifyPropertyChanged = sourceObject as INotifyPropertyChanged;
            if (notifyPropertyChanged != null)
            {
                notifyPropertyChanged.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName != sourceProperty.Name) return;
                    SetValue(sourceObject, sourceProperty, destinationObject, destinationProperty, converter);
                };
            }
        }

        private static void SetValue<T, U>(object sourceObject, PropertyInfo sourceProperty,
            object destinationObject, PropertyInfo destinationProperty, Func<T, U> converter)
        {
            object value = sourceProperty.GetValue(sourceObject, null);
            if (converter != null) value = converter((T)value);
            destinationProperty.SetValue(destinationObject, value, null);
        }

        private static void GetTargetAndProperty<T>(Expression<Func<T>> propertyAccessExpression, string argumentName, out object target, out PropertyInfo property)
        {
            Argument.EnsureNotNull(propertyAccessExpression, argumentName);

            MemberExpression memberAccessExpression = propertyAccessExpression.Body as MemberExpression;
            if (memberAccessExpression == null) throw new ArgumentException(argumentName);

            property = memberAccessExpression.Member as PropertyInfo;
            if (property == null) throw new ArgumentException(argumentName);

            if (property.GetAccessors()[0].IsStatic)
            {
                target = null;
                return;
            }

            ConstantExpression constantTargetExpression = memberAccessExpression.Expression as ConstantExpression;
            if (constantTargetExpression != null)
            {
                target = constantTargetExpression.Value;
                return;
            }

            target = Expression.Lambda<Func<object>>(memberAccessExpression.Expression).Compile()();
        }
        #endregion
    }
}
