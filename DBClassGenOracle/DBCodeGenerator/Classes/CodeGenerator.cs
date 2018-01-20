using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using DBClassGen.Common.Classes;
using DBClassGen.Generator.Enumerations;
using DBClassGen.Generator.Factories;
using DBClassGen.Generator.Interfaces;

namespace DBClassGen.Generator.Classes {
    public class CodeGenerator {

        public DBCodeResponse Generate(DBCodeRequest request){
            // TODO:Validate request

            var resp=new DBCodeResponse();
            //var codeNamespace=new CodeNamespace();
            var ccu=new CodeCompileUnit();
            var provider=CodeDomProvider.CreateProvider(request.Language.ToString());
            var classNamespace=new CodeNamespace(request.Namespace);
            
            var options=CreateCodeGeneratorOptions();
            
            resp.FileName = Path.Combine(Path.Combine(request.FilePath, request.ClassName), provider.FileExtension);
            
            using (var ms = new MemoryStream())
            using(var sw=new StreamWriter(ms)){

                var ccg=provider.CreateGenerator(sw);
                classNamespace.Imports.Add(new CodeNamespaceImport("System"));
                classNamespace.Imports.Add(new CodeNamespaceImport("System.Data"));               
                classNamespace.Imports.Add(new CodeNamespaceImport(DBCodeProviderFactory.GetDBCodeProvider(request.ServerInfo).GetDBNamespace()));

                var cls=CreateClassDeclaration(request.ClassName, request.BaseClassType);
                //cls.Members.Add(BuildGetConnectionFunction(request.ServerInfo));
                if(request.GenerateGet){
                    cls.Members.Add(BuildSelectStatement(request.ServerInfo, request.TableInfo));
                    cls.Members.AddRange(BuildGetFunctions(request.ServerInfo, request.TableInfo, request.DBReturnType));
                }

                using (var sr = new StreamReader(ms)){
                    resp.Code=sr.ReadToEnd(); 
                }
                
                sw.Flush();
            }
            
            return resp;
        }
        
        private CodeMemberMethod[] BuildGetFunctions(ServerInfo serverInfo, TableInfo tableInfo, DBReturnTypes returnType) {
            CodeMemberMethod[] rt;
            var hasKeys=tableInfo.Columns.ToList().Any(c=>c.IsKey);
            if (hasKeys)
                rt = new CodeMemberMethod[]{ BuildGetFunction(serverInfo, tableInfo.Columns.ToList(), true, true, returnType), BuildGetFunction(serverInfo, tableInfo.Columns.ToList(), false, true, returnType) };

            else
                rt = new CodeMemberMethod[] { BuildGetFunction(serverInfo, tableInfo.Columns.ToList(), true, false, returnType) };
            return rt;
        }

        private CodeMemberMethod BuildGetFunction(ServerInfo serverInfo, List<ColumnInfo> cols, bool isDefault, bool isOverloaded, DBReturnTypes returnType) {
            var rt=new CodeMemberMethod();
            IDBCodeProvider dbCode=DBCodeProviderFactory.GetDBCodeProvider(serverInfo);
            rt.Name="Get";
            rt.Attributes=MemberAttributes.Public;
            if (isOverloaded)
                 rt.Attributes=rt.Attributes | MemberAttributes.Overloaded;

            // If this is not the default call meaning all data then pass keys in
            if(isOverloaded & !isDefault){
                foreach(var col in cols.Where(c=>c.IsKey)){
                    rt.Parameters.Add(new CodeParameterDeclarationExpression(col.DataType, col.ColumnName));                    
                }
            }

            rt.ReturnType=new CodeTypeReference(returnType.ToString());

            //Blank Line
            rt.Statements.Add(new CodeSnippetStatement());

            //Diminsion the return variable
            rt.Statements.Add(new CodeVariableDeclarationStatement(returnType.ToString(), "rt", new CodeObjectCreateExpression(returnType.ToString(), new CodeExpression[]{})));

            //Create Connection and command object 
            rt.Statements.Add(new CodeVariableDeclarationStatement(typeof(IDbConnection), 
                "con", new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "GetConnection", new CodeExpression[]{})));


