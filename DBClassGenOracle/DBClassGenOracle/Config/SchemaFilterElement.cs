using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace DBClassGen.Config {
    public class SchemaFilterElement : ConfigurationElement {

        private const String _nameProperty = "name";

        [ConfigurationProperty(_nameProperty, IsRequired = true, IsKey = true),
        StringValidator(InvalidCharacters = @"~!@#$%^&*()[]{}/;'""|\")]
        public String Name {
            get {
                return this[_nameProperty] as String;
            }

            set { this[_nameProperty] = value; }

        }
    }
}
