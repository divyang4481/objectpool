using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

namespace SystemUtilities.Serialization
{
    internal sealed class SerializableMemberList : IEnumerable<SerializableMember>
    {
        private readonly ICollection<SerializableMember> _collect;
        private readonly int _count = 0;

        public SerializableMemberList(MemberInfo[] members)
        {
            ExceptionHelper.ThrowIfArgumentNull(members, "members");
            _count = members.Length;
            _collect = new List<SerializableMember>(_count);
            for (int i = 0; i < _count; i++)
            {
                FieldInfo field = (FieldInfo)members[i];
                _collect.Add(new SerializableMember(field));
            }
        }

        public int Count
        {
            get { return _count; }
        }

        #region IEnumerable<SerializableMember> Members

        public IEnumerator<SerializableMember> GetEnumerator()
        {
            return _collect.GetEnumerator();
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
