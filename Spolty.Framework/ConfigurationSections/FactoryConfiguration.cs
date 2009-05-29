using System.Configuration;

namespace Spolty.Framework.ConfigurationSections
{
    public class FactoryConfiguration : ConfigurationElement
    {
        private const string NameAttribute = "name";
        private const string TypeAttribute = "type";

        [ConfigurationProperty(NameAttribute, IsRequired = true, IsKey = true)]
        public string Name
        {
            get { return (string) this[NameAttribute]; }
            set { this[NameAttribute] = value; }
        }

        [ConfigurationProperty(TypeAttribute, IsRequired = true)]
        public string Type
        {
            get { return (string) this[TypeAttribute]; }
            set { this[TypeAttribute] = value; }
        }
    }
}