using System;
using System.Configuration;
using DBClassGen.Common.Enumerations;

namespace DBClassGen.Config {
    public class ServerConfigurationElement : ConfigurationElement {
        private const String _nameProperty = "name";
        private const String _connectionStringNameProperty = "connectionStringName";
        private const String _serverTypeProperty="serverType";
        private const String _schemaFilters="Filters";

        [ConfigurationProperty(_nameProperty, IsRequired = true, IsKey = true),
        StringValidator(InvalidCharacters = @"~!@#$%^&*()[]{}/;'""|\")]
        public String Name {
            get {
                return this[_nameProperty] as String;
            }

            set { this[_nameProperty] = value; }

        }

        [ConfigurationProperty(_connectionStringNameProperty, IsRequired = true)]
        public String ConnectionStringName { get { return this[_connectionStringNameProperty] as String; } set { this[_connectionStringNameProperty] = value; }
        }

        // TODO :Write a customvalidator...
        [ConfigurationProperty(_serverTypeProperty, IsRequired=true)]
        public ServerTypes Type { get { return (ServerTypes)this[_serverTypeProperty]; } set { this[_serverTypeProperty] = value; } }

        [ConfigurationProperty(_schemaFilters)]
        public SchemaFilterElementCollection Filters { get { return this[_schemaFilters] as SchemaFilterElementCollection; } set { this[_schemaFilters] = value; } }
    }
}
