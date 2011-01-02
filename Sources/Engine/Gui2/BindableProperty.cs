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
    /// Represents a binding between a source and a destination property which keeps them in sync.
    /// </summary>
    public sealed class BindableProperty<T>
    {
        #region Fields
        private readonly object target;
        private readonly PropertyInfo property;
        private readonly Func<T> getter;
        private readonly Action<T> setter;
        private bool? isBound;
        #endregion

        #region Constructors
        public BindableProperty(object target, PropertyInfo property)
        {
            Argument.EnsureNotNull(property, "property");

            this.property = property;
            this.getter = CreateDelegate<Func<T>>(target, property.GetGetMethod());
            this.setter = CreateDelegate<Action<T>>(target, property.GetSetMethod());
            this.target = (getter == null && setter == null) ? null : target;

            if (getter == null) isBound = false;
        }
        #endregion

        #region Events
        /// <summary>
        /// Raised when the value of the property changes.
        /// </summary>
        public event Action<BindableProperty<T>> ValueChanged;
        #endregion

        #region Properties
        public object Target
        {
            get { return target; }
        }

        public PropertyInfo Property
        {
            get { return property; }
        }

        public bool IsGettable
        {
            get { return getter != null; }
        }

        public bool IsSettable
        {
            get { return setter != null; }
        }

        public bool IsBound
        {
            get { return isBound.GetValueOrDefault(); }
        }

        public bool IsStatic
        {
            get { return target == null; }
        }

        public T Value
        {
            get { return GetValue(); }
            set { SetValue(value); }
        }
        #endregion

        #region Methods
        public T GetValue()
        {
            if (getter == null) throw new InvalidOperationException("Cannot get property value, it is not gettable.");
            return getter();
        }

        public void SetValue(T value)
        {
            if (setter == null) throw new InvalidOperationException("Cannot set property value, it is not settable.");
            setter(value);
        }

        private static TDelegate CreateDelegate<TDelegate>(object target, MethodInfo method) where TDelegate : class
        {
            if (method == null) return null;

            if (method.IsStatic) return (TDelegate)(object)Delegate.CreateDelegate(typeof(TDelegate), method);
            else return (TDelegate)(object)Delegate.CreateDelegate(typeof(TDelegate), target, method);
        }

        public bool Bind()
        {
            if (isBound.HasValue) return isBound.Value;
            
            BindingFlags bindingFlags = (IsStatic ? BindingFlags.Static : BindingFlags.Instance) | BindingFlags.Public;
            var events = property.GetCustomAttributes(typeof(PropertyChangedEventAttribute), true)
                .Cast<PropertyChangedEventAttribute>()
                .Select(attribute => attribute.EventName)
                .DefaultIfEmpty(property.Name + "Changed")
                .Select(eventName => property.ReflectedType.GetEvent(eventName, bindingFlags))
                .Where(@event => @event != null);

            bool wasBound = false;
            foreach (EventInfo @event in events)
            {
                var parameterTypes = @event.EventHandlerType.GetMethod("Invoke")
                    .GetParameters()
                    .Select(parameter => parameter.ParameterType)
                    .ToArray();

                MethodInfo handlerMethod = typeof(BindableProperty<T>)
                    .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                    .FirstOrDefault(method => method.Name == "OnChanged" && method.GetGenericArguments().Length == parameterTypes.Length);
                if (handlerMethod != null)
                {
                    handlerMethod = handlerMethod.MakeGenericMethod(parameterTypes);
                    Delegate handlerDelegate = Delegate.CreateDelegate(@event.EventHandlerType, this, handlerMethod);
                    @event.AddEventHandler(target, handlerDelegate);
                    wasBound = true;
                }
            }

            isBound = wasBound;

            return wasBound;
        }

        private void OnChanged()
        {
            ValueChanged.Raise(this);
        }

        private void OnChanged<T1>(T1 arg1)
        {
            ValueChanged.Raise(this);
        }

        private void OnChanged<T1, T2>(T1 arg1, T2 arg2)
        {
            ValueChanged.Raise(this);
        }

        private void OnChanged<T1, T2, T3>(T1 arg1, T2 arg2, T3 arg3)
        {
            ValueChanged.Raise(this);
        }

        private void OnChanged<T1, T2, T3, T4>(T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            ValueChanged.Raise(this);
        }

        private void OnChanged<T1, T2, T3, T4, T5>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            ValueChanged.Raise(this);
        }

        public static BindableProperty<T> FromExpression(Expression<Func<T>> expression)
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

            return new BindableProperty<T>(target, property);
        }
        #endregion
    }
}
