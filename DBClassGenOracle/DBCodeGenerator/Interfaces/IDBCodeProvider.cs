using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DBClassGen.Common.Classes;

namespace DBClassGen.Generator.Interfaces {
    public interface IDBCodeProvider {

        String BuildSelectStatement(TableInfo tableInfo);
        String BuildUpdateStatement(TableInfo tableInfo);
        String BuildInsertStatement(TableInfo tableInfo);
        String BuildDeleteStatement(TableInfo tableInfo);

        Type GetExceptionType();

        String GetDBNamespace();
    }
}
