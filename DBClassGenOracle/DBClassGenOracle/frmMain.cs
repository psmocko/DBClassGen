using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DBClassGen.Classes;
using DBClassGen.Common.Classes;
using DBClassGen.Common.Enumerations;
using DBClassGen.Common.Interfaces;
using DBClassGen.Config;
using DBClassGen.Factories;

namespace DBClassGen {
    public partial class frmMain : Form{

        public frmMain(){
            InitializeComponent();
        }

        private void InitLists(){
            lvTables.Columns.Add("Name", 300);
            lvTables.View=View.Details;

            lvColumns.Columns.Add("Name", 300);
            lvColumns.Columns.Add("Type", 100);
            lvColumns.Columns.Add("Lenght", 75);
            lvColumns.Columns.Add("Precision", 75);
            lvColumns.Columns.Add("Scale", 75);
            lvColumns.Columns.Add("Constraint", 200);
            //lvColumns.ListViewItemSorter=new ListViewSorter();
        }

        private TreeNode[] GetLoadingNodes(){
            return new TreeNode[]{new TreeNode(@"Loading...")};
        }

        private void Form1_Load(object sender, EventArgs e){
            InitLists();
            LoadServers();
            LoadActiveSqlServers();
        }

        private void LoadActiveSqlServers(){
            ISchemaRetriever server=SchemaRetrieverFactory.GetSchemaRetriever(ServerTypes.SqlServer);
            LoadServers(server.GetServers().ToList());
        }

        private void LoadServers(){
            LoadServers(ServerInfoProvider.ServerInfos);
        }

        private void LoadServers(List<ServerInfo> servers){
            foreach(var serverInfo in servers){
                var node=new TreeNode(serverInfo.Name, 0, 0, GetLoadingNodes());
                node.Tag=serverInfo;
                tvSchemas.Nodes.Add(node);
            }
        }

        private void tvSchemas_AfterExpand(object sender, TreeViewEventArgs e){
            e.Node.Nodes.Clear();
            var serverInfo=e.Node.Tag as ServerInfo;
            ISchemaRetriever server=SchemaRetrieverFactory.GetSchemaRetriever(serverInfo);
            foreach(var schema in server.GetSchemas(serverInfo)){
                e.Node.Nodes.Add(new TreeNode(schema, 1, 1));
            }

        }

        private void tvSchemas_AfterSelect(object sender, TreeViewEventArgs e){
            if(e.Node.Parent!=null){
                lvTables.Items.Clear();
                lvColumns.Items.Clear();
                var serverInfo=e.Node.Parent.Tag as ServerInfo;
                ISchemaRetriever server=SchemaRetrieverFactory.GetSchemaRetriever(serverInfo);
                foreach(var table in server.GetTables(serverInfo, e.Node.Text)){
                    var item=lvTables.Items.Add(table.TableName, String.Format("{0}.{1}", table.Owner, table.TableName), 2);
                    item.Tag=table;
                }
            }
        }

        private void lvTables_SelectedIndexChanged(object sender, EventArgs e){
            if(tvSchemas.SelectedNode!=null && tvSchemas.SelectedNode.Parent!=null && lvTables.SelectedItems.Count==1){
                lvColumns.Items.Clear();
                var serverInfo=tvSchemas.SelectedNode.Parent.Tag as ServerInfo;
                var schema = tvSchemas.SelectedNode.Text;
                ISchemaRetriever server=SchemaRetrieverFactory.GetSchemaRetriever(serverInfo);
                if(lvTables.SelectedItems.Count==1){
                    var tableInfo=lvTables.SelectedItems[0].Tag as TableInfo;
                    tableInfo.Columns = server.GetColumns(serverInfo, tableInfo, schema);
                    foreach (var columnInfo in tableInfo.Columns){
                        var item=lvColumns.Items.Add(columnInfo.ColumnName);
                        item.SubItems.Add(columnInfo.DataType.Name);
                        item.SubItems.Add(columnInfo.Size.ToString());
                        //if(columnInfo.Precision.HasValue)
                        item.SubItems.Add(columnInfo.Precision.ToString());
                        //if(columnInfo.Scale.HasValue)
                        item.SubItems.Add(columnInfo.Scale.ToString());
                        item.Tag=columnInfo;
                        if(columnInfo.IsKey){
                            if(tableInfo.Keys.Any(k => k.ColumnName.Equals(columnInfo.ColumnName, StringComparison.InvariantCultureIgnoreCase) && k.KeyType==KeyConstraintTypes.Primary)){
                                var constraint=tableInfo.Keys.First(k => k.ColumnName.Equals(columnInfo.ColumnName, StringComparison.InvariantCultureIgnoreCase) && k.KeyType==KeyConstraintTypes.Primary);
                                item.SubItems.Add(constraint.ConstraintName);
                            }
                            item.ImageIndex=3;
                        } else if(columnInfo.IsForeignKey){
                            item.ImageIndex=4;
                            if(tableInfo.Keys.Any(k => k.ColumnName.Equals(columnInfo.ColumnName, StringComparison.InvariantCultureIgnoreCase) && k.KeyType==KeyConstraintTypes.Foreign)){
                                var constraint=tableInfo.Keys.First(k => k.ColumnName.Equals(columnInfo.ColumnName, StringComparison.InvariantCultureIgnoreCase) && k.KeyType==KeyConstraintTypes.Foreign);
                                item.SubItems.Add(constraint.ConstraintName);
                            }
                        } else if(columnInfo.IsUnique){
                            item.ImageIndex=5;
                            if(tableInfo.Keys.Any(k => k.ColumnName.Equals(columnInfo.ColumnName, StringComparison.InvariantCultureIgnoreCase) && k.KeyType==KeyConstraintTypes.Unique)){
                                var constraint=tableInfo.Keys.First(k => k.ColumnName.Equals(columnInfo.ColumnName, StringComparison.InvariantCultureIgnoreCase) && k.KeyType==KeyConstraintTypes.Unique);
                                item.SubItems.Add(constraint.ConstraintName);
                            }
                        }
                    }
                }
            }
        }

        private void lvColumns_ColumnClick(object sender, ColumnClickEventArgs e){
            //    var listSorter=lvColumns.ListViewItemSorter as ListViewSorter;
            //    if (listSorter.LastSort == e.Column) {
            //        // Change sort order for current column
            //        if (lvColumns.Sorting == SortOrder.Ascending) {
            //            lvColumns.Sorting = SortOrder.Descending;
            //        }
            //        else {
            //            lvColumns.Sorting = SortOrder.Ascending;
            //        }
            //    }
            //    else {
            //        lvColumns.Sorting = SortOrder.Descending;
            //    }
            //    listSorter.ByColumn = e.Column;
            //    try { lvColumns.Sort(); }
            //    catch (Exception){}

            //}


        }

        private void lvTables_MouseDoubleClick(object sender, MouseEventArgs e){
            var serverInfo=tvSchemas.SelectedNode.Parent.Tag as ServerInfo;
            var tableInfo=lvTables.SelectedItems[0].Tag as TableInfo;
            using (var frmCC = new frmClassCreator(serverInfo, tableInfo)){
                frmCC.ShowDialog(this);
            }
        }
    }
}
