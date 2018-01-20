using System;
using System.Windows.Forms;
using DBClassGen.Classes;
using DBClassGen.Common.Classes;
using DBClassGen.Common.Interfaces;
using DBClassGen.Factories;
using DBClassGen.Generator.Classes;
using DBClassGen.Generator.Enumerations;

namespace DBClassGen {
    public partial class frmClassCreator : Form {

        private readonly ServerInfo _server;
        private readonly TableInfo _table;

        public frmClassCreator(ServerInfo server, TableInfo table) {
            InitializeComponent();

            _server=server;
            _table = table;
            if (_table.Columns == null)
                GetTableColumns();
        }

        private void GetTableColumns() {
            ISchemaRetriever schema=SchemaRetrieverFactory.GetSchemaRetriever(_server);
            _table.Columns=schema.GetColumns(_server, _table);
        }

        private void btnOk_Click(object sender, EventArgs e) {
            var codeRequest=CreateCodeRequest();
            
            // TODO: Call method to generate code...
            var gen=new CodeGenerator();
            var code=gen.Generate(codeRequest);
        }

        private DBCodeRequest CreateCodeRequest() {
            return new DBCodeRequest(){
                                          Language=(Languages) Enum.Parse(typeof(Languages), cboLang.SelectedItem.ToString(), true),
                                          ClassName=txtClassName.Text,
                                          FilePath=txtFileName.Text,
                                          BaseClassType=txtBaseClassName.Text,
                                          DBReturnType=(DBReturnTypes) Enum.Parse(typeof(DBReturnTypes), cboDataRetrievalType.SelectedItem.ToString(), true),
                                          Namespace=txtNamespace.Text,
                                          GenerateAdd=chkAdd.Checked,
                                          GenerateDelete=chkDelete.Checked,
                                          GenerateGet=chkGet.Checked,
                                          GenerateUpdate=chkUpdate.Checked,
                                          ServerInfo=_server,
                                          TableInfo=_table
                                      };
        }

        private void btnCodeFileName_Click(object sender, EventArgs e) {
            using(var fld = new FolderBrowserDialog()){
                fld.Description="Please select a location to creat the new class.";
                fld.ShowNewFolderButton=true;
                if(fld.ShowDialog()==DialogResult.OK){
                    txtFileName.Text=fld.SelectedPath;
                }
            }
        }

    }
}
