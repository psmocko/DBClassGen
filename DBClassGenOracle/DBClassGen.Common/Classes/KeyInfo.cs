using System;
using DBClassGen.Common.Enumerations;

namespace DBClassGen.Common.Classes {
    public class KeyInfo {
        public String ConstraintName { get; set; }
        public String ColumnName { get; set; }
        public KeyConstraintTypes KeyType { get; set; }

        public KeyInfo() {}
        public KeyInfo(String constraintName, String columnName, KeyConstraintTypes keyType){
            ConstraintName=constraintName;
            ColumnName = columnName;
            KeyType = keyType;
        }
    }
}
