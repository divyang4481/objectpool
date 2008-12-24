using System;

namespace SystemUtilities.Serialization
{
    public interface IDeserializable
    {
        string AssemblyName { get; }
        DeserializableMemberList DeserializableMembers { get; }
        string TypeName { get; }
    }
}
