using System;

namespace SystemUtilities.Serialization
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public sealed class SerializedAttribute : Attribute
    {
        private string _name = null;

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
    }
}
