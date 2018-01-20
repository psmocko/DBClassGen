using System;
using System.Collections.Generic;
using DBClassGen.Common.Classes;

namespace DBClassGen.Common.Interfaces {
    public interface ISchemaRetriever {
        IEnumerable<ServerInfo> GetServers();
        IEnumerable<String> GetSchemas(ServerInfo server);
        IEnumerable<TableInfo> GetTables(ServerInfo server, string schema);
        IEnumerable<ColumnInfo> GetColumns(ServerInfo server, TableInfo table, string schema = null);
    }
}
