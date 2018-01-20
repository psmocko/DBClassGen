using System;
using System.Configuration;

namespace DBClassGen.Config {
    public class ServerConfigurationSection : ConfigurationSection {
        private const String _serversProperty = "Servers";
        public const String SectionName = "ServerConfiguration";

        public ServerConfigurationSection() { }

        public static ServerConfigurationSection GetConfig() {
            
                //var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                //var serverSection = config.GetSection(SectionName) as ServerConfigurationSection;
                //return serverSection;
            return ConfigurationManager.GetSection(SectionName) as ServerConfigurationSection ?? new ServerConfigurationSection();
           
        }

        [ConfigurationProperty(_serversProperty)]
        public ServerConfigurationElementCollection Servers {
            get { return this[_serversProperty] as ServerConfigurationElementCollection ?? new ServerConfigurationElementCollection(); }
        }


        public override bool IsReadOnly() {

            return false;
        }

    }
}
