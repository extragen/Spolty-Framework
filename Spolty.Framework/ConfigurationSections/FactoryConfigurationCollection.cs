using System.Configuration;

namespace Spolty.Framework.ConfigurationSections
{
    public class FactoryConfigurationCollection : ConfigurationElementCollection
    {
        public const string FirstValue = "first";
        private const string UseAttribute = "use";

        [ConfigurationProperty(UseAttribute)]
        public string Use
        {
            get { return (string) this[UseAttribute]; }
            set{ this[UseAttribute] = value; }
        }

        public FactoryConfiguration UseFactory
        {
            get
            {
                if (string.IsNullOrEmpty(Use))
                {
                    return (FactoryConfiguration) BaseGet(0);
                }
                return (FactoryConfiguration) BaseGet(Use);
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new FactoryConfiguration();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((FactoryConfiguration) element).Name;
        }
    }
}