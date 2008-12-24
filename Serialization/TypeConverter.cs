using System;
using SystemUtilities.Configuration;

namespace SystemUtilities.Serialization
{
    internal static class TypeConverter
    {
        private static IConverter _converter = null;

        public static object ConvertTo(object obj, Type type)
        {
            LoadConverter();
            return _converter.ConvertTo(obj, type);
        }

        private static void LoadConverter()
        {
            if (_converter == null)
            {
                _converter = (IConverter)Activator.CreateInstance(ConfigurationManager.Converter.Type);
            }
        }
    }
}
