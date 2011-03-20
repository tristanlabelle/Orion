using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Diagnostics;
using System.Reflection;
using Orion.Engine;
using Orion.Engine.Collections;
using System.Collections;
using System.Globalization;

namespace Orion.Game.Simulation.Components.Serialization
{
    public class XmlDeserializer
    {
        #region Static
        #region Fields
        private static readonly Type[] componentConstructorTypeArguments = new Type[] { typeof(Entity) };
        private const BindingFlags fieldLookupFlags = BindingFlags.IgnoreCase | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;
        #endregion
        #endregion

        #region Instance
        #region Fields
        private Func<Handle> handleGenerator;
        #endregion

        #region Constructors
        public XmlDeserializer(Func<Handle> generator)
        {
            this.handleGenerator = generator;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Generates an Entity object from an XML file located at a given path.
        /// </summary>
        /// <param name="filePath">The path of the XML document</param>
        /// <param name="persistentOnly">Indicates if only persistent and mandatory attributes should be loaded</param>
        /// <returns>An Entity object with the specs from the XML file</returns>
        public Unit DeserializeEntity(string filePath, bool persistentOnly)
        {
            XmlDocument document = new XmlDocument();
            document.Load(filePath);
            Debug.Assert(document.DocumentElement.Name.Equals("Entity", StringComparison.InvariantCultureIgnoreCase), "Root element has the wrong name!");
            return DeserializeEntity(document.DocumentElement, persistentOnly);
        }

        /// <summary>
        /// Generates and Entity object from an Entity XML element.
        /// </summary>
        /// <param name="filePath">The path of the XML document</param>
        /// <param name="persistentOnly">Indicates if only persistent and mandatory attributes should be loaded</param>
        /// <returns>An Entity object with the specs from the XML file</returns>
        public Unit DeserializeEntity(XmlElement entityElement, bool persistentOnly)
        {
            Debug.Assert(entityElement.Name == "Entity");

            Unit entity = new Unit(handleGenerator());
            Type baseComponentType = typeof(Component);
            Assembly componentAssembly = baseComponentType.Assembly;
            object[] constructorArguments = new object[] { entity };
            foreach (XmlElement componentElement in entityElement.ChildNodes.OfType<XmlElement>())
            {
                string name = componentElement.Name;
                string fullName = baseComponentType.Namespace + "." + name;
                Type preciseComponentType = componentAssembly.GetType(fullName, true, true);
                if (entity.Components.Has(preciseComponentType))
                    throw new InvalidOperationException("Trying to attach multiple components of the same type to an entity ");

                if (!baseComponentType.IsAssignableFrom(preciseComponentType))
                    throw new InvalidOperationException("Trying to instantiate a class that isn't assignable to Component");

                ConstructorInfo constructor = preciseComponentType.GetConstructor(componentConstructorTypeArguments);
                Component component = (Component)constructor.Invoke(constructorArguments);
                HashSet<PropertyInfo> mandatoryProperties = new HashSet<PropertyInfo>(baseComponentType
                    .GetProperties()
                    .Where(p => p.GetCustomAttributes(typeof(MandatoryAttribute), true).Length == 1));
                foreach (XmlElement propertyElement in componentElement.ChildNodes.OfType<XmlElement>())
                {
                    PropertyInfo property = preciseComponentType.GetProperty(propertyElement.Name);
                    if (property == null)
                        throw new InvalidOperationException("Couldn't find a field associated to the {0} XML tag".FormatInvariant(propertyElement.Name));

                    DeserializeProperty(component, property, propertyElement, persistentOnly);
                    mandatoryProperties.Remove(property);
                }
                
                if (mandatoryProperties.Count != 0)
                    throw new InvalidOperationException("Not all mandatory properties were assigned during deserialization");

                entity.Components.Add(component);
            }
            return entity;
        }

        /// <summary>
        /// Deserializes a property of a certain component based on XML data.
        /// </summary>
        /// <param name="component">The component for which the property should be deserialized.</param>
        /// <param name="property">The property to be deserialized.</param>
        /// <param name="propertyElement">The XML element for the property.</param>
        private void DeserializeProperty(Component component, PropertyInfo property, XmlElement propertyElement, bool persistentOnly)
        {
            Argument.EnsureNotNull(component, "component");
            Argument.EnsureNotNull(property, "property");
            Argument.EnsureNotNull(propertyElement, "fieldElement");

            string fieldName = propertyElement.Name;

            object[] attributes = property.GetCustomAttributes(typeof(PersistentAttribute), false);
            if (persistentOnly && attributes.Length != 1)
                throw new InvalidOperationException("Trying to deserialize a transient field in a persistent-only deserialization");

            Type propertyType = property.PropertyType;
            if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(ICollection<>))
            {
                // The non-generic ICollection interface doesn't have an Add method; therefore, we need a method that
                // can use a generic version of ICollection. Because of that, it will need to be generic itself;
                // and we need to populate the type arguments with Type objects we won't know until runtime.
                // Therefore, we must use reflection.
                MethodInfo nongenericDeserializeCollection = typeof(XmlDeserializer).GetMethod("DeserializeCollection", BindingFlags.NonPublic | BindingFlags.Instance);
                MethodInfo deserializeCollection = nongenericDeserializeCollection.MakeGenericMethod(propertyType.GetGenericArguments()[0]);
                deserializeCollection.Invoke(this, new object[] { component, property, propertyElement });
            }
            else
            {
                try
                {
                    object value = DeserializeObject(propertyType, propertyElement);
                    property.SetValue(component, value, null);
                }
                catch (TypeMismatchException e)
                {
                    string message = "Unable to convert the string value {0} to an object of type {1}".FormatInvariant(e.Data, e.ExpectedType);
                    throw new InvalidOperationException(message, e);
                }
            }
        }

