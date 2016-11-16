using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis
{
    public class MemberTypeAttribute : Attribute
    {
        private readonly MemberType _memberType;

        public MemberTypeAttribute(MemberType memberType)
        {
            _memberType = memberType;
        }

        public MemberType MemberType
        {
            get
            {
                return _memberType;
            }
        }
    }
}
