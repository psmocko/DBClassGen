using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using CoreDataAccess.Extensions;
using DBClassGen.Common.Classes;
using DBClassGen.Common.Enumerations;
using DBClassGen.Common.Interfaces;
using Oracle.DataAccess.Client;

namespace DBClassGen.Classes {
    public class OracleSchema :  ISchemaRetriever {

        public IEnumerable<ServerInfo> GetServers() {
            throw new NotImplementedException(); // not a way to enumerate servers...
        }

        public IEnumerable<string> GetSchemas(ServerInfo server) {
            var schemas = new List<string>();

            using (var con = GetConnection(server)) {
                con.Open();
                using (var dt = con.GetSchema("Users")) {
                    using (var dv = new DataView(dt)) {
                        dv.Sort = dt.Columns[0].ColumnName;                      
                        foreach (DataRowView dr in dv) {
                            if (server.SchemaFilters.Count > 0){
                                if (server.SchemaFilters.Contains(dr[0].ToString()))
                                    schemas.Add(dr[0].ToString());
                            }
                            else
                                //Console.WriteLine(dr[0].ToString());
                                schemas.Add(dr[0].ToString());
                        }
                    }
                }

            }

            return schemas;
        }

        public IEnumerable<TableInfo> GetTables(ServerInfo server, string schema) {
            var tables = new List<TableInfo>();

            using (var con = GetConnection(server)) {
                con.Open();
                using (var dt = con.GetSchema("Tables", new[] { schema })) {
                    using (var dv = new DataView(dt)) {
                        dv.Sort = dt.Columns[0].ColumnName;
                        foreach (DataRowView dr in dv) {
                            var tableInfo=new TableInfo(dr[0].ToString(), dr[1].ToString(), dr[2].ToString());
                            tableInfo.Keys=GetTableKeyInfo(server, tableInfo);
                            tables.Add(tableInfo);
                        }
                    }
                }

            }

            return tables;
        }

        public IEnumerable<ColumnInfo> GetColumns(ServerInfo server, TableInfo table, string schema) {
            
            var cols=new List<ColumnInfo>();
            var keys = table.Keys.ToList();
            using (var con = GetConnection(server))
            using (var cmd = con.CreateCommand()) {
                cmd.CommandText=String.Format("Select * from {0}.{1}", table.Owner, table.TableName);
                cmd.CommandType = CommandType.Text;
                con.Open();
                
                using (var rdr = cmd.ExecuteReader(CommandBehavior.KeyInfo)){
                    var dt=rdr.GetSchemaTable();
                    
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
                                                        IsRowId=Convert.ToBoolean(dr["IsRowID"]),
                                                        DbDataType=ConvertSystemTypeToDbType(dr["DataType"].ToString())
                                                    };
                        colInfo.IsKey=colInfo.IsKey|keys.Any(k => k.ColumnName.Equals(colInfo.ColumnName, StringComparison.InvariantCultureIgnoreCase) && k.KeyType==KeyConstraintTypes.Primary);
                        colInfo.IsForeignKey=keys.Any(k => k.ColumnName.Equals(colInfo.ColumnName, StringComparison.InvariantCultureIgnoreCase) && k.KeyType==KeyConstraintTypes.Foreign);
                        cols.Add(colInfo);                        

                    }

                }
                

            }

            table.Columns = cols;

