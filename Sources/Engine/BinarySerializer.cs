using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Reflection;
using Orion.Engine.Collections;

namespace Orion.Engine
{
    /// <summary>
    /// Provides services to serialize and deserialize objects from classes
    /// derived from a given type to binary streams of data.
    /// </summary>
    /// <remarks>
    /// Classes that can be serialized by this classes must adhere to a static interface
    /// which provides a Serialize and a Deserialize method.
    /// </remarks>
    public sealed class BinarySerializer<TBase> where TBase : class
    {
        #region Nested Types
        private struct Entry
        {
            public readonly Type Type;
            public readonly MethodInfo Serializer;
            public readonly Func<BinaryReader, TBase> Deserializer;

            public Entry(Type type, MethodInfo serializer, Func<BinaryReader, TBase> deserializer)
            {
                this.Type = type;
                this.Serializer = serializer;
                this.Deserializer = deserializer;
            }
        }
        #endregion

        #region Instance
        #region Fields
        private readonly List<Entry> entries;
        private readonly object[] tempSerializeMethodArguments = new object[2];
        private readonly MemoryStream tempMemoryStream;
        private readonly BinaryWriter tempBinaryWriter;
        #endregion

        #region Constructor
        public BinarySerializer(IEnumerable<Type> types)
        {
            Argument.EnsureNotNull(types, "types");

            entries = new List<Entry>();
            tempMemoryStream = new MemoryStream();
            tempBinaryWriter = new BinaryWriter(tempMemoryStream);

            var serializeMethodArgumentTypes = new[] { null, typeof(BinaryWriter) };
            var deserializeMethodArgumentTypes = new[] { typeof(BinaryReader) };
            foreach (Type type in types.OrderBy(type => type.FullName))
            {
                if (type == null) throw new ArgumentNullException("types", "A type within the sequence was null.");
                
                bool alreadyRegistered = entries.Any(e => e.Type == type);
                if (alreadyRegistered) continue;
                
                Assert(type.IsPublic, type, "Type must be public.");

                serializeMethodArgumentTypes[0] = type;
                MethodInfo serializeMethod = type.GetMethod("Serialize",
                    BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly,
                    Type.DefaultBinder, serializeMethodArgumentTypes, null);
                Assert(serializeMethod != null, type,
                    "Type lacks a static Serialize method.".FormatInvariant(type.FullName));

                MethodInfo deserializeMethod = type.GetMethod("Deserialize",
                    BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly,
                    Type.DefaultBinder, deserializeMethodArgumentTypes, null);
                Assert(deserializeMethod != null, type,
                    "Type lacks a static  Deserialize method.".FormatInvariant(type.FullName));
                Assert(typeof(TBase).IsAssignableFrom(deserializeMethod.ReturnType), type,
                    "Type deserialization method does not return {0}.".FormatInvariant(typeof(TBase).FullName));

                var deserializer = (Func<BinaryReader, TBase>)
                    Delegate.CreateDelegate(typeof(Func<BinaryReader, TBase>), deserializeMethod);
                entries.Add(new Entry(type, serializeMethod, deserializer));
            }

            if (entries.Count > byte.MaxValue)
                throw new NotSupportedException("Too many serializable types.");
        }
        #endregion

        #region Methods
        #region Serialization
        /// <summary>
        /// Serializes an object to a binary data stream.
        /// </summary>
        /// <param name="object">The object to be serialized.</param>
        /// <param name="writer">A <see cref="BinaryWriter"/> to which data should be written.</param>
        public void Serialize(TBase @object, BinaryWriter writer)
        {
            Argument.EnsureNotNull(@object, "object");
            Argument.EnsureNotNull(writer, "writer");

            Type type = GetType();
            int index = entries.IndexOf(t => t.Type == type);
            if (index == -1)
            {
                throw new InvalidOperationException(
                    "Cannot serialize objects of type {0} as it was not registered to the serialization system."
                    .FormatInvariant(type.FullName));
            }

            writer.Write((byte)index);

            tempSerializeMethodArguments[0] = @object;
            tempSerializeMethodArguments[1] = writer;
            entries[index].Serializer.Invoke(null, tempSerializeMethodArguments);
        }

