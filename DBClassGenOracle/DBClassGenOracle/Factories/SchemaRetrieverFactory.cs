using System;
using DBClassGen.Classes;
using DBClassGen.Common.Classes;
using DBClassGen.Common.Enumerations;
using DBClassGen.Common.Interfaces;

namespace DBClassGen.Factories {
    public class SchemaRetrieverFactory {
        
        public static ISchemaRetriever GetSchemaRetriever(ServerInfo server){
            return GetSchemaRetriever(server.ServerType);
        }

        public static ISchemaRetriever GetSchemaRetriever(ServerTypes serverType) {
            switch (serverType) {
                case ServerTypes.SqlServer:
                    return new SqlSchema();
                case ServerTypes.Oracle:
                    return new OracleSchema();
                default:
                    throw new ArgumentOutOfRangeException("Invalid Server Info.", "server");
            }
        }

    }
}
