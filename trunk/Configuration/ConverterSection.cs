using System;
using System.Configuration;

namespace SystemUtilities.Configuration
{
    public sealed class ConverterSection : ConfigurationSection
    {
        [ConfigurationProperty("name", DefaultValue = "DefaultConverter", IsRequired = true)]
        [StringValidator(MinLength = 1)]
        public string Name
        {
            get { return (string)base["name"]; }
            set { base["name"] = value; }
        }

        [ConfigurationProperty("type", DefaultValue = "SystemUtilities.Serialization.Converter", IsRequired = true)]
        [StringValidator(MinLength = 1)]
        public string Type
        {
            get { return (string)base["type"]; }
            set { base["type"] = value; }
        }
    }
}
