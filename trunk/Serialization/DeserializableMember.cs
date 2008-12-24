using System;

namespace SystemUtilities.Serialization
{
    public sealed class DeserializableMember
    {
        private readonly string _name;
        private readonly object _value;

        public DeserializableMember(string name, object value)
        {
            ExceptionHelper.ThrowIfArgumentNullOrEmptyString(name, "name");
            _name = name;
            _value = value;
        }

        public string Name
        {
            get { return _name; }
        }

        public object Value
        {
            get { return _value; }
        }
    }
}