            rt.Statements.Add(new CodeVariableDeclarationStatement(typeof(IDbCommand), "cmd",
                                                                   new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeVariableReferenceExpression("con"), "CreateCommand"))));
            string whereClause = null;
            if (cols.Any(c => c.IsKey)){
                whereClause=BuildWhereClause(cols.Where(c => c.IsKey).ToList());
            }

            //Blank Line
            rt.Statements.Add(new CodeSnippetStatement());
            rt.Statements.Add(new CodeVariableDeclarationStatement(typeof(IDbDataAdapter), "da",
                                                                   new CodeObjectCreateExpression(typeof(IDbDataAdapter), 
                                                                       new CodeExpression[]{new CodeVariableReferenceExpression("cmd")})));
            //Blank Line
            rt.Statements.Add(new CodeSnippetStatement());
            var tryBlock=new CodeTryCatchFinallyStatement();
            tryBlock.TryStatements.Add(new CodeAssignStatement(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("da"), "SelectCommand"),
                                                               new CodeVariableReferenceExpression("cmd")));

            if (cols.Any(c => c.IsKey)) {
                var exp = new CodeExpression();
                //Set the command text
                tryBlock.TryStatements.Add(new CodeAssignStatement(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("cmd"), "CommandText"),
                                                                   new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression("_selectSQL"), CodeBinaryOperatorType.Add, new CodeSnippetExpression(whereClause))));

                //add parameters for keys
                foreach (var key in cols.Where(c => c.IsKey)) {
                    tryBlock.TryStatements.Add(new CodeAssignStatement(new CodePropertyReferenceExpression(new CodeMethodInvokeExpression(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("cmd"), "Parameters"), 
                        "Add", new CodeExpression[] { new CodeSnippetExpression(String.Format(@"""@{0}""", key.ColumnName)), 
                            new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(new CodeTypeReference(key.DbDataType.GetType())), key.DbDataType), 
                            new CodePrimitiveExpression(key.Size) }), "Value"), new CodeVariableReferenceExpression()));

                }
                rt.Statements.Add(new CodeSnippetStatement());
            }
            else{
                // Set the command text
                tryBlock.TryStatements.Add(new CodeAssignStatement(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("cmd"), "CommandText"),
                                                                   new CodeVariableReferenceExpression("_selectSQL")));
            }

            tryBlock.TryStatements.Add(new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeVariableReferenceExpression("con"), "Open"), new CodeExpression[]{}));
            tryBlock.TryStatements.Add(new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeVariableReferenceExpression("da"), "Fill"), 
                new CodeExpression[]{new CodeVariableReferenceExpression("rt")}));
            tryBlock.TryStatements.Add(new CodeSnippetStatement());

            var catchBlock=new CodeCatchClause("exSQL", new CodeTypeReference(dbCode.GetExceptionType()));
            catchBlock.Statements.Add(new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), "ThrowException"),
                                                                     new CodeExpression[]{new CodePrimitiveExpression("An SQL error has occurred retrieving data."), 
                                                                         new CodeVariableReferenceExpression("exSQL")}));
            catchBlock.Statements.Add(new CodeSnippetStatement());
            tryBlock.CatchClauses.Add(catchBlock);

            catchBlock=new CodeCatchClause("ex", new CodeTypeReference(typeof(Exception)));
            catchBlock.Statements.Add(new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), "ThrowException"),
                                                                     new CodeExpression[]{new CodePrimitiveExpression("An error has occurred retrieving data."), 
                                                                         new CodeVariableReferenceExpression("ex")}));
            catchBlock.Statements.Add(new CodeSnippetStatement());
            tryBlock.CatchClauses.Add(catchBlock);

            var cond=new CodeConditionStatement();
            cond.Condition=new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression("da"), CodeBinaryOperatorType.IdentityInequality, new CodePrimitiveExpression(null));
            cond.TrueStatements.Add(GetDataAdapterDisposeStatement());
            tryBlock.FinallyStatements.Add(cond);

            tryBlock.FinallyStatements.Add(GetConnectionCloseStatement());
            tryBlock.FinallyStatements.Add(GetConnectionDisposeStatement());
            tryBlock.FinallyStatements.Add(GetCommandDisposeStatement());
            tryBlock.FinallyStatements.Add(new CodeSnippetStatement());

            rt.Statements.Add(tryBlock);
            rt.Statements.Add(new CodeSnippetStatement());
           
            rt.Statements.Add(new CodeMethodReturnStatement(new CodeVariableReferenceExpression("rt")));
            rt.Statements.Add(new CodeSnippetStatement());


            return rt;
            
        }

        private string BuildWhereClause(List<ColumnInfo> keys) {
            var sb=new StringBuilder();
            sb.Append(@"""" + " WHERE ");
    
            foreach (var col in keys){
                sb.AppendFormat("{0} = @{1}", col.ColumnName, col.ColumnName);
                if (keys.IndexOf(col) < (keys.Count - 1))
                    sb.Append(" AND ");
            }
            sb.Append(@"""");
            return sb.ToString();
        }

        private static CodeMethodInvokeExpression GetCommandDisposeStatement() {
            return GetDisposeStatement("cmd");
        }

        private static CodeMethodInvokeExpression GetConnectionDisposeStatement() {
            return GetDisposeStatement("con");
        }

        private static CodeMethodInvokeExpression GetDataAdapterDisposeStatement() {
            return GetDisposeStatement("da");
        }

        private static CodeMethodInvokeExpression GetDisposeStatement(String variableName){
            return new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeVariableReferenceExpression(variableName), "Dispose"), new CodeExpression[] { });
        }

        private CodeConditionStatement GetConnectionCloseStatement() {
            var cond=new CodeConditionStatement();
            cond.Condition=new CodeBinaryOperatorExpression(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("con"), "State"), CodeBinaryOperatorType.IdentityInequality,
                                                            new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(typeof(ConnectionState)), "Closed"));
            cond.TrueStatements.Add(new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeVariableReferenceExpression("con"), "Close"), new CodeExpression[]{}));

            return cond;
        }

        
        private CodeTypeDeclaration CreateClassDeclaration(String className, String baseClassTypeName){
            var cls = new CodeTypeDeclaration(className);  // Create the class

            if (!String.IsNullOrWhiteSpace(baseClassTypeName)) {
                cls.BaseTypes.Add(new CodeTypeReference(baseClassTypeName));
            }

            cls.TypeAttributes=TypeAttributes.Public;
            cls.IsClass = true;
            
            return cls;
        }

        private CodeMemberField BuildSelectStatement(ServerInfo serverInfo, TableInfo tableInfo){
            var cm=new CodeMemberField(typeof(System.String), "_selectSQL");
            cm.Attributes= (cm.Attributes & ~MemberAttributes.AccessMask & ~MemberAttributes.ScopeMask) | MemberAttributes.Private | MemberAttributes.Const;
            IDBCodeProvider dbProvider=DBCodeProviderFactory.GetDBCodeProvider(serverInfo);
            cm.InitExpression = new CodePrimitiveExpression(dbProvider.BuildSelectStatement(tableInfo));
            return cm;
        }

        

        private CodeGeneratorOptions CreateCodeGeneratorOptions() {
            return new CodeGeneratorOptions(){
                                                      BracingStyle="C",
                                                      IndentString="\t"
                                                  };
        }
    }
}
