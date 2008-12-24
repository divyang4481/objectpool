using System;
using System.Configuration;
using SystemUtilities.Serialization;

namespace SystemUtilities.Configuration
{
    internal sealed class ConverterSettings
    {
        private readonly string _name;
        private readonly Type _type;

        public ConverterSettings(ConverterSection settings)
        {
            if (settings == null)
            {
                _type = typeof(Converter);
            }
            else
            {
                _name = settings.Name;
                _type = Type.GetType(settings.Type);

                if (_type == null)
                    throw new ConfigurationErrorsException(String.Format(Resources.CouldNotFindType, settings.Type));

                if (_type.GetInterface("IConverter") == null)
                    throw new ConfigurationErrorsException(String.Format(Resources.TypeDoesNotImplementInterface, settings.Type, typeof(IConverter).FullName));
            }
        }

        public string Name
        {
            get { return _name; }
        }

        public Type Type
        {
            get { return _type; }
        }
    }
}
