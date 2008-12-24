using System;
using System.Collections.Generic;

namespace SystemUtilities.Serialization
{
    internal sealed class DeserializablObject : IDeserializable
    {
        private readonly string _assemblyName;
        private int _count = 0;
        private readonly ICollection<DeserializableMember> _collect;
        private readonly string _typeName;

        public DeserializablObject(string assemblyName, string typeName)
        {
            ExceptionHelper.ThrowIfArgumentNullOrEmptyString(assemblyName, "assemblyName");
            ExceptionHelper.ThrowIfArgumentNullOrEmptyString(typeName, "typeName");
            _assemblyName = assemblyName;
            _collect = new List<DeserializableMember>();
            _typeName = typeName;
        }

        #region IDeserializable Members

        public string AssemblyName
        {
            get { return _assemblyName; }
        }

        public Serialization.DeserializableMemberList DeserializableMembers
        {
            get { return new DeserializableMemberList(_collect, _count); }
        }

        public string TypeName
        {
            get { return _typeName; }
        }

        #endregion

        public void Add(DeserializableMember item)
        {
            ExceptionHelper.ThrowIfArgumentNull(item, "item");
            _collect.Add(item);
            _count++;
        }

        private sealed class DeserializableMemberList : Serialization.DeserializableMemberList
        {
            private readonly IEnumerable<DeserializableMember> _collect;
            private readonly int _count = 0;

            public DeserializableMemberList(IEnumerable<DeserializableMember> collect, int count)
            {
                _collect = collect;
                _count = count;
            }

            public override int Count
            {
                get { return _count; }
            }

            public override IEnumerator<DeserializableMember> GetEnumerator()
            {
                return _collect.GetEnumerator();
            }
        }
    }
}
