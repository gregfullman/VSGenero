using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.Schema
{
    public static class Extensions
    {
        public static byte GetNibble<T>(this T t, int nibblePos) where T : struct, IConvertible
        {
            nibblePos *= 4;
            var value = t.ToInt64(CultureInfo.CurrentCulture);
            return (byte)((value >> nibblePos) & 0xF);
        }
    }
}
