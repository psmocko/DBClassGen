using System;
using System.Collections.Generic;
using DBClassGen.Common.Enumerations;

namespace DBClassGen.Common.Classes {
    public class ServerInfo {

        public String Name { get; set; }
        public String ConnectionStringName { get; set; }
        public ServerTypes ServerType{get; set;}
        public List<String> SchemaFilters { get; set; }
        
    }
}
