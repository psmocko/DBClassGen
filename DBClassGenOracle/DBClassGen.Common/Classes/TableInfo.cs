using System;
using System.Collections.Generic;
using DBClassGen.Common.Interfaces;

namespace DBClassGen.Common.Classes {
    public class TableInfo{

        public String Owner { get; private set; }
        public string Database { get; private set; }
        public String TableName { get; private set; }
        public String Type { get; private set; }
        public IEnumerable<ColumnInfo> Columns { get; set; }
        public IEnumerable<KeyInfo> Keys { get; set; }

        
        public TableInfo(String owner, String tableName, String type, string databaseName = null){
            ////_server = server;
            Owner = owner;
            TableName=tableName;
            Type = type;

        }
    }
}
