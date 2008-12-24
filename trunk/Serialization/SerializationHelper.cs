using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using SystemUtilities.Caching;

namespace SystemUtilities.Serialization
{
    public static class SerializationHelper
    {
        private static readonly Cache<string, Assembly> _assemblyCache = new Cache<string, Assembly>(0);
        private static readonly Cache<string, Type> _typeCache = new Cache<string, Type>(0);
        private static readonly Cache<Type, SerializableMemberList> _serializableMembersCache = new Cache<Type, SerializableMemberList>(0);

        public static object Clone(object obj)
        {
            ExceptionHelper.ThrowIfArgumentNull(obj, "obj");
            IFormatter formatter = new BinaryFormatter();
            using (Stream stream = new MemoryStream())
            {
                formatter.Serialize(stream, obj);
                stream.Seek(0, SeekOrigin.Begin);
                return formatter.Deserialize(stream);
            }
        }

        public static T Clone<T>(T obj)
        {
            return (T)Clone((Object)obj);
        }

        public static object CreateInstance(string assemblyString, string typeName)
        {
            ExceptionHelper.ThrowIfArgumentNullOrEmptyString(assemblyString, "assemblyString");
            ExceptionHelper.ThrowIfArgumentNullOrEmptyString(typeName, "typeName");
            return CreateInstance(GetType(assemblyString, typeName));
        }

        public static T CreateInstance<T>()
        {
            return (T)CreateInstance(typeof(T));
        }

        public static object CreateInstance(Type type)
        {
            ExceptionHelper.ThrowIfArgumentNull(type, "type");
            return FormatterServices.GetSafeUninitializedObject(type);
        }

        public static object Deserialize(IDeserializable source)
        {
            ExceptionHelper.ThrowIfArgumentNull(source, "source");
            object result = CreateInstance(source.AssemblyName, source.TypeName);
            {
                using (SerializableObject serializableObject = new SerializableObject(result))
                {
                    DeserializableMemberList members = source.DeserializableMembers;
                    if (members != null && members.Count > 0)
                    {
                        foreach (DeserializableMember member in members)
                        {
                            serializableObject[member.Name] = member.Value;
                        }
                    }
                }
            }
            return result;
        }

        public static T Deserialize<T>(IDeserializable source)
        {
            return (T)Deserialize(source);
        }

        private static Assembly GetAssembly(string assemblyString)
        {
            Assembly assembly = _assemblyCache[assemblyString];
            if (assembly == null)
            {
                assembly = Assembly.Load(assemblyString);
                _assemblyCache[assemblyString] = assembly;
            }
            return assembly;
        }

        internal static string GetSerializableMemberName(MemberInfo member)
        {
            ExceptionHelper.ThrowIfArgumentNull(member, "member");
            SerializedAttribute attribute;
            if (TryGetSerializedAttribute(member, out attribute))
            {
                return attribute.Name;
            }
            else
            {
                return member.Name;
            }
        }

        internal static SerializableMemberList GetSerializableMembers(Type type)
        {
            ExceptionHelper.ThrowIfArgumentNull(type, "type");
            SerializableMemberList serializableMembers = _serializableMembersCache[type];
            if (serializableMembers == null)
            {
                serializableMembers = new SerializableMemberList(FormatterServices.GetSerializableMembers(type));
                _serializableMembersCache[type] = serializableMembers;
            }
            return serializableMembers;
        }

        private static Type GetType(string assemblyString, string name)
        {
            Type type = _typeCache[name];
            if (type == null)
            {
                type = FormatterServices.GetTypeFromAssembly(GetAssembly(assemblyString), name);
                _typeCache[name] = type;
            }
            return type;
        }

        private static bool TryGetSerializedAttribute(MemberInfo member, out SerializedAttribute attribute)
        {
            object[] attributes = member.GetCustomAttributes(typeof(SerializedAttribute), false);
            if (attributes.Length > 0)
            {
                attribute = (SerializedAttribute)attributes[0];
                return true;
            }
            else
            {
                attribute = null;
                return false;
            }
        }
    }
}
