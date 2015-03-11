using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis
{
    [Flags]
    public enum GetMemberOptions
    {
        None,
        /// <summary>
        /// When an expression resolves to multiple types the intersection of members is returned.  When this flag
        /// is not present the union of all members is returned.
        /// </summary>
        IntersectMultipleResults = 0x01,

        /// <summary>
        /// True if advanced members (currently defined as name mangled private members) should be hidden.
        /// </summary>
        HideAdvancedMembers = 0x02,

        /// <summary>
        /// True if the members should include keywords which only show up in a statement context that can 
        /// be completed in the current context in addition to the member list.  
        /// 
        /// Keywords are only included when the request for completions is for all top-level members 
        /// during a call to ModuleAnalysis.GetMembersByIndex or when specifically requesting top 
        /// level members via ModuleAnalysis.GetAllAvailableMembersByIndex.
        /// </summary>
        IncludeStatementKeywords = 0x04,

        /// <summary>
        /// True if the members should include keywords which can show up in an expression context that
        /// can be completed in the current context in addition to the member list.  
        /// 
        /// Keywords are only included when the request for completions is for all top-level members 
        /// during a call to ModuleAnalysis.GetMembersByIndex or when specifically requesting top 
        /// level members via ModuleAnalysis.GetAllAvailableMembersByIndex.
        /// </summary>
        IncludeExpressionKeywords = 0x08,
    }

    internal static class GetMemberOptionsExtensions
    {
        public static bool Intersect(this GetMemberOptions self)
        {
            return (self & GetMemberOptions.IntersectMultipleResults) != 0;
        }
        public static bool HideAdvanced(this GetMemberOptions self)
        {
            return (self & GetMemberOptions.HideAdvancedMembers) != 0;
        }
        public static bool Keywords(this GetMemberOptions self)
        {
            return (self & GetMemberOptions.IncludeStatementKeywords | GetMemberOptions.IncludeExpressionKeywords) != 0;
        }
        public static bool StatementKeywords(this GetMemberOptions self)
        {
            return (self & GetMemberOptions.IncludeStatementKeywords) != 0;
        }
        public static bool ExpressionKeywords(this GetMemberOptions self)
        {
            return (self & GetMemberOptions.IncludeExpressionKeywords) != 0;
        }
    }
}
