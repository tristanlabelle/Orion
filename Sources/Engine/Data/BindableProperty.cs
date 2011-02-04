using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;
using System.ComponentModel;

namespace Orion.Engine.Data
{
    /// <summary>
    /// Represents a binding between a source and a destination property which keeps them in sync.
    /// </summary>
    public sealed class BindableProperty : Bindable
    {
        #region Fields
        [ThreadStatic]
        private static readonly object[] parameters = new object[1];

        private readonly object target;
        private readonly PropertyInfo property;
        private readonly EventInfo changedEvent;
        private readonly Delegate changedEventHandler;
        #endregion

        #region Constructors
        public BindableProperty(object target, PropertyInfo property, EventInfo changedEvent)
        {
            Argument.EnsureNotNull(property, "property");
            if (property.GetIndexParameters().Length > 0)
                throw new ArgumentException("Cannot bind to a property with index parameters.", "property");

            this.target = property.IsStatic() ? null : target;
            this.property = property;

            if (changedEvent != null)
            {
                if (changedEvent.DeclaringType != property.DeclaringType)
                    throw new ArgumentException("The declaring type of the changed event should match the property's.", "changedEvent");

                this.changedEvent = changedEvent;

                var changedEventParameterTypes = changedEvent.EventHandlerType.GetDelegateParameterTypes();

                MethodInfo handlerMethod = typeof(BindableProperty)
                    .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                    .FirstOrDefault(method => method.Name == "OnChanged" && method.GetGenericArguments().Length == changedEventParameterTypes.Length);
                if (handlerMethod != null)
                {
                    handlerMethod = handlerMethod.MakeGenericMethod(changedEventParameterTypes);
                    this.changedEventHandler = Delegate.CreateDelegate(changedEvent.EventHandlerType, this, handlerMethod);
                    changedEvent.AddEventHandler(target, changedEventHandler);
                }
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the instance which has the bindable property.
        /// </summary>
        public object Target
        {
            get { return target; }
        }

        /// <summary>
        /// Gets the property wrapped by this <see cref="BindableProperty{T}"/>.
        /// </summary>
        public PropertyInfo Property
        {
            get { return property; }
        }

        public override bool IsReadable
        {
            get { return property.CanRead; }
        }

        public override bool IsWriteable
        {
            get { return property.CanWrite; }
        }

        public override bool IsObservable
        {
            get { return changedEventHandler != null; }
        }

        public override Type Type
        {
            get { return property.PropertyType; }
        }

        /// <summary>
        /// Gets a value indicating if the target property is static.
        /// </summary>
        public bool IsStatic
        {
            get { return target == null; }
        }
        #endregion

        #region Methods
        protected override object GetValue()
        {
            return property.GetGetMethod().Invoke(target, null);
        }

        protected override void SetValue(object value)
        {
            parameters[0] = value;
            property.GetSetMethod().Invoke(target, parameters);
        }

        public static EventInfo GetChangedEvent(PropertyInfo property)
        {
            Argument.EnsureNotNull(property, "property");

            BindingFlags bindingFlags = (property.IsStatic() ? BindingFlags.Static : BindingFlags.Instance) | BindingFlags.Public;
            var changedEventAttribute = (PropertyChangedEventAttribute)property
                .GetCustomAttributes(typeof(PropertyChangedEventAttribute), true)
                .FirstOrDefault();

            string changedEventName = changedEventAttribute == null
                ? property.Name + "Changed"
                : changedEventAttribute.EventName;
            
            return property.ReflectedType.GetEvent(changedEventName, bindingFlags);
        }

        private void OnChanged()
        {
            RaiseValueChanged();
        }

        private void OnChanged<T1>(T1 arg1)
        {
            RaiseValueChanged();
        }

        private void OnChanged<T1, T2>(T1 arg1, T2 arg2)
        {
            RaiseValueChanged();
        }

        private void OnChanged<T1, T2, T3>(T1 arg1, T2 arg2, T3 arg3)
        {
            RaiseValueChanged();
        }

        private void OnChanged<T1, T2, T3, T4>(T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            RaiseValueChanged();
        }

        private void OnChanged<T1, T2, T3, T4, T5>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            RaiseValueChanged();
        }

        public static BindableProperty FromExpression<T>(Expression<Func<T>> expression)
        {
            Argument.EnsureNotNull(expression, "expression");

            MemberExpression memberAccessExpression = expression.Body as MemberExpression;
            if (memberAccessExpression == null) throw new ArgumentException("expression");

            PropertyInfo property = memberAccessExpression.Member as PropertyInfo;
            if (property == null) throw new ArgumentException("expression");

            ConstantExpression constantTargetExpression = memberAccessExpression.Expression as ConstantExpression;
            object target = constantTargetExpression == null
                ? Expression.Lambda<Func<object>>(memberAccessExpression.Expression).Compile()()
                : constantTargetExpression.Value;

            EventInfo changedEvent = GetChangedEvent(property);

            return new BindableProperty(target, property, changedEvent);
        }
        #endregion
    }
}
