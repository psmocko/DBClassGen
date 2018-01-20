using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using CoreDataAccess.Extensions;
using DBClassGen.Common.Classes;
using DBClassGen.Common.Enumerations;
using DBClassGen.Common.Interfaces;

namespace DBClassGen.Classes {
    public class SqlSchema : ISchemaRetriever {

        public IEnumerable<ServerInfo> GetServers() {
            var servers = new List<ServerInfo>();
            var sqlEnum = SqlDataSourceEnumerator.Instance;

            using (var dt = sqlEnum.GetDataSources())
            using (var dv = new DataView(dt)) {
                dv.Sort = "ServerName, InstanceName";
                foreach (DataRowView dr in dv) {

                    if (!dr.Row.IsNull("InstanceName") && dr["InstanceName"].ToString().Trim() != String.Empty) {
                        servers.Add(new ServerInfo() {
                            Name = String.Format(@"{0}\{1}", dr["ServerName"], dr["InstanceName"]),
                            ServerType = ServerTypes.SqlServer
                        });
                    }
                    else {
                        servers.Add(new ServerInfo() {
                            Name = dr["ServerName"].ToString(),
                            ServerType = ServerTypes.SqlServer
                        });

                    }

                }
            }

            return servers;
        }

        public IEnumerable<string> GetSchemas(ServerInfo server) {
            var schemas=new List<String>();

            try{
                using(var con = GetConnection(server)){
                    con.Open();
                    using(var dt =con.GetSchema(SqlClientMetaDataCollectionNames.Databases))
                    using(var dv = new DataView(dt)){
                        dv.Sort=(dt.Columns[0].ColumnName);
                        foreach(DataRowView dr in dv){
                            if (server.SchemaFilters != null && server.SchemaFilters.Count > 0) {
                                if (server.SchemaFilters.Contains(dr[0].ToString()))
                                    schemas.Add(dr[0].ToString());
                            }
                            else
                                schemas.Add(dr[0].ToString());
                        }
                    }
                }
            }
            catch(Exception ex){
                schemas.Add(String.Format("Error connecting to database server. {0}", ex.Message));    
                
            }
            

            return schemas;
        }

        public IEnumerable<TableInfo> GetTables(ServerInfo server, string schema) {
            // set up restrictions
            var rest=new []{schema, "dbo", null, "BASE TABLE"};
            var tables=new List<TableInfo>();

            try {
                using (var con = GetConnection(server, schema)) {
                    con.Open();
                    using (var dt = con.GetSchema(SqlClientMetaDataCollectionNames.Tables, rest))
                    using (var dv = new DataView(dt)) {
                        dv.Sort = dt.Columns[2].ColumnName;
                        foreach (DataRowView dr in dv) {
                            var tableInfo = new TableInfo(dr[1].ToString(), dr[2].ToString(), dr[3].ToString(), schema);
                            tableInfo.Keys = GetTableKeyInfo(server, tableInfo);
                            tables.Add(tableInfo);
                        }
                    }
                }
            }
            catch(Exception){
                
            }

            return tables;
        }

        public IEnumerable<ColumnInfo> GetColumns(ServerInfo server, TableInfo table, string schema) {
            var cols=new List<ColumnInfo>();
            try{
                var keys=table.Keys.ToList();
                using (var con = GetConnection(server, schema ?? table.Database))
                using (var cmd = con.CreateCommand()) {
                    cmd.CommandText = String.Format("Select * from [{0}]", table.TableName);
                    cmd.CommandType = CommandType.Text;
                    con.Open();
                    using (var rdr = cmd.ExecuteReader(CommandBehavior.KeyInfo))
                    using (var dt = rdr.GetSchemaTable()) {
                        foreach (DataRow dr in dt.Rows) {

                            var colInfo=new ColumnInfo(){

                                                            ColumnName=dr["ColumnName"].ToString(),
                                                            Ordinal=dr.GetInt32("ColumnOrdinal"),
                                                            Size=dr.GetInt32("ColumnSize"),
                                                            Precision=dr.GetInt32OrNull("NumericPrecision"),
                                                            Scale=dr.GetInt32OrNull("NumericScale"),
                                                            AllowDBNull=Convert.ToBoolean(dr["AllowDBNull"]),
                                                            IsKey=Convert.ToBoolean(dr["IsKey"]),
                                                            IsUnique=Convert.ToBoolean(dr["IsUnique"]),
                                                            DataType=Type.GetType(dr["DataType"].ToString()),
                                                            IsRowId=Convert.ToBoolean(dr["IsRowVersion"]),
                                                            IsIdentity=Convert.ToBoolean(dr["IsIdentity"]),
                                                            DbDataType=dr["ProviderSpecificDataType"] as string
                                                        };
                            colInfo.IsForeignKey = keys.Any(k => k.ColumnName.Equals(colInfo.ColumnName, StringComparison.InvariantCultureIgnoreCase) && k.KeyType == KeyConstraintTypes.Foreign);
                            cols.Add(colInfo);
                        }

                    }
                }
            }
            catch(Exception ex){
                Debug.Write(ex.Message);
            }
          
            return cols;
        }

        private static SqlConnection GetConnection(ServerInfo server, String database = null) {
                  if (!String.IsNullOrWhiteSpace(server.ConnectionStringName)){
                var conString=ConfigurationManager.ConnectionStrings[server.ConnectionStringName];
                return new SqlConnection(conString.ConnectionString);
            }

            return GetDefaultServerConnection(server, database);                                                                      
        }

        private static SqlConnection GetDefaultServerConnection(ServerInfo server, String database=null) {
            var cb=new SqlConnectionStringBuilder();
            cb.DataSource = server.Name;
            if (!string.IsNullOrWhiteSpace(database))
                cb.InitialCatalog = database;
            else
                cb.InitialCatalog = "";
            cb.IntegratedSecurity = true;
            return new SqlConnection(cb.ConnectionString);
        }

        private static IEnumerable<KeyInfo> GetTableKeyInfo(ServerInfo server, TableInfo table) {            
            var keys = new List<KeyInfo>();

            //TODO:Figure out how to get foreign key info for tables...

            return keys;
        }

    }
}