        /// <summary>
        /// Deserializes a collection and adds it to a property representing a collection in the given component.
        /// </summary>
        /// <typeparam name="T">The collection's item type</typeparam>
        /// <param name="component">The component for which the field should be deserialized</param>
        /// <param name="property">The field representing the ICollection object</param>
        /// <param name="fieldElement">The XML collection description</param>
        private void DeserializeCollection<T>(Component component, PropertyInfo property, XmlElement fieldElement)
        {
            IEnumerable<XmlElement> childItems = fieldElement
                .ChildNodes
                .OfType<XmlElement>()
                .Where(x => x.Name.Equals("Item", StringComparison.InvariantCultureIgnoreCase));

            ICollection<T> collection = (ICollection<T>)property.GetValue(component, null);
            foreach (XmlElement item in childItems)
                collection.Add((T)DeserializeObject(typeof(T), item));
        }

        /// <summary>
        /// Deserializes an object from an XML description.
        /// </summary>
        /// <remarks>
        /// Strings, integers, reals and enum values are converted directly from their string form into their final form.
        /// More complex objects, such as delegates and custom types, are handled by finding an appropriate constructor
        /// considering the child nodes of the objectElement. There is no support for collections on this level.
        /// </remarks>
        /// <param name="type">The type of object to deserialize</param>
        /// <param name="objectElement">The XML element representing this object</param>
        /// <returns>An object of the class represented by Type</returns>
        private object DeserializeObject(Type type, XmlElement objectElement)
        {
            // primitive types
            if (type == typeof(string) || type == typeof(int) || type == typeof(float) || type == typeof(bool))
            {
                try
                {
                    return Convert.ChangeType(objectElement.InnerText, type, CultureInfo.InvariantCulture);
                }
                catch (InvalidCastException) { }
                catch (FormatException) { }
                catch (ArgumentException) { }

                throw new TypeMismatchException(type, objectElement.InnerText);
            }

            // enum types
            if (type.IsEnum)
            {
                string innerText = objectElement.InnerText;
                if (!Enum.IsDefined(type, innerText))
                    throw new TypeMismatchException(type, innerText);
                return Enum.Parse(type, innerText, true);
            }

            // delegate types
            if (typeof(Delegate).IsAssignableFrom(type))
            {
                if (!objectElement.HasAttribute("CallTarget"))
                    throw new TypeMismatchException(type, objectElement.OuterXml);
                string target = objectElement.GetAttribute("CallTarget");

                // We need to return an object of the right type; therefore, we have to create a legit, working
                // delegate with the right return type. But to do so, we need to have some generic arguments
                // filled with runtime Type objects; so we have to use reflection to call the generating method.
                MethodInfo genericDeserializeDelegate = typeof(XmlDeserializer).GetMethod("DeserializeDelegate", BindingFlags.Instance | BindingFlags.NonPublic);
                MethodInfo method = genericDeserializeDelegate.MakeGenericMethod(type.GetMethod("Invoke").ReturnType);
                return method.Invoke(this, new object[] { target, objectElement });
            }

            // complex types
            // those are harder because the XML serializer uses structural typing
            return DeserializeComplexType(type, objectElement);
        }

