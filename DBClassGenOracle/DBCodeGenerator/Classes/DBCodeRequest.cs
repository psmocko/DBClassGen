using System;
using DBClassGen.Common.Classes;
using DBClassGen.Generator.Enumerations;

namespace DBClassGen.Generator.Classes {
    public class DBCodeRequest {
        public Languages Language { get; set; }
        public String ClassName { get; set; }
        public String FilePath { get; set; }
        public String BaseClassType { get; set; }
        public DBReturnTypes DBReturnType { get; set; }
        public String Namespace { get; set; }
        public bool GenerateGet { get; set; }
        public bool GenerateAdd { get; set; }
        public bool GenerateUpdate{ get; set; }
        public bool GenerateDelete { get; set; }

        public ServerInfo ServerInfo{ get; set; }
        public TableInfo TableInfo { get; set; }

    }
}
