using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis
{
    /// <summary>
    /// Well known built-in types that the analysis engine needs for doing interpretation.
    /// </summary>
    public enum BuiltinTypeId : int
    {
        Unknown,
        Object,
        Type,
        NoneType,

        Bool,
        Int,
        /// <summary>
        /// The long integer type.
        /// </summary>
        /// <remarks>
        /// Interpreters should map this value to Int if they only have one
        /// integer type.
        /// </remarks>
        Long,
        Float,
        Complex,

        Tuple,
        List,
        Dict,
        Set,
        FrozenSet,

        /// <summary>
        /// The default string type.
        /// </summary>
        /// <remarks>
        /// Interpreters should map this value to either Bytes or Unicode
        /// depending on the type of "abc"
        /// </remarks>
        Str,
        /// <summary>
        /// The non-Unicode string type.
        /// </summary>
        Bytes,
        /// <summary>
        /// The Unicode string type.
        /// </summary>
        Unicode,

        /// <summary>
        /// The iterator for the default string type.
        /// </summary>
        /// <remarks>
        /// Interpreters should map this value to either BytesIterator or
        /// UnicodeIterator depending on the type of iter("abc").
        /// </remarks>
        StrIterator,
        /// <summary>
        /// The iterator for the non-Unicode string type.
        /// </summary>
        BytesIterator,
        /// <summary>
        /// The iterator for the Unicode string type.
        /// </summary>
        UnicodeIterator,

        Module,
        Function,
        BuiltinMethodDescriptor,
        BuiltinFunction,
        Generator,

        Property,
        ClassMethod,
        StaticMethod,

        Ellipsis,

        TupleIterator,
        ListIterator,
        /// <summary>
        /// The type returned by dict.iterkeys (2.x) or dict.keys (3.x)
        /// Also the type returned by iter(dict())
        /// </summary>
        DictKeys,
        /// <summary>
        /// The type returned by dict.itervalues (2.x) or dict.values (3.x)
        /// </summary>
        DictValues,
        /// <summary>
        /// The type returned by dict.iteritems (2.x) or dict.items (3.x)
        /// </summary>
        DictItems,
        SetIterator,
        CallableIterator,
    }

    internal static class BuiltinTypeIdExtensions
    {
        /// <summary>
        /// Indicates whether an ID should be remapped by an interpreter.
        /// </summary>
        public static bool IsVirtualId(this BuiltinTypeId id)
        {
            return id == BuiltinTypeId.Str ||
                id == BuiltinTypeId.StrIterator ||
                (int)id > (int)LastTypeId;
        }

        public static BuiltinTypeId LastTypeId
        {
            get
            {
                return BuiltinTypeId.CallableIterator;
            }
        }
    }
}
