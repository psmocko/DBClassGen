using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace DBClassGen.Config {
    public class ServerConfigurationElementCollection : ConfigurationElementCollection {

        public ServerConfigurationElement this[int index]{
            get { return base.BaseGet(index) as ServerConfigurationElement; }

            set {
                if (base.BaseGet(index) != null)
                    base.BaseRemoveAt(index);

                BaseAdd(index, value);
            }
        }

        public new ServerConfigurationElement this[string name] {
            get { return base.BaseGet(name) as ServerConfigurationElement; }

            set {
                if (base.BaseGet(name) != null)
                    base.BaseRemove(name);

                BaseAdd(value);
            }
        }

        public override ConfigurationElementCollectionType CollectionType {
            get {
                return ConfigurationElementCollectionType.AddRemoveClearMap;
            }
        }

        protected override ConfigurationElement CreateNewElement() {
            return new ServerConfigurationElement();
        }

        protected override object GetElementKey(ConfigurationElement element) {
            return ((ServerConfigurationElement)element).Name;
        }

        public override bool IsReadOnly() {
            return false;
        }
    }
}
