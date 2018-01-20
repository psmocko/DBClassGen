using System.Configuration;

namespace DBClassGen.Config {
    public class SchemaFilterElementCollection : ConfigurationElementCollection {

        public SchemaFilterElement this[int index] {
            get { return base.BaseGet(index) as SchemaFilterElement; }

            set {
                if (base.BaseGet(index) != null)
                    base.BaseRemoveAt(index);

                BaseAdd(index, value);
            }
        }

        public new SchemaFilterElement this[string name] {
            get { return base.BaseGet(name) as SchemaFilterElement; }

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
            return new SchemaFilterElement();
        }

        protected override object GetElementKey(ConfigurationElement element) {
            return ((SchemaFilterElement)element).Name;
        }

        public override bool IsReadOnly() {
            return false;
        }
    }
}
