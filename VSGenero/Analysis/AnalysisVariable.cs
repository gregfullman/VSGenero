/* ****************************************************************************
*
* Copyright (c) Microsoft Corporation. 
*
* This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
* copy of the license can be found in the License.html file at the root of this distribution. If 
* you cannot locate the Apache License, Version 2.0, please send an email to 
* vspython@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
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
    public class AnalysisVariable : IAnalysisVariable
    {
        public AnalysisVariable(LocationInfo locInfo, VariableType type)
        {
            _location = locInfo;
            _type = type;
        }

        /// <summary>
        /// Constructor to be used in cases where duplicates might exist
        /// </summary>
        /// <param name="locInfo"></param>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <param name="priority"></param>
        public AnalysisVariable(LocationInfo locInfo, VariableType type, string name, int priority)
            : this(locInfo, type)
        {
            _name = name;
            _priority = priority;
        }

        private LocationInfo _location;
        public LocationInfo Location
        {
            get { return _location; }
        }

        private VariableType _type;
        public VariableType Type
        {
            get { return _type; }
        }

        private string _name;
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        private int _priority;
        public int Priority
        {
            get { return _priority; }
            set { _priority = value; }
        }
    }
}
