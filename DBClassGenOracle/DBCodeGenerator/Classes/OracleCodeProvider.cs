using System;
using System.Linq;
using System.Text;
using DBClassGen.Generator.Interfaces;
using Oracle.DataAccess.Client;

namespace DBClassGen.Generator.Classes {
    public class OracleCodeProvider : IDBCodeProvider {

        public string BuildSelectStatement(Common.Classes.TableInfo tableInfo) {
            var sb = new StringBuilder("SELECT");

            var columns = tableInfo.Columns.ToList();
            foreach (var col in columns) {
                sb.Append(col.ColumnName);
                if (columns.IndexOf(col) < columns.Count)
                    sb.Append(", ");
            }

            sb.AppendFormat(@" FROM {0}", tableInfo.TableName);
            return sb.ToString();
        }

        public string BuildUpdateStatement(Common.Classes.TableInfo tableInfo) {
            var sb = new StringBuilder();
            sb.AppendFormat("UPDATE {0} SET", tableInfo.TableName.Trim().ToUpper());

            var cols = tableInfo.Columns.ToList();
            foreach (var col in cols) {

                // do not allow the keys to be updated through this function
                if ((!col.IsIdentity) && (!col.IsKey)) {
                    sb.AppendFormat("{0} = @{0}", col.ColumnName);
                    if (cols.IndexOf(col) != cols.Count() - 1) {
                        sb.Append(", ");
                    }
                }

                sb.Append(" WHERE ");
                var keys = tableInfo.Keys.ToList();
                foreach (var s in keys) {
                    sb.AppendFormat("{0} = @Original{0}", s.ColumnName);
                    if (keys.IndexOf(s) != keys.Count - 1) {
                        sb.Append(" AND ");
                    }
                }
            }

            return sb.ToString();
        }

        public string BuildInsertStatement(Common.Classes.TableInfo tableInfo) {
            var sb = new StringBuilder(256);
            var sb2 = new StringBuilder(256);
            var bHasSeed = false;
            sb.AppendFormat("INSERT INTO {0}", tableInfo.TableName.Trim().ToUpper());
            sb2.Append(" VALUES(");

            var cols = tableInfo.Columns.ToList();
            foreach (var col in cols) {
                if (!col.IsIdentity) {
                    sb.Append(col.ColumnName);
                    sb2.AppendFormat("@{0}", col.ColumnName);
                    if (cols.IndexOf(col) != cols.Count() - 1) {
                        sb.Append(", ");
                        sb2.Append(", ");
                    }
                    else
                        bHasSeed = true;
                }
            }

            sb.Append(")");
            sb2.Append(")");

            //Only return scope identity if the key has an identity seed
            if (bHasSeed)
                sb2.AppendFormat("RETURNING {0} into @NewId", cols.First(c=>c.IsIdentity).ColumnName);


            return String.Format("{0}{1}", sb.ToString(), sb2);
        }

        public string BuildDeleteStatement(Common.Classes.TableInfo tableInfo) {
            var sb = new StringBuilder(256);
            var colKeys = tableInfo.Keys.ToList();
            sb.AppendFormat("DELETE FROM {0}", tableInfo.TableName.Trim().ToUpper());

            if (colKeys.Count > 0) {
                sb.Append(" WHERE ");
                foreach (var s in colKeys) {
                    sb.AppendFormat("{0} = @Original{0}", s);
                    if (colKeys.IndexOf(s) != colKeys.Count - 1)
                        sb.Append(" AND ");
                }
            }
            return sb.ToString();
        }

        public Type GetExceptionType() {
            return typeof(OracleException);
        }


        public string GetDBNamespace(){
            return "Oracle.DataAccess.Client";
        }
    }
}