        /// <summary>
        /// Serializes an object to a binary representation.
        /// </summary>
        /// <param name="object">The object to be serialized.</param>
        /// <returns>The serialized object data.</returns>
        public byte[] Serialize(TBase @object)
        {
            Argument.EnsureNotNull(@object, "object");

            tempMemoryStream.SetLength(0);
            tempMemoryStream.Position = 0;
            Serialize(@object, tempBinaryWriter);
            tempBinaryWriter.Flush();

            return tempMemoryStream.ToArray();
        }
        #endregion

        #region Deserialization
        /// <summary>
        /// Deserializes an object from a stream of data.
        /// </summary>
        /// <param name="reader">A <see cref="BinaryReader"/> over a stream of data.</param>
        /// <returns>The object that was deserialized.</returns>
        public TBase Deserialize(BinaryReader reader)
        {
            Argument.EnsureNotNull(reader, "reader");

            byte code = reader.ReadByte();
            if (code >= entries.Count) throw new InvalidDataException("Invalid object type code.");
            return entries[code].Deserializer(reader);
        }

        /// <summary>
        /// Deserializes an object from a binary representation.
        /// </summary>
        /// <param name="buffer">A buffer of data containing a binary representation.</param>
        /// <returns>The object that was deserialized.</returns>
        public TBase Deserialize(Subarray<byte> buffer)
        {
            Argument.EnsureNotNull(buffer.Array, "buffer.Array");

            var stream = new MemoryStream(buffer.Array, buffer.Offset, buffer.Count, false, false);
            var reader = new BinaryReader(stream);
            return Deserialize(reader);
        }

        /// <summary>
        /// Deserializes objects until the end of the stream is reached.
        /// </summary>
        /// <param name="reader">The <see cref="BinaryReader"/> to be used.</param>
        /// <returns>The collection of deserialized objects.</returns>
        public List<TBase> DeserializeToEnd(BinaryReader reader)
        {
            Argument.EnsureNotNull(reader, "reader");

            List<TBase> objects = new List<TBase>();

            var stream = reader.BaseStream;
            while (stream.Position < stream.Length)
            {
                TBase @object = Deserialize(reader);
                objects.Add(@object);
            }

            return objects;
        }

        /// <summary>
        /// Deserializes objects until the end of the stream is reached.
        /// </summary>
        /// <param name="buffer">A buffer of data containing the binary representations of objects.</param>
        /// <returns>The collection of deserialized objects.</returns>
        public List<TBase> DeserializeToEnd(Subarray<byte> buffer)
        {
            Argument.EnsureNotNull(buffer.Array, "buffer.Array");

            var stream = new MemoryStream(buffer.Array, buffer.Offset, buffer.Count, false, false);
            var reader = new BinaryReader(stream);
            return DeserializeToEnd(reader);
        }
        #endregion

        private static void Assert(bool condition, Type type, string message)
        {
            if (condition) return;
            throw new ArgumentException("Type {0} cannot be serialized. ".FormatInvariant(type.FullName) + message, "types");
        }
        #endregion
        #endregion

        #region Static
        #region Methods
        /// <summary>
        /// Creates a new <see cref="BinarySerializer{TBase}"/> from the types exported from an assembly.
        /// </summary>
        /// <param name="assembly">The assembly to be reflected for exported types.</param>
        /// <returns>A newly created <see cref="BinarySerializer{TBase}"/> with those types.</returns>
        public static BinarySerializer<TBase> FromExportedTypes(Assembly assembly)
        {
            Argument.EnsureNotNull(assembly, "assembly");

            var types = assembly.GetExportedTypes()
                .Where(type => !type.IsAbstract && typeof(TBase).IsAssignableFrom(type));
            return new BinarySerializer<TBase>(types);
        }

        /// <summary>
        /// Creates a new <see cref="BinarySerializer{TBase}"/> from the types exported from the calling assembly.
        /// </summary>
        /// <returns>A newly created <see cref="BinarySerializer{TBase}"/> with those types.</returns>
        public static BinarySerializer<TBase> FromCallingAssemblyExportedTypes()
        {
            Assembly assembly = Assembly.GetCallingAssembly();
            return FromExportedTypes(assembly);
        }
        #endregion
        #endregion
    }
}
