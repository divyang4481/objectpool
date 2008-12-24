using System;
using System.Configuration;

namespace SystemUtilities.Configuration
{
    public sealed class ThreadPoolSection : ConfigurationSection
    {
        [ConfigurationProperty("initialThreads", DefaultValue = "2", IsRequired = true)]
        [IntegerValidator(MinValue = 1)]
        public int InitialThreads
        {
            get { return (int)base["initialThreads"]; }
            set { base["initialThreads"] = value; }
        }

        [ConfigurationProperty("maxThreads", DefaultValue = "5", IsRequired = true)]
        [IntegerValidator(MinValue = 1)]
        public int MaxThreads
        {
            get { return (int)base["maxThreads"]; }
            set { base["maxThreads"] = value; }
        }

        [ConfigurationProperty("name", DefaultValue = "AsyncHelperThreadPool", IsRequired = true)]
        [StringValidator(MinLength = 1)]
        public string Name
        {
            get { return (string)base["name"]; }
            set { base["name"] = value; }
        }
    }
}
