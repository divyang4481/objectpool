using System;
using System.Reflection;

namespace SystemUtilities.Serialization
{
    public sealed class SerializableMember
    {
        private readonly FieldInfo _field;
        private readonly string _name;
        private readonly object _obj;

        internal SerializableMember(FieldInfo field)
        {
            ExceptionHelper.ThrowIfArgumentNull(field, "field");
            _field = field;
            _name = SerializationHelper.GetSerializableMemberName(field);
            _obj = null;
        }

        internal SerializableMember(SerializableMember serializableMember, object obj)
        {
            ExceptionHelper.ThrowIfArgumentNull(serializableMember, "serializableMember");
            ExceptionHelper.ThrowIfArgumentNull(obj, "obj");
            _field = serializableMember._field;
            _name = serializableMember._name;
            _obj = obj;
        }

        public string Name
        {
            get { return _name; }
        }

        public Type Type
        {
            get { return _field.FieldType; }
        }

        public object Value
        {
            get
            {
                if (_obj == null)
                {
                    return null;
                }
                return _field.GetValue(_obj);
            }
            set
            {
                if (_obj == null)
                {
                    return;
                }
                if (value == null || !Type.Equals(value.GetType(), _field.FieldType))
                {
                    value = TypeConverter.ConvertTo(value, _field.FieldType);
                }
                _field.SetValue(_obj, value);
            }
        }
    }
}
