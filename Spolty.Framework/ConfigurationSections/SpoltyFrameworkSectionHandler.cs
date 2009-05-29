using System.Configuration;

namespace Spolty.Framework.ConfigurationSections
{
    public class SpoltyFrameworkSectionHandler : ConfigurationSection
    {
        private const string SectionName = "spolty.framework";
        private const string FactoriesElementName = "factories";
        private static SpoltyFrameworkSectionHandler _instance;


        public static SpoltyFrameworkSectionHandler Instance
        {
            get
            {
                _instance = _instance ?? ConfigurationManager.GetSection(SectionName) as SpoltyFrameworkSectionHandler;
                return _instance;
            }
        }

        [ConfigurationProperty(FactoriesElementName)]
        public FactoryConfigurationCollection Factories
        {
            get { return (FactoryConfigurationCollection) base[FactoriesElementName]; }
        }
    }
}