            return cols;
        }

        private static string ConvertSystemTypeToDbType(string typeName) {

            var sType=typeName.Remove(typeName.IndexOf("System."), 7);
            switch (sType) {
                case "String":
                    return "OracleDbType.Varchar2";
                case "Int32":
                    return "OracleDbType.Int32";
                case"Int16":
                    return "OracleDbType.Int16";
                case "Int64":
                    return "OracleDbType.Int64";
                case"Byte":
                    return "OracleDbType.Byte";
                case "StringFixedLength":
                    return "OracleDbType.Char";
                case"Date":
                    return "OracleDbType.Date";
                case "Double":
                    return "OracleDbType.Double";
                case "TimeSpan":
                    return "OracleDbType.IntervalDS";
                case "Decimal":
                    return "OracleDbType.Decimal";
                case "DateTime":
                    return "OracleDbType.TimeStamp";
                case "Single":
                    return "OracleDbType.Single";

                default:
                    throw new ArgumentOutOfRangeException(String.Format("Type {0} not supported.", typeName), "typeName");
            }
        }

        private static OracleConnection GetConnection(ServerInfo server) {            
            var conString=ConfigurationManager.ConnectionStrings[server.ConnectionStringName];
            
            return new OracleConnection(conString.ConnectionString);
        }

        private static IEnumerable<KeyInfo> GetTableKeyInfo(ServerInfo server, TableInfo table) {
            var keys = new List<KeyInfo>();
            using (var con = GetConnection(server))
            using (var cmd = con.CreateCommand()) {
                cmd.CommandText = @"select cols.constraint_name, cols.column_name, cons.constraint_type
                                    from dba_constraints cons
                                        inner join dba_cons_columns cols on cons.owner = cols.owner and 
                                            cons.constraint_name = cols.constraint_name and cons.table_name = cols.table_name
                                    where cons.owner = :tableowner and cons.table_name = :tablename and (cons.constraint_type='P' or cons.constraint_type='R') and cons.status='ENABLED'";
                cmd.BindByName = true;
                cmd.CommandType = CommandType.Text;

                cmd.Parameters.Add("tableowner", OracleDbType.Varchar2).Value = table.Owner;
                cmd.Parameters.Add("tablename", OracleDbType.Varchar2).Value = table.TableName;
                //cmd.Parameters.Add("constrainttype", OracleDbType.Varchar2).Value = constraintType;
                con.Open();
                using (var rdr = cmd.ExecuteReader()) {
                    while (rdr.Read()) {
                        keys.Add(new KeyInfo() {
                            ConstraintName = rdr.GetString(0),
                            ColumnName = rdr.GetString(1),
                            KeyType = GetKeyTypeFromValue(rdr.GetString(2))
                        });
                    }
                }
                con.Close();
            }
            return keys;
        }

        private static KeyConstraintTypes GetKeyTypeFromValue(string keyType) {
            switch (keyType) {
                case "P":
                    return KeyConstraintTypes.Primary;
                case "R":
                    return KeyConstraintTypes.Foreign;
                default:
                    return KeyConstraintTypes.Unknown;
            }
        }

        #region Depricated
        
        //public static IEnumerable<String> GetSchemas(string server) {
        //    var schemas = new List<string>();

        //    using (var con = GetConnection(server, string.Empty)) {
        //        con.Open();
        //        using (var dt = con.GetSchema("Users")) {
        //            using (var dv = new DataView(dt)) {
        //                dv.Sort = dt.Columns[0].ColumnName;
        //                foreach (DataRowView dr in dv) {
        //                    Console.WriteLine(dr[0].ToString());
        //                    schemas.Add(dr[0].ToString());
        //                }
        //            }
        //        }

        //    }

        //    return schemas;
        //}

        //public static IEnumerable<TableInfo> GetTables(string server, string schema){
        //    var tables = new List<TableInfo>();

        //    using (var con = GetConnection(server, string.Empty)) {
        //        con.Open();
        //        using (var dt = con.GetSchema("Tables", new[]{schema})) {
        //            using (var dv = new DataView(dt)) {
        //                dv.Sort = dt.Columns[0].ColumnName;
        //                foreach (DataRowView dr in dv) {
        //                    //Console.WriteLine(dr[0].ToString());
        //                    //tables.Add(String.Format("{0}.{1}", dr[0].ToString(), dr[1].ToString()));
        //                    tables.Add(new TableInfo(dr[0].ToString(), dr[1].ToString(), dr[2].ToString()));
        //                }
        //            }
        //        }

        //    }

        //    return tables;
        //}

        //public static IEnumerable<ColumnInfo> GetColumns(string server, TableInfo table){
        //    var cols=new List<ColumnInfo>();
        //    var keys = GetTableKeyInfo(server, table.Owner, table.TableName);
        //    using (var con = GetConnection(server, string.Empty)) {
        //        con.Open();
        //        var cmd=new OracleCommand(String.Format("Select * from {0}.{1}", table.Owner, table.TableName), con);
        //        using (var rdr = cmd.ExecuteReader(CommandBehavior.KeyInfo)){
        //            var dt=rdr.GetSchemaTable();
                    
        //            foreach (DataRow dr in dt.Rows) {
        //                var colInfo=new ColumnInfo(){
        //                                                ColumnName=dr["ColumnName"].ToString(),
        //                                                Ordinal=dr.GetInt32("ColumnOrdinal"),
        //                                                Size=dr.GetInt32("ColumnSize"),
        //                                                Precision=dr.GetInt32OrNull("NumericPrecision"),
        //                                                Scale=dr.GetInt32OrNull("NumericScale"),
        //                                                AllowDBNull=Convert.ToBoolean(dr["AllowDBNull"]),
        //                                                IsKey=Convert.ToBoolean(dr["IsKey"]),
        //                                                IsUnique=Convert.ToBoolean(dr["IsUnique"]),
        //                                                DataType=Type.GetType(dr["DataType"].ToString()),
        //                                                IsRowId=Convert.ToBoolean(dr["IsRowID"])
        //                                            };
        //                colInfo.IsKey=colInfo.IsKey|keys.Any(k => k.ColumnName.Equals(colInfo.ColumnName, StringComparison.InvariantCultureIgnoreCase) && k.KeyType==KeyConstraintTypes.Primary);
        //                colInfo.IsForeignKey=keys.Any(k => k.ColumnName.Equals(colInfo.ColumnName, StringComparison.InvariantCultureIgnoreCase) && k.KeyType==KeyConstraintTypes.Foreign);
        //                cols.Add(colInfo);
        //                ////foreach (DataColumn prop in dt.Columns) {

        //                ////    //Display the field name and value.
        //                ////    Console.WriteLine(prop.ColumnName + " = " + dr[prop]);
        //                ////}
        //                ////break;

        //            }

        //            //using (var dtCols = con.GetSchema("Columns", new[] { table.Owner, table.TableName})) {
        //            //    using (var dv = new DataView(dtCols)) {
        //            //        //dv.Sort = dt.Columns[0].ColumnName;
        //            //        foreach (DataRowView dr in dv) {
        //            //            //Console.WriteLine(dr[0].ToString());
        //            //            //tables.Add(String.Format("{0}.{1}", dr[0].ToString(), dr[1].ToString()));
        //            //            foreach (DataColumn prop in dtCols.Columns){
        //            //                Console.WriteLine(prop.ColumnName + " = " + dr[prop.ColumnName]);
        //            //            }
                                
        //            //        }
        //            //    }
        //            //}
        //        }
                

        //    }

        //    return cols;
        //}

        

        //public static IEnumerable<String> GetDatabases(string server) {
        //    var dbs = new List<string>();
        //    using (var con = GetConnection(server, string.Empty)) {
        //        con.Open();
        //        using (var dt = con.GetSchema()) {
        //            using (var dv = new DataView(dt)) {
        //                dv.Sort = dt.Columns[0].ColumnName;
        //                foreach (DataRowView dr in dv) {
        //                    dbs.Add(dr[0].ToString());
        //                }
        //            }
        //        }

        //    }

        //    return dbs;
        //}
        #endregion

        
    }
}
