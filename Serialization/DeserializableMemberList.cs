using System;
using System.Collections;
using System.Collections.Generic;

namespace SystemUtilities.Serialization
{
    public abstract class DeserializableMemberList : IEnumerable<DeserializableMember>
    {
        public abstract int Count { get; }

        #region IEnumerable<DeserializableMember> Members

        public abstract IEnumerator<DeserializableMember> GetEnumerator();

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
