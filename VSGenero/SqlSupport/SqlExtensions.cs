/* ****************************************************************************
 * Copyright (c) 2015 Greg Fullman 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution.
 * By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/ 

using Microsoft.SqlServer.Management.Smo.RegSvrEnum;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.SqlSupport
{
    public static class SqlExtensions
    {
        private static string _sqlExtractionFile;

        public static void SetSqlExtractionFile(string filename)
        {
            _sqlExtractionFile = filename;
        }

        public static string GetSqlExtractionFile()
        {
            return _sqlExtractionFile;
        }

        public static IEnumerable<FieldInfo> GetAllFields(this Type t)
        {
            if (t == null)
                return Enumerable.Empty<FieldInfo>();

            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic |
                                 BindingFlags.Static | BindingFlags.Instance |
                                 BindingFlags.DeclaredOnly;
            return t.GetFields(flags).Concat(GetAllFields(t.BaseType));
        }

        public static void SetSqlEditorConnection(IVsTextLines ppBuffer, ISqlContextDeterminator contextDeterminator, OleMenuCommandService commandService, CommandID sqlEditorConnectCmdId)
        {
            if(ppBuffer != null && contextDeterminator != null && _sqlExtractionFile != null)
            {
#if DEV15_OR_LATER
                Assembly dll = Assembly.Load(@"Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=15.0.0.0");
#elif DEV14_OR_LATER
                Assembly dll = Assembly.Load(@"Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=14.0.0.0");
#else
                Assembly dll = Assembly.Load(@"Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=12.0.0.0");
#endif
                if (dll != null)
                {
                    Type sqlEditorPackageType = dll.GetType("Microsoft.VisualStudio.Data.Tools.SqlEditor.VSIntegration.SqlEditorPackage");
                    if (sqlEditorPackageType != null)
                    {
                        // get the GetAuxiliaryDocData method
                        var instanceProp = sqlEditorPackageType.GetProperty("Instance", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
                        if (instanceProp != null)
                        {
                            var instanceVal = instanceProp.GetValue(null, null);
                            if (instanceVal != null)
                            {
                                var getAuxMethod = sqlEditorPackageType.GetMethod("GetAuxillaryDocData", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
                                if (getAuxMethod != null)
                                {
                                    var auxiliaryDocDataVal = getAuxMethod.Invoke(instanceVal, new object[] { ppBuffer });
                                    if (auxiliaryDocDataVal != null)
                                    {
                                        // set the sql connection (IDbConnection) on the QueryExecutor.ConnectionStrategy
                                        var queryExecutorType = auxiliaryDocDataVal.GetType().GetProperty("QueryExecutor", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
                                        if (queryExecutorType != null)
                                        {
                                            var queryExecutorVal = queryExecutorType.GetValue(auxiliaryDocDataVal, null);
                                            if (queryExecutorVal != null)
                                            {
                                                var connectionStrategyType = queryExecutorVal.GetType().GetProperty("ConnectionStrategy", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
                                                if (connectionStrategyType != null)
                                                {
                                                    var connectionStrategyVal = connectionStrategyType.GetValue(queryExecutorVal, null);
                                                    if (connectionStrategyVal != null)
                                                    {
                                                        // set the _connection field

                                                        var connectionField = connectionStrategyVal.GetType().GetAllFields().Where(x => x.Name == "_connection").FirstOrDefault();
                                                        var uiConnectionField = connectionStrategyVal.GetType().GetAllFields().Where(x => x.Name == "_connectionInfo").FirstOrDefault();
                                                        if (connectionField != null && uiConnectionField != null)
                                                        {
                                                            string server, database;
                                                            if (contextDeterminator.DetermineSqlContext(_sqlExtractionFile, out server, out database))
                                                            {
                                                                SqlConnection sqlCon = new SqlConnection(string.Format(@"Data Source={0};Integrated Security=true;", server));
                                                                sqlCon.Open();
                                                                sqlCon.ChangeDatabase(database);
                                                                connectionField.SetValue(connectionStrategyVal, sqlCon);

                                                                try
                                                                {
                                                                    UIConnectionInfo connInfo = new UIConnectionInfo() { ServerName = server, UserName = string.Format("{0}\\{1}", Environment.UserDomainName, Environment.UserName) };
                                                                    uiConnectionField.SetValue(connectionStrategyVal, connInfo);
                                                                }
                                                                catch(ArgumentException ae)
                                                                {
                                                                    // In VS2015 this exception gets thrown, but everything works correctly.
                                                                    // So for the time being, we'll just keep going
                                                                }

                                                                if (commandService != null)
                                                                {
                                                                    commandService.GlobalInvoke(sqlEditorConnectCmdId);
                                                                }

                                                                // attempt to set the QueryExecutor's IsConnected value
                                                                var isConnProp = queryExecutorVal.GetType().GetProperty("IsConnected", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
                                                                if(isConnProp != null)
                                                                {
                                                                    isConnProp.SetValue(queryExecutorVal, true, null);
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
