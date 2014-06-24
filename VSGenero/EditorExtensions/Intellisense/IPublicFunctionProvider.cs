/* ****************************************************************************
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

namespace VSGenero.EditorExtensions.Intellisense
{
    public interface IPublicFunctionProvider
    {
        IEnumerable<MemberCompletion> GetPublicFunctionCompletions();

        string GetPublicFunctionQuickInfo(string functionName);

        FunctionSignatureInfo GetPublicFunctionSignatureInfo(string functionName);
    }

    public class FunctionSignatureInfo
    {
        public string Name { get; set; }
        public string Parent { get; set; }
        public string Description { get; set; }

        private List<FunctionSignatureParamInfo> _params;
        public IList<FunctionSignatureParamInfo> Parameters
        {
            get
            {
                if (_params == null)
                    _params = new List<FunctionSignatureParamInfo>();
                return _params;
            }
        }

        private List<FunctionSignatureReturnInfo> _returns;
        public IList<FunctionSignatureReturnInfo> Returns
        {
            get
            {
                if (_returns == null)
                    _returns = new List<FunctionSignatureReturnInfo>();
                return _returns;
            }
        }

        public string GetSignatureText(bool includeReturns, bool includeParams, bool includeDescription)
        {
            StringBuilder sb = new StringBuilder();

            if (includeReturns)
            {
                // if we have zero or one returns, put it in front of the 
                if (Returns.Count == 0)
                {
                    sb.Append("void ");
                }
                else if (Returns.Count == 1)
                {
                    sb.AppendFormat("{0} ", Returns[0].Type);
                }
            }

            if(!string.IsNullOrWhiteSpace(Parent))
            {
                sb.AppendFormat("{0}.", Parent);
            }
            sb.Append(Name);
            if(includeParams)
            {
                sb.Append("(");
                for(int i = 0; i < Parameters.Count; i++)
                {
                    sb.Append(Parameters[i].ToString());
                    if (i + 1 < Parameters.Count)
                    {
                        sb.Append(", ");
                    }
                }
                sb.Append(")");
            }

            if(includeReturns)
            {
                if(Returns.Count > 1)
                {
                    sb.Append("\nReturns:\n");
                    for(int i = 0; i < Returns.Count; i++)
                    {
                        sb.AppendFormat("\t{0}", Returns[i].Type);
                        if(i + 1 < Returns.Count)
                        {
                            sb.Append(",\n");
                        }
                    }
                }
            }

            if(includeDescription && !string.IsNullOrWhiteSpace(Description))
            {
                sb.AppendFormat("\n{0}", Description);
            }
            return sb.ToString();
        }
    }

    public abstract class FunctionSignatureElementInfo
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
    }

    public class FunctionSignatureParamInfo : FunctionSignatureElementInfo
    {
        public bool IsMulti { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            if (IsMulti)
            {
                sb.AppendFormat("{0}...", Type);
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(Type))
                {
                    sb.Append(Type);
                }
                if (sb.Length > 0 && !string.IsNullOrWhiteSpace(Name))
                    sb.Append(" ");
                if(!string.IsNullOrWhiteSpace(Name))
                    sb.Append(Name);
            }

            return sb.ToString();
        }
    }

    public class FunctionSignatureReturnInfo : FunctionSignatureElementInfo
    {
    }
}
