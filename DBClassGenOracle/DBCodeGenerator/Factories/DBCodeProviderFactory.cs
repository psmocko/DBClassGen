using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DBClassGen.Common.Classes;
using DBClassGen.Common.Enumerations;
using DBClassGen.Generator.Classes;
using DBClassGen.Generator.Interfaces;

namespace DBClassGen.Generator.Factories {
    public static class DBCodeProviderFactory {

        public static IDBCodeProvider GetDBCodeProvider(ServerInfo serverInfo){
            switch(serverInfo.ServerType){
                case ServerTypes.SqlServer:
                    return new SqlServerCodeProvider();
                case ServerTypes.Oracle:
                    return new OracleCodeProvider();
                default:
                    throw new ArgumentOutOfRangeException(String.Format("Invalid server info. Server Type {0} not supported.", serverInfo.ServerType.ToString()), "serverInfo");
            }
        }
    }
}
