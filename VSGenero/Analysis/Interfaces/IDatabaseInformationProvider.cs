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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis
{
    public interface IDatabaseInformationProvider
    {
        void SetFilename(string filename);

        IEnumerable<IDbTableResult> GetTables();
        IAnalysisResult GetTable(string tablename);
        IEnumerable<IAnalysisResult> GetColumns(string tableName);
        IAnalysisResult GetColumn(string tableName, string columnName);
        string GetColumnType(string tableName, string columnName);
    }
}