        /// <summary>
        /// Deserializes an object of a complex type.
        /// </summary>
        /// <remarks>
        /// Complex types are anything that is not a string, an integer, a real or a delegate. They are created by
        /// finding a constructor with arguments that match the elements inside objectElement and
        /// using it. No further treatment is possible on objects created this way.
        /// </remarks>
        /// <param name="type">The type of the object to create</param>
        /// <param name="objectElement">The XML element representing the complex object</param>
        /// <returns>An object of the specified type, as described by the XML element</returns>
        private object DeserializeComplexType(Type type, XmlElement objectElement)
        {
            XmlElement[] children = objectElement.ChildNodes.OfType<XmlElement>().ToArray();
            if (children.Length == 0)
            {
                // just text inside the tag: find constructors with just one argument
                ConstructorInfo[] constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                    .Where(c => c.GetParameters().Length == 1)
                    .ToArray();

                foreach (ConstructorInfo constructor in constructors)
                {
                    ParameterInfo parameter = constructor.GetParameters()[0];
                    try
                    {
                        object[] arguments = new object[]
                        {
                            DeserializeObject(parameter.ParameterType, objectElement)
                        };
                        return constructor.Invoke(arguments);
                    }
                    catch (TypeMismatchException)
                    {
                        continue;
                    }
                }
            }
            else
            {
                ConstructorInfo[] constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                    .Where(c => c.GetParameters().Length == children.Length)
                    .ToArray();

                // find the first matching constructor
                foreach (ConstructorInfo constructor in constructors)
                {
                    ParameterInfo[] parameters = constructor.GetParameters();
                    bool argumentNamesMatch = parameters
                        .Select(p => p.Name)
                        .SequenceEqual(children.Select(c => c.Name), StringComparer.InvariantCultureIgnoreCase);
                    if (!argumentNamesMatch) continue;

                    object[] arguments;
                    try
                    {
                        arguments = Enumerable
                            .Range(0, parameters.Length)
                            .Select(i => DeserializeObject(parameters[i].ParameterType, children[i]))
                            .ToArray();
                    }
                    catch (TypeMismatchException) { break; }

                    return constructor.Invoke(arguments);
                }
            }

            throw new TypeMismatchException(type, objectElement.OuterXml);
        }
        
        /// <summary>
        /// Deserializes a delegate of a specified type.
        /// </summary>
        /// <remarks>
        /// This method finds a specified method, binds as many arguments as the delegateElement
        /// binds, and returns the delegate.
        /// The method must have the SerializationReferenceable attribute on it. Inheritance will
        /// not be taken into account to determine if the method has the attribute.
        /// </remarks>
        /// <typeparam name="T">The return type of the delegate</typeparam>
        /// <param name="target">The path of the method (in the form Namespace.Class.Method)</param>
        /// <param name="delegateElement">The XML element representing the delegate</param>
        /// <returns>A delegate that calls the specified method with curried arguments</returns>
        private Func<Entity, T> DeserializeDelegate<T>(string target, XmlElement delegateElement)
        {
            string className = target.Substring(0, target.LastIndexOf('.'));
            string methodName = target.Substring(target.LastIndexOf('.') + 1);
            Type holdingType = Type.GetType(className, true, true);
            MethodInfo method = holdingType.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public);
            if (method == null)
                throw new InvalidOperationException("Type {0} doesn't have a static {1} method".FormatInvariant(holdingType.Name, methodName));
            object[] attributes = method.GetCustomAttributes(typeof(SerializationReferenceableAttribute), false);
            if (attributes.Length == 0 || attributes.OfType<SerializationReferenceableAttribute>().Count() == 0)
                throw new InvalidOperationException("Cannot deserialize a delegate that doesn't have the SerializationReferenceable attribute");

            ParameterInfo[] parameters = method.GetParameters();
            Dictionary<string, int> nameToIndex = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < parameters.Length; i++)
                nameToIndex[parameters[i].Name] = i;

            object[] arguments = new object[parameters.Length];
            foreach (XmlElement argument in delegateElement.ChildNodes.OfType<XmlElement>())
            {
                string name = argument.Name;
                int index = nameToIndex[name];
                if (index == 0 || arguments[index] != null)
                    throw new InvalidOperationException("Multiple definitions of argument {0}".FormatInvariant(name));
                ParameterInfo parameter = parameters[index];
                arguments[index] = DeserializeObject(parameter.ParameterType, argument);
            }

            return delegate(Entity e)
            {
                arguments[0] = e;
                return (T)method.Invoke(null, arguments);
            };
        }
        #endregion
        #endregion
    }
}
