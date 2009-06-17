using System.Configuration;
using Spolty.Framework.Exceptions;

namespace Spolty.Framework.ConfigurationSections
{
    public class SpoltyFrameworkSectionHandler : ConfigurationSection
    {
        private const string SectionName = "spolty.framework";
        private const string FactoriesElementName = "factories";
        private string _use;

        private static SpoltyFrameworkSectionHandler _instance;


        public static SpoltyFrameworkSectionHandler Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = (SpoltyFrameworkSectionHandler) ConfigurationManager.GetSection(SectionName);
                    _instance.Use = _instance.Factories.Use;
                }
                return _instance;
            }
        }

        [ConfigurationProperty(FactoriesElementName)]
        public FactoryConfigurationCollection Factories
        {
            get { return (FactoryConfigurationCollection) base[FactoriesElementName]; }
        }

        public string Use
        {
            get { return _use; }
            set { _use = value; }
        }
        
        public FactoryConfiguration UseFactory
        {
            get
            {
                if (string.IsNullOrEmpty(Use))
                {
                    return GetFactoryConfiguration(0);
                }
                return GetFactoryConfiguration(Use);
            }
        }

        protected internal FactoryConfiguration GetFactoryConfiguration(int index)
        {
            int i = 0;
            FactoryConfiguration result = null;
            foreach (FactoryConfiguration factoryConfiguration in Factories)
            {
                if (i == index)
                {
                    result = factoryConfiguration;
                }
                i++;
            }
            if (result == null)
            {
                throw new SpoltyException("FactoryConfiguration not found.");
            }
            return result;
        }


        protected internal FactoryConfiguration GetFactoryConfiguration(string name)
        {
            foreach (FactoryConfiguration entry in Factories)
            {
                if (name.Equals(entry.Name))
                {
                    return entry;
                }
            }
            return null;
        }


    }
}