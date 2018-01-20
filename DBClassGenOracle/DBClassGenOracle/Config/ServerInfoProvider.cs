using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;
using DBClassGen.Classes;
using DBClassGen.Common.Classes;

namespace DBClassGen.Config {
    public class ServerInfoProvider {

        private static bool _initialized = false;
        public static List<ServerInfo> _serverInfos;
        private static object _locker = new object();

        public static List<ServerInfo> ServerInfos {
            get { return _serverInfos; }            
        }

        static ServerInfoProvider(){
            if (!_initialized)
                InitializeList();
        }

        private static void InitializeList() {
            lock (_locker){
                _serverInfos=new List<ServerInfo>();
                var config=ServerConfigurationSection.GetConfig();
                foreach(ServerConfigurationElement server in config.Servers){
                    var filters=server.Filters;
                    _serverInfos.Add(new ServerInfo() {
                        Name = server.Name,
                        ConnectionStringName = server.ConnectionStringName,
                        ServerType = server.Type,
                        SchemaFilters = filters!=null ? GetFiltersFromConfiguration(filters) : new List<String>()
                    });
                }
                _initialized=true;
            }
        }

        private static List<string> GetFiltersFromConfiguration(SchemaFilterElementCollection filters){
            var rt=new List<String>();

            foreach (SchemaFilterElement filter in filters) {
               rt.Add(filter.Name);
            }

            return rt;
        }
    }
}
