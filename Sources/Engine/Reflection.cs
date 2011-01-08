using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Orion.Engine
{
    /// <summary>
    /// Provides utility methods when dealing with reflection.
    /// </summary>
    public static class Reflection
    {
        /// <summary>
        /// Gets a value indicating if a given <see cref="PropertyInfo"/> refers to a static property.
        /// </summary>
        /// <param name="property">The property to be tested.</param>
        /// <returns><c>True</c> if it is a static property, <c>false</c> if it is an instance property.</returns>
        public static bool IsStatic(this PropertyInfo property)
        {
            Argument.EnsureNotNull(property, "property");

            return (property.CanRead && property.GetGetMethod().IsStatic)
                || (property.CanWrite && property.GetSetMethod().IsStatic);
        }

        /// <summary>
        /// Gets the types of the parameters of a given delegate type.
        /// </summary>
        /// <param name="type">The delegate type.</param>
        /// <returns>An array containing its parameter types.</returns>
        public static Type[] GetDelegateParameterTypes(this Type type)
        {
            Argument.EnsureNotNull(type, "type");
            if (!typeof(Delegate).IsAssignableFrom(type))
                throw new ArgumentException("Type should be a delegate type.", "type");

            return type.GetMethod("Invoke")
                .GetParameters()
                .Select(parameter => parameter.ParameterType)
                .ToArray();
        }
    }
}
