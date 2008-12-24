using System;

namespace SystemUtilities.Serialization
{
    public interface IConverter
    {
        object ConvertTo(object obj, Type type);
    }
}
