/* ****************************************************************************
 * 
 * Copyright (c) 2014 Greg Fullman 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution.
 * By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Text;
using VSGenero.EditorExtensions;

namespace VSGenero.EditorExtensions
{
    public static class GeneroSingletons
    {
        private static Genero4GL_XMLSettingsLoader _languageSettings;
        public static Genero4GL_XMLSettingsLoader LanguageSettings
        {
            get
            {
                if (_languageSettings == null)
                {
                    _languageSettings = new Genero4GL_XMLSettingsLoader();
                }
                return _languageSettings;
            }
        }

        private static Dictionary<string, VariableDefinition> _systemVariables;
        public static Dictionary<string, VariableDefinition> SystemVariables
        {
            get
            {
                if (_systemVariables == null)
                {
                    _systemVariables = new Dictionary<string, VariableDefinition>();
                    FillSystemVariables();
                }
                return _systemVariables;
            }
        }

        private static void FillSystemVariables()
        {
            // 1) sqlca
            /*
             * From the 4js site:
             * DEFINE SQLCA RECORD
             *  SQLCODE INTEGER,
             *  SQLERRM CHAR(71),
             *  SQLERRP CHAR(8),
             *  SQLERRD ARRAY[6] OF INTEGER,
             *  SQLAWARN CHAR(7)
             * END RECORD
             */
            var sqlcaVarDef = new VariableDefinition { Name = "sqlca", IsRecordType = true };
            List<VariableDefinition> sqlcaElements = new List<VariableDefinition>();
            sqlcaElements.Add(new VariableDefinition { Name = "sqlcode", Type = "integer" });
            sqlcaElements.Add(new VariableDefinition { Name = "sqlerrm", Type = "char(71)" });
            sqlcaElements.Add(new VariableDefinition { Name = "sqlerrp", Type = "char(8)" });
            sqlcaElements.Add(new VariableDefinition { Name = "sqlerrd", ArrayType = VSGenero.EditorExtensions.ArrayType.Static, StaticArraySize = 6, Type = "integer" });
            sqlcaElements.Add(new VariableDefinition { Name = "sqlawarn", Type = "char(7)" });

            foreach (var element in sqlcaElements)
                sqlcaVarDef.RecordElements.AddOrUpdate(element.Name, element, ((x,y) => element));
            _systemVariables.Add(sqlcaVarDef.Name, sqlcaVarDef);

            // 2) Dialog
            var dialogVarDef = new VariableDefinition { Name = "dialog", Type = "ui.Dialog" };
            _systemVariables.Add(dialogVarDef.Name, dialogVarDef);
        }
    }
}
