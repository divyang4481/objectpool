using System;
using System.Collections;
using System.Collections.Generic;

namespace SystemUtilities.Serialization
{
    public sealed class SerializableObject : DisposableObject, IEnumerable<SerializableMember>
    {
        private readonly int _count = 0;
        private readonly IDictionary<string, SerializableMember> _dict;
        private readonly object _obj;

        public SerializableObject(object obj)
        {
            ExceptionHelper.ThrowIfArgumentNull(obj, "obj");
            _obj = obj;
            SerializableMemberList serializableMembers = SerializationHelper.GetSerializableMembers(obj.GetType());
            _count = serializableMembers.Count;
            _dict = new Dictionary<string, SerializableMember>(_count);
            foreach (SerializableMember serializableMember in serializableMembers)
            {
                if (!_dict.ContainsKey(serializableMember.Name))
                    _dict.Add(serializableMember.Name, new SerializableMember(serializableMember, obj));
            }
        }

        public int Count
        {
            get
            {
                ExceptionHelper.ThrowIfDisposed(this);
                return _count;
            }
        }

        public object this[string name]
        {
            get
            {
                ExceptionHelper.ThrowIfDisposed(this);
                ExceptionHelper.ThrowIfArgumentNullOrEmptyString(name, "name");
                SerializableMember serializableMember;
                if (_dict.TryGetValue(name, out serializableMember))
                {
                    return serializableMember.Value;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                ExceptionHelper.ThrowIfDisposed(this);
                ExceptionHelper.ThrowIfArgumentNullOrEmptyString(name, "name");
                SerializableMember serializableMember;
                if (_dict.TryGetValue(name, out serializableMember))
                {
                    serializableMember.Value = value;
                }
            }
        }

        #region IEnumerable<SerializableMember> Members

        public IEnumerator<SerializableMember> GetEnumerator()
        {
            ExceptionHelper.ThrowIfDisposed(this);
            return _dict.Values.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
