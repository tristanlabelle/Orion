using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Diagnostics;
using System.Reflection;
using Orion.Engine;
using System.Collections;

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
        private World world;
        private Func<Handle> handleGenerator;
        #endregion

        #region Constructors
        public XmlDeserializer(World world, Func<Handle> generator)
        {
            this.world = world;
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
        public Entity DeserializeEntity(string filePath, bool persistentOnly)
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
        public Entity DeserializeEntity(XmlElement entityElement, bool persistentOnly)
        {
            Debug.Assert(entityElement.Name == "Entity");

            Entity entity = new Entity(world, handleGenerator());
            Assembly currentAssembly = Assembly.GetExecutingAssembly();
            Type componentClass = typeof(Component);
            object[] constructorArguments = new object[] { entity };
            foreach (XmlElement componentElement in entityElement.ChildNodes.OfType<XmlElement>())
            {
                string name = componentElement.Name;
                Type componentType = currentAssembly.GetType(name, true, true);
                if (entity.HasComponent(componentType))
                    throw new InvalidOperationException("Trying to attach multiple components of the same type to an entity ");

                if (!componentClass.IsAssignableFrom(componentType))
                    throw new InvalidOperationException("Trying to instantiate a class that isn't assignable to Component");

                ConstructorInfo constructor = componentType.GetConstructor(componentConstructorTypeArguments);
                Component component = (Component)constructor.Invoke(constructorArguments);
                foreach (XmlElement fieldElement in componentElement.ChildNodes.OfType<XmlElement>())
                    DeserializeField(component, fieldElement, persistentOnly);
                entity.AddComponent(component);
            }
            return entity;
        }

        /// <summary>
        /// Deserializes a field of a certain component based on XML data.
        /// </summary>
        /// <param name="component">The component for which the field should be deserialized</param>
        /// <param name="fieldElement">The XML element for the field</param>
        /// <returns>An Entity object with the specs from the XML file</returns>
        private void DeserializeField(Component component, XmlElement fieldElement, bool persistentOnly)
        {
            string fieldName = fieldElement.Name;
            FieldInfo field = component.GetType().GetField(fieldName, fieldLookupFlags);
            if (field == null)
                throw new InvalidOperationException("Couldn't find a field associated to the {0} tag".FormatInvariant(fieldName));

            object[] attributes = field.GetCustomAttributes(typeof(PersistentAttribute), false);
            if (persistentOnly && attributes.Length != 1)
                throw new InvalidOperationException("Trying to deserialize a transient field in a persistent-only deserialization");

            Type fieldType = field.FieldType;
            if (typeof(ICollection).IsAssignableFrom(fieldType))
            {
                // The non-generic ICollection interface doesn't have an Add method; therefore, we need a method that
                // can use a generic version of ICollection. Because of that, it will need to be generic itself;
                // and we need to populate the type arguments with Type objects we won't know until runtime.
                // Therefore, we must use reflection.
                MethodInfo nongenericDeserializeCollection = typeof(XmlDeserializer).GetMethod("DeserializeCollection");
                MethodInfo deserializeCollection = nongenericDeserializeCollection.MakeGenericMethod(fieldType);
                deserializeCollection.Invoke(this, new object[] { component, field, fieldElement });
            }
            else
            {
                try
                {
                    object value = DeserializeObject(field.FieldType, fieldElement);
                    field.SetValue(component, value);
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
        /// <param name="field">The field representing the ICollection object</param>
        /// <param name="fieldElement">The XML collection description</param>
        private void DeserializeCollection<T>(Component component, FieldInfo field, XmlElement fieldElement)
        {
            IEnumerable<XmlElement> childItems = fieldElement
                .ChildNodes
                .OfType<XmlElement>()
                .Where(x => x.Name.Equals("Item", StringComparison.InvariantCultureIgnoreCase));

            ICollection<T> collection = (ICollection<T>)field.GetValue(component);
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
            if (type == typeof(string))
                return objectElement.InnerText;

            if (type == typeof(int))
            {
                int result;
                if (!int.TryParse(objectElement.InnerText, out result))
                    throw new TypeMismatchException(type, objectElement.InnerText);
                return result;
            }

            if (type == typeof(float))
            {
                float result;
                if (!float.TryParse(objectElement.InnerText, out result))
                    throw new TypeMismatchException(type, objectElement.InnerText);
                return result;
            }

            if (type == typeof(bool))
            {
                bool result;
                if (!bool.TryParse(objectElement.InnerText, out result))
                    throw new TypeMismatchException(type, objectElement.InnerText);
                return result;
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
                MethodInfo genericDeserializeDelegate = typeof(XmlDeserializer).GetMethod("DeserializeDelegate");
                MethodInfo method = genericDeserializeDelegate.MakeGenericMethod(type.GetMethod("Invoke").ReturnType);
                return method.Invoke(this, new object[] { target, objectElement });
            }

            // special Orion types (like references to unit types)
            // (no such type now)

            // complex types
            // those are harder because the XML serializer uses structural typing
            return DeserializeComplexType(type, objectElement);
        }

        /// <summary>
        /// Deserializes an object of a complex type.
        /// </summary>
        /// <remarks>
        /// Complex types are anything that is not a string, an integer or a real. They are created by
        /// finding a constructor with arguments that match the elements inside objectElement and
        /// using it. No further treatment is possible on objects created this way.
        /// </remarks>
        /// <param name="type">The type of the object to create</param>
        /// <param name="objectElement">The XML element representing the complex object</param>
        /// <returns>An object of the specified type, as described by the XML element</returns>
        private object DeserializeComplexType(Type type, XmlElement objectElement)
        {
            XmlElement[] children = objectElement.ChildNodes.OfType<XmlElement>().ToArray();
            ConstructorInfo[] constructors = type.GetConstructors(BindingFlags.Public)
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
            MethodInfo method = holdingType.GetMethod(methodName, BindingFlags.Static);
            object[] attributes = method.GetCustomAttributes(typeof(SerializationReferenceableAttribute), false);
            if (attributes.Length == 0)
                throw new InvalidOperationException("Cannot deserialize a delegate that doesn't have the SerializationReferenceable attribute");

            ParameterInfo[] parameters = method.GetParameters();
            Dictionary<string, int> nameToIndex = new Dictionary<string, int>();
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
