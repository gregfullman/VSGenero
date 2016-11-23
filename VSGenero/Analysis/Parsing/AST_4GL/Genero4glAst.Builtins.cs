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

using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST_4GL
{
    public partial class Genero4glAst
    {
        internal static VariableDef DialogVariable = new VariableDef("Dialog", new TypeReference("ui.Dialog"), -1);
        internal static VariableDef PagenoVariable = new VariableDef("pageno", new TypeReference("integer"), -1);

        private static bool _builtinsInitialized = false;
        private static object _builtinsInitLock = new object();

        private static void InitializeSystemVariables()
        {
            // System variables
            lock (_systemVariablesLock)
            {
                if (_systemVariables == null)
                {
                    _systemVariables = new Dictionary<string, IAnalysisResult>(StringComparer.OrdinalIgnoreCase);
                    _systemVariables.Add("status", new ProgramRegister("status", "int"));
                    _systemVariables.Add("int_flag", new ProgramRegister("int_flag", "boolean"));
                    _systemVariables.Add("quit_flag", new ProgramRegister("quit_flag", "boolean"));
                    _systemVariables.Add("sqlca", new ProgramRegister("sqlca", "record", new List<ProgramRegister>
                        {
                            new ProgramRegister("sqlcode", "int"),
                            new ProgramRegister("sqlerrm", "char(71)"),
                            new ProgramRegister("sqlerrp", "char(8)"),
                            new ProgramRegister("sqlerrd", "array[6] of int"),
                            new ProgramRegister("sqlawarn", "char(7)")
                        }));
                    _systemVariables.Add("SQLSTATE", new ProgramRegister("SQLSTATE", "int"));
                    _systemVariables.Add("SQLERRMESSAGE", new ProgramRegister("SQLERRMESSAGE", "string"));
                    _systemVariables.Add("TODAY", new ProgramRegister("TODAY", "date"));
                }
            }
        }
        private static object _systemVariablesLock = new object();
        private static Dictionary<string, IAnalysisResult> _systemVariables;
        public static IDictionary<string, IAnalysisResult> SystemVariables
        {
            get
            {
                if (_systemVariables == null)
                    InitializeSystemVariables();
                return _systemVariables;
            }
        }

        private static void InitializeSystemConstants()
        {
            // System constants
            lock (_systemConstantsLock)
            {
                if (_systemConstants == null)
                {
                    _systemConstants = new Dictionary<string, IAnalysisResult>(StringComparer.OrdinalIgnoreCase);
                    _systemConstants.Add("null", new SystemConstant("null", null, null));
                    _systemConstants.Add("true", new SystemConstant("true", "int", 1));
                    _systemConstants.Add("false", new SystemConstant("false", "int", 0));
                    _systemConstants.Add("notfound", new SystemConstant("notfound", "int", 100));
                }
            }
        }
        private static object _systemConstantsLock = new object();
        private static Dictionary<string, IAnalysisResult> _systemConstants;
        public static IDictionary<string, IAnalysisResult> SystemConstants
        {
            get
            {
                if (_systemConstants == null)
                    InitializeSystemConstants();
                return _systemConstants;
            }
        }

        private static void InitializeSystemMacros()
        {
            lock(_systemMacrosLock)
            {
                _systemMacros = new Dictionary<string, IAnalysisResult>(StringComparer.OrdinalIgnoreCase);
                _systemMacros.Add("__LINE__", new SystemMacro("__LINE__", null));
                _systemMacros.Add("__FILE__", new SystemMacro("__FILE__", null));
                _systemMacros.Add("current", new SystemMacro("current", null));
            }
        }
        private static object _systemMacrosLock = new object();
        private static Dictionary<string, IAnalysisResult> _systemMacros;
        public static IDictionary<string, IAnalysisResult> SystemMacros
        {
            get
            {
                if (_systemMacros == null)
                    InitializeSystemMacros();
                return _systemMacros;
            }
        }

        private static Dictionary<string, IFunctionResult> _systemFunctions;
        public static IDictionary<string, IFunctionResult> SystemFunctions
        {
            get
            {
                if (_systemFunctions == null)
                    InitializeBuiltins();
                return _systemFunctions;
            }
        }

        private static Dictionary<string, IFunctionResult> _arrayFunctions;
        public static IDictionary<string, IFunctionResult> ArrayFunctions
        {
            get
            {
                if (_arrayFunctions == null)
                    InitializeBuiltins();
                return _arrayFunctions;
            }
        }

        private static Dictionary<string, IFunctionResult> _stringFunctions;
        public static IDictionary<string, IFunctionResult> StringFunctions
        {
            get
            {
                if (_stringFunctions == null)
                    InitializeBuiltins();
                return _stringFunctions;
            }
        }

        private static Dictionary<string, IFunctionResult> _byteFunctions;
        public static IDictionary<string, IFunctionResult> ByteFunctions
        {
            get
            {
                if (_byteFunctions == null)
                    InitializeBuiltins();
                return _byteFunctions;
            }
        }

        private static Dictionary<string, IFunctionResult> _textFunctions;
        public static IDictionary<string, IFunctionResult> TextFunctions
        {
            get
            {
                if (_textFunctions == null)
                    InitializeBuiltins();
                return _textFunctions;
            }
        }

        private static void InitializeBuiltins()
        {
            lock (_builtinsInitLock)
            {
                if (!_builtinsInitialized)
                {
                    _arrayFunctions = new Dictionary<string, IFunctionResult>(StringComparer.OrdinalIgnoreCase);
                    _systemFunctions = new Dictionary<string, IFunctionResult>(StringComparer.OrdinalIgnoreCase);
                    _stringFunctions = new Dictionary<string, IFunctionResult>(StringComparer.OrdinalIgnoreCase);
                    _byteFunctions = new Dictionary<string, IFunctionResult>(StringComparer.OrdinalIgnoreCase);
                    _textFunctions = new Dictionary<string, IFunctionResult>(StringComparer.OrdinalIgnoreCase);

                    #region Generated Functions
                    _arrayFunctions.Add("appendElement", new BuiltinFunction("appendElement", "Array", new List<ParameterResult>
                    { }, new List<string> { },
"Adds a new element at the end of a dynamic array. This method has no effect on a static array.",
""));
                    _arrayFunctions.Add("clear", new BuiltinFunction("clear", "Array", new List<ParameterResult>
                    { }, new List<string> { },
                    "Removes all elements in a dynamic array. Sets all elements to NULL in a static array.",
                    ""));
                    _arrayFunctions.Add("deleteElement", new BuiltinFunction("deleteElement", "Array", new List<ParameterResult>
{new ParameterResult("pos", "", "integer"),
}, new List<string> { },
                    "Removes an element at the given position. In a static or dynamic array, the elements after the given position are moved up. In a dynamic array, the number of elements is decremented by 1.",
                    ""));
                    _arrayFunctions.Add("getLength", new BuiltinFunction("getLength", "Array", new List<ParameterResult>
                    { }, new List<string> { "integer", },
                    "Returns the length of a one-dimensional array.",
                    ""));
                    _arrayFunctions.Add("insertElement", new BuiltinFunction("insertElement", "Array", new List<ParameterResult>
{new ParameterResult("pos", "", "integer"),
}, new List<string> { },
                    "Inserts a new element at the given position. In a static or dynamic array, the elements after the given position are moved down. In a dynamic array, the number of elements increments by 1.",
                    ""));
                    _stringFunctions.Add("append", new BuiltinFunction("append", "String", new List<ParameterResult>
{new ParameterResult("part", "", "string"),
}, new List<string> { "string", },
                    "Returns a new string made by adding part to the end of the current string.",
                    ""));
                    _stringFunctions.Add("equals", new BuiltinFunction("equals", "String", new List<ParameterResult>
{new ParameterResult("source", "", "string"),
}, new List<string> { "integer", },
                    "Returns TRUE if the string passed as parameters matches the current string. If one of the strings is NULL the method returns FALSE.",
                    ""));
                    _stringFunctions.Add("equalsIgnoreCase", new BuiltinFunction("equalsIgnoreCase", "String", new List<ParameterResult>
{new ParameterResult("source", "", "string"),
}, new List<string> { "integer", },
                    "Returns TRUE if the string passed as parameters matches the current string, ignoring character case. If one of the strings is NULL the method returns FALSE.",
                    ""));
                    _stringFunctions.Add("getCharAt", new BuiltinFunction("getCharAt", "String", new List<ParameterResult>
{new ParameterResult("pos", "", "integer"),
}, new List<string> { "string", },
                    "Returns the character at the position pos (starts at 1). The unit for character positions depend on the length semantics. In BLS, the method returns NULL if pos does not match a valid character-byte position in the current string.",
                    ""));
                    _stringFunctions.Add("getIndexOf", new BuiltinFunction("getIndexOf", "String", new List<ParameterResult>
{new ParameterResult("part", "", "string"),
new ParameterResult("spos", "", "integer"),
}, new List<string> { "integer", },
                    "Returns the position of the substring part in the current string, starting from position spos. The unit for character positions depend on the length semantics used. Returns zero if the substring was not found. Returns -1 if string is NULL.",
                    ""));
                    _stringFunctions.Add("getLength", new BuiltinFunction("getLength", "String", new List<ParameterResult>
                    { }, new List<string> { "integer", },
                    "Returns the lenfth of the string, including trailing blanks (Note that the LENGTH() built-in function ignores trailing blanks). The unit for character length depend on the length semantics used.",
                    ""));
                    _stringFunctions.Add("subString", new BuiltinFunction("subString", "String", new List<ParameterResult>
{new ParameterResult("spos", "", "integer"),
new ParameterResult("epos", "", "integer"),
}, new List<string> { "string", },
                    "Returns the substring starting at position spos and ending at epos. The unit for character positions depend on the length semantics used. In BLS, returns NULL if the positions do not delimit a valid substring in the current string.",
                    ""));
                    _stringFunctions.Add("toLowerCase", new BuiltinFunction("toLowerCase", "String", new List<ParameterResult>
                    { }, new List<string> { "string", },
                    "Converts the current string to lowercase. Returns NULL if the string is null.",
                    ""));
                    _stringFunctions.Add("toUpperCase", new BuiltinFunction("toUpperCase", "String", new List<ParameterResult>
                    { }, new List<string> { "string", },
                    "Converts the current string to uppercase. Returns NULL if the string is null.",
                    ""));
                    _stringFunctions.Add("trim", new BuiltinFunction("trim", "String", new List<ParameterResult>
                    { }, new List<string> { "string", },
                    "Removes white space characters from the beginning and end of the current string. Returns NULL if the string is null.",
                    ""));
                    _stringFunctions.Add("trimLeft", new BuiltinFunction("trimLeft", "String", new List<ParameterResult>
                    { }, new List<string> { "string", },
                    "Removes white space characters from the beginning of the current string. Returns NULL if the string is null.",
                    ""));
                    _stringFunctions.Add("trimRight", new BuiltinFunction("trimRight", "String", new List<ParameterResult>
                    { }, new List<string> { "string", },
                    "Removes white space characters from the end of the current string. Returns NULL if the string is null.",
                    ""));
                    _byteFunctions.Add("readFile", new BuiltinFunction("readFile", "Byte", new List<ParameterResult>
{new ParameterResult("fileName", "", "string"),
}, new List<string> { },
                    "Reads data from a file and copies into memory or to the file used by the variables according to the LOCATE statement issued on the object.",
                    ""));
                    _byteFunctions.Add("writeFile", new BuiltinFunction("writeFile", "Byte", new List<ParameterResult>
{new ParameterResult("fileName", "", "string"),
}, new List<string> { },
                    "Writes data from the variable (memory or source file) to the destination file passed as parameter. The file is created if it does not exist.",
                    ""));
                    _textFunctions.Add("getLength", new BuiltinFunction("getLength", "Text", new List<ParameterResult>
                    { }, new List<string> { "integer", },
                    "Returns the size of the text in bytes.",
                    ""));
                    _textFunctions.Add("readFile", new BuiltinFunction("readFile", "Text", new List<ParameterResult>
{new ParameterResult("filename", "", "string"),
}, new List<string> { },
                    "Reads data from a file and copies into memory or to the file used by the variables according to the LOCATE statement issued on the object.",
                    ""));
                    _textFunctions.Add("writeFile", new BuiltinFunction("writeFile", "Text", new List<ParameterResult>
{new ParameterResult("filename", "", "string"),
}, new List<string> { },
                    "Writes data from the variable (memory or source file) to the destination file passed as parameter. The file is created if it does not exist.",
                    ""));
                    _systemFunctions.Add("nvl", new BuiltinFunction("nvl", null, new List<ParameterResult>
{new ParameterResult("main-expr", "", "expression"),
new ParameterResult("subst-expr", "", "expression"),
}, new List<string> { "expression", },
                    "Returns the second parameter if the first argument evaluates to NULL.",
                    ""));
                    _systemFunctions.Add("iif", new BuiltinFunction("iif", null, new List<ParameterResult>
{new ParameterResult("bool-expr", "", "expression"),
new ParameterResult("true-expr", "", "expression"),
new ParameterResult("false-expr", "", "expression"),
}, new List<string> { "expression", },
                    "Returns the second or third parameter according to the boolean expression given as first argument.",
                    ""));
                    _systemFunctions.Add("ascii", new BuiltinFunction("ascii", null, new List<ParameterResult>
{new ParameterResult("int-expr", "", "integer"),
}, new List<string> { "char(1)", },
                    "Produces an ASCII character.",
                    ""));
                    _systemFunctions.Add("ord", new BuiltinFunction("ord", null, new List<ParameterResult>
{new ParameterResult("source", "", "string"),
}, new List<string> { "integer", },
                    "Returns the code point of a character in the current locale.",
                    ""));
                    _systemFunctions.Add("lstr", new BuiltinFunction("lstr", null, new List<ParameterResult>
{new ParameterResult("source", "", "string"),
}, new List<string> { "integer", },
                    "Returns a localized string.",
                    ""));
                    _systemFunctions.Add("sfmt", new BuiltinFunction("sfmt", null, new List<ParameterResult>
{new ParameterResult("str-expr", "", "string"),
new ParameterResult("expr...", "", "exprList"),
}, new List<string> { "string", },
                    "Replaces place holders (%n) in a string with values.",
                    "http://4js.com/online_documentation/fjs-fgl-{0}manual-html/index.html?path=fjs-fgl-manual-html/index#c_fgl_operators_SFMT.html"));
                    _systemFunctions.Add("extend", new BuiltinFunction("extend", null, new List<ParameterResult>
{new ParameterResult("dt-expr", "", "expression"),
new ParameterResult("qual", "", "qualifier"),
}, new List<string> { "datetime", },
                    "Adjusts a date time value according to the qualifier.",
                    ""));
                    _systemFunctions.Add("date", new BuiltinFunction("date", null, new List<ParameterResult>
{new ParameterResult("expr", "", "expression"),
}, new List<string> { "date", },
                    "Converts an expression to a DATE value.",
                    ""));
                    _systemFunctions.Add("time", new BuiltinFunction("time", null, new List<ParameterResult>
{new ParameterResult("expr", "", "expression"),
}, new List<string> { "time", },
                    "Returns a time part of the date time expression.",
                    ""));
                    _systemFunctions.Add("year", new BuiltinFunction("year", null, new List<ParameterResult>
{new ParameterResult("expr", "", "expression"),
}, new List<string> { "integer", },
                    "Extracts the year of a date time expression.",
                    ""));
                    _systemFunctions.Add("month", new BuiltinFunction("month", null, new List<ParameterResult>
{new ParameterResult("expr", "", "expression"),
}, new List<string> { "integer", },
                    "Extracts the month of a date time expression.",
                    ""));
                    _systemFunctions.Add("day", new BuiltinFunction("day", null, new List<ParameterResult>
{new ParameterResult("expr", "", "expression"),
}, new List<string> { "integer", },
                    "Extracts the day of a date time expression.",
                    ""));
                    _systemFunctions.Add("weekday", new BuiltinFunction("weekday", null, new List<ParameterResult>
{new ParameterResult("expr", "", "expression"),
}, new List<string> { "integer", },
                    "Returns a positive whole number between 0 and 6 corresponding to the day of the week implied by its Parameter.",
                    ""));
                    _systemFunctions.Add("mdy", new BuiltinFunction("mdy", null, new List<ParameterResult>
{new ParameterResult("month", "", "integer"),
new ParameterResult("day", "", "integer"),
new ParameterResult("year", "", "integer"),
}, new List<string> { "date", },
                    "Creates a date from month, day and year units.",
                    ""));
                    _systemFunctions.Add("get_fldbuf", new BuiltinFunction("get_fldbuf", null, new List<ParameterResult>
{new ParameterResult("field", "", "formvar"),
new ParameterResult("addFields...", "", "formvarList"),
}, new List<string> { "context-dependent", },
                    "Returns as character strings the current values of the specified fields.",
                    ""));
                    _systemFunctions.Add("infield", new BuiltinFunction("infield", null, new List<ParameterResult>
{new ParameterResult("field", "", "formvar"),
}, new List<string> { "boolean", },
                    "Checks for the current screen field.",
                    ""));
                    _systemFunctions.Add("field_touched", new BuiltinFunction("field_touched", null, new List<ParameterResult>
{new ParameterResult("field", "", "formvar"),
new ParameterResult("field...", "", "formvarList"),
}, new List<string> { "boolean", },
                    "Checks if fields were modified during the dialog execution.",
                    ""));
                    _systemFunctions.Add("arg_val", new BuiltinFunction("arg_val", null, new List<ParameterResult>
{new ParameterResult("position", "", "integer"),
}, new List<string> { "string", },
                    "Returns a command line argument by position.",
                    ""));
                    _systemFunctions.Add("arr_count", new BuiltinFunction("arr_count", null, new List<ParameterResult>
                    { }, new List<string> { "integer", },
                    "Returns the number of rows entered during a INPUT ARRAY statement.",
                    ""));
                    _systemFunctions.Add("arr_curr", new BuiltinFunction("arr_curr", null, new List<ParameterResult>
                    { }, new List<string> { "integer", },
                    "Returns the current row in a DISPLAY ARRAY or INPUT ARRAY.",
                    ""));
                    _systemFunctions.Add("downshift", new BuiltinFunction("downshift", null, new List<ParameterResult>
{new ParameterResult("source", "", "string"),
}, new List<string> { "string", },
                    "Converts a string to lowercase.",
                    ""));
                    _systemFunctions.Add("err_get", new BuiltinFunction("err_get", null, new List<ParameterResult>
{new ParameterResult("errnum", "", "integer"),
}, new List<string> { "string", },
                    "Returns the text corresponding to an error number.",
                    ""));
                    _systemFunctions.Add("err_print", new BuiltinFunction("err_print", null, new List<ParameterResult>
{new ParameterResult("errnum", "", "integer"),
}, new List<string> { },
                    "Prints in the error line the text corresponding to an error number.",
                    ""));
                    _systemFunctions.Add("err_quit", new BuiltinFunction("err_quit", null, new List<ParameterResult>
{new ParameterResult("errnum", "", "integer"),
}, new List<string> { },
                    "Prints in the error line the text corresponding to an error number and terminates the program",
                    ""));
                    _systemFunctions.Add("errorlog", new BuiltinFunction("errorlog", null, new List<ParameterResult>
{new ParameterResult("text", "", "string"),
}, new List<string> { },
                    "Copies the string passed as parameter into the error log file.",
                    ""));
                    _systemFunctions.Add("fgl_buffertouched", new BuiltinFunction("fgl_buffertouched", null, new List<ParameterResult>
                    { }, new List<string> { "integer", },
                    "Returns TRUE if the input buffer was modified in the current field",
                    ""));
                    _systemFunctions.Add("fgl_db_driver_type", new BuiltinFunction("fgl_db_driver_type", null, new List<ParameterResult>
                    { }, new List<string> { "char(3)", },
                    "Returns the 3-letter identifier/code of the current database driver.",
                    ""));
                    _systemFunctions.Add("fgl_decimal_truncate", new BuiltinFunction("fgl_decimal_truncate", null, new List<ParameterResult>
{new ParameterResult("value", "", "decimal"),
new ParameterResult("decimals", "", "integer"),
}, new List<string> { "decimal", },
                    "Returns a decimal truncated to the precision passed as parameter.",
                    ""));
                    _systemFunctions.Add("fgl_decimal_sqrt", new BuiltinFunction("fgl_decimal_sqrt", null, new List<ParameterResult>
{new ParameterResult("value", "", "decimal"),
}, new List<string> { "decimal", },
                    "Computes the square root of the decimal passed as parameter.",
                    ""));
                    _systemFunctions.Add("fgl_decimal_exp", new BuiltinFunction("fgl_decimal_exp", null, new List<ParameterResult>
{new ParameterResult("value", "", "decimal"),
}, new List<string> { "decimal", },
                    "Returns the value of Euler's constant (e) raised to the power of the decimal passed as parameter",
                    ""));
                    _systemFunctions.Add("fgl_decimal_logn", new BuiltinFunction("fgl_decimal_logn", null, new List<ParameterResult>
{new ParameterResult("value", "", "decimal"),
}, new List<string> { "decimal", },
                    "Returns the natural logarithm of the decimal passed as parameter.",
                    ""));
                    _systemFunctions.Add("fgl_decimal_power", new BuiltinFunction("fgl_decimal_power", null, new List<ParameterResult>
{new ParameterResult("base", "", "decimal"),
new ParameterResult("exp", "", "decimal"),
}, new List<string> { "decimal", },
                    "Raises decimal to the power of the real exponent.",
                    ""));
                    _systemFunctions.Add("fgl_dialog_getbuffer", new BuiltinFunction("fgl_dialog_getbuffer", null, new List<ParameterResult>
                    { }, new List<string> { "string", },
                    "Returns the text of the input buffer of the current field.",
                    ""));
                    _systemFunctions.Add("fgl_dialog_getbufferlength", new BuiltinFunction("fgl_dialog_getbufferlength", null, new List<ParameterResult>
                    { }, new List<string> { "integer", },
                    "Returns the number of rows to feed a paged DISPLAY ARRAY.",
                    ""));
                    _systemFunctions.Add("fgl_dialog_getbufferstart", new BuiltinFunction("fgl_dialog_getbufferstart", null, new List<ParameterResult>
                    { }, new List<string> { "integer", },
                    "Returns the row offset of the page to feed a paged display array.",
                    ""));
                    _systemFunctions.Add("fgl_dialog_getcursor", new BuiltinFunction("fgl_dialog_getcursor", null, new List<ParameterResult>
                    { }, new List<string> { "integer", },
                    "Returns the position of the edit cursor in the current field.",
                    ""));
                    _systemFunctions.Add("fgl_dialog_getfieldname", new BuiltinFunction("fgl_dialog_getfieldname", null, new List<ParameterResult>
                    { }, new List<string> { "string", },
                    "Returns the name of the current input field.",
                    ""));
                    _systemFunctions.Add("fgl_dialog_getkeylabel", new BuiltinFunction("fgl_dialog_getkeylabel", null, new List<ParameterResult>
{new ParameterResult("keyname", "", "string"),
}, new List<string> { "string", },
                    "Returns the label associated to a key for the current interactive instruction",
                    ""));
                    _systemFunctions.Add("fgl_dialog_getselectionend", new BuiltinFunction("fgl_dialog_getselectionend", null, new List<ParameterResult>
                    { }, new List<string> { "integer", },
                    "Returns the position of the last selected character in the current field.",
                    ""));
                    _systemFunctions.Add("fgl_dialog_infield", new BuiltinFunction("fgl_dialog_infield", null, new List<ParameterResult>
{new ParameterResult("field-name", "", "string"),
}, new List<string> { "integer", },
                    "This function checks for the current input field.",
                    ""));
                    _systemFunctions.Add("fgl_dialog_setbuffer", new BuiltinFunction("fgl_dialog_setbuffer", null, new List<ParameterResult>
{new ParameterResult("value", "", "string"),
}, new List<string> { },
                    "Sets the input buffer of the current field.",
                    ""));
                    _systemFunctions.Add("fgl_dialog_setcurrline", new BuiltinFunction("fgl_dialog_setcurrline", null, new List<ParameterResult>
{new ParameterResult("line", "", "integer"),
new ParameterResult("row", "", "integer"),
}, new List<string> { },
                    "This function moves to a specific row in a record list.",
                    ""));
                    _systemFunctions.Add("fgl_dialog_setcursor", new BuiltinFunction("fgl_dialog_setcursor", null, new List<ParameterResult>
{new ParameterResult("position", "", "integer"),
}, new List<string> { },
                    "This function sets the position of the edit cursor in the current field.",
                    ""));
                    _systemFunctions.Add("fgl_dialog_setfieldorder", new BuiltinFunction("fgl_dialog_setfieldorder", null, new List<ParameterResult>
{new ParameterResult("active", "", "integer"),
}, new List<string> { },
                    "This function enables or disables field order constraint.",
                    ""));
                    _systemFunctions.Add("fgl_dialog_setkeylabel", new BuiltinFunction("fgl_dialog_setkeylabel", null, new List<ParameterResult>
{new ParameterResult("keyname", "", "string"),
new ParameterResult("label", "", "string"),
}, new List<string> { },
                    "Sets the label associated to a key for the current interactive instruction.",
                    ""));
                    _systemFunctions.Add("fgl_dialog_setselection", new BuiltinFunction("fgl_dialog_setselection", null, new List<ParameterResult>
{new ParameterResult("cursor", "", "integer"),
new ParameterResult("end", "", "integer"),
}, new List<string> { },
                    "Selects the text in the current field.",
                    ""));
                    _systemFunctions.Add("fgl_drawbox", new BuiltinFunction("fgl_drawbox", null, new List<ParameterResult>
{new ParameterResult("height", "", "integer"),
new ParameterResult("width", "", "integer"),
new ParameterResult("line", "", "integer"),
new ParameterResult("column", "", "integer"),
new ParameterResult("color", "", "integer"),
}, new List<string> { },
                    "Draws a rectangle in the current window.",
                    ""));
                    _systemFunctions.Add("fgl_drawline", new BuiltinFunction("fgl_drawline", null, new List<ParameterResult>
{new ParameterResult("column", "", "integer"),
new ParameterResult("line", "", "integer"),
new ParameterResult("width", "", "integer"),
new ParameterResult("type", "", "char(1)"),
new ParameterResult("color", "", "integer"),
}, new List<string> { },
                    "Draws a line in the current window (TUI and traditional mode).",
                    ""));
                    _systemFunctions.Add("fgl_getenv", new BuiltinFunction("fgl_getenv", null, new List<ParameterResult>
{new ParameterResult("variable", "", "string"),
}, new List<string> { "string", },
                    "Returns the value of the environment variable.",
                    ""));
                    _systemFunctions.Add("fgl_getfile", new BuiltinFunction("fgl_getfile", null, new List<ParameterResult>
{new ParameterResult("src", "", "string"),
new ParameterResult("dest", "", "string"),
}, new List<string> { },
                    "Transfers a file from the front end workstation to the application server machine.",
                    ""));
                    _systemFunctions.Add("fgl_gethelp", new BuiltinFunction("fgl_gethelp", null, new List<ParameterResult>
{new ParameterResult("help-id", "", "integer"),
}, new List<string> { "string", },
                    "Returns the help text according to its identifier by reading the current help file.",
                    ""));
                    _systemFunctions.Add("fgl_getkey", new BuiltinFunction("fgl_getkey", null, new List<ParameterResult>
                    { }, new List<string> { "integer", },
                    "Waits for a keystroke and returns the key number.",
                    ""));
                    _systemFunctions.Add("fgl_getkeylabel", new BuiltinFunction("fgl_getkeylabel", null, new List<ParameterResult>
{new ParameterResult("keyname", "", "string"),
}, new List<string> { "string", },
                    "Returns the default label associated to a key.",
                    ""));
                    _systemFunctions.Add("fgl_getpid", new BuiltinFunction("fgl_getpid", null, new List<ParameterResult>
                    { }, new List<string> { "integer", },
                    "Returns the system process identifier.",
                    ""));
                    _systemFunctions.Add("fgl_getresource", new BuiltinFunction("fgl_getresource", null, new List<ParameterResult>
{new ParameterResult("name", "", "string"),
}, new List<string> { "string", },
                    "Returns the value of an FGLPROFILE entry.",
                    ""));
                    _systemFunctions.Add("fgl_getversion", new BuiltinFunction("fgl_getversion", null, new List<ParameterResult>
                    { }, new List<string> { "string", },
                    "Returns the build number of the runtime system.",
                    ""));
                    _systemFunctions.Add("fgl_getwin_height", new BuiltinFunction("fgl_getwin_height", null, new List<ParameterResult>
                    { }, new List<string> { "integer", },
                    "Returns the number of rows of the current window.",
                    ""));
                    _systemFunctions.Add("fgl_getwin_width", new BuiltinFunction("fgl_getwin_width", null, new List<ParameterResult>
                    { }, new List<string> { "integer", },
                    "Returns the width of the current window as a number of columns.",
                    ""));
                    _systemFunctions.Add("fgl_getwin_x", new BuiltinFunction("fgl_getwin_x", null, new List<ParameterResult>
                    { }, new List<string> { "integer", },
                    "Returns the horizontal position of the current window.",
                    ""));
                    _systemFunctions.Add("fgl_getwin_y", new BuiltinFunction("fgl_getwin_y", null, new List<ParameterResult>
                    { }, new List<string> { "integer", },
                    "Returns the vertical position of the current window.",
                    ""));
                    _systemFunctions.Add("fgl_keyval", new BuiltinFunction("fgl_keyval", null, new List<ParameterResult>
{new ParameterResult("string", "", "string"),
}, new List<string> { "integer", },
                    "Returns the key code of a logical or physical key.",
                    ""));
                    _systemFunctions.Add("fgl_lastkey", new BuiltinFunction("fgl_lastkey", null, new List<ParameterResult>
                    { }, new List<string> { "integer", },
                    "Returns the key code corresponding to the logical key that the user most recently typed in the form.",
                    ""));
                    _systemFunctions.Add("fgl_putfile", new BuiltinFunction("fgl_putfile", null, new List<ParameterResult>
{new ParameterResult("src", "", "string"),
new ParameterResult("dest", "", "string"),
}, new List<string> { },
                    "Transfers a file from the application server machine to the front end workstation.",
                    ""));
                    _systemFunctions.Add("fgl_report_print_binary_file", new BuiltinFunction("fgl_report_print_binary_file", null, new List<ParameterResult>
{new ParameterResult("filename", "", "string"),
}, new List<string> { },
                    "Prints a file containing binary data during a report.",
                    ""));
                    _systemFunctions.Add("fgl_report_set_document_handler", new BuiltinFunction("fgl_report_set_document_handler", null, new List<ParameterResult>
{new ParameterResult("handler", "", "om.SaxDocumentHandler"),
}, new List<string> { },
                    "Redirects the next report to an XML document handler.",
                    ""));
                    _systemFunctions.Add("fgl_scr_size", new BuiltinFunction("fgl_scr_size", null, new List<ParameterResult>
{new ParameterResult("screen-array", "", "string"),
}, new List<string> { "integer", },
                    "Returns the size of the specified screen array in the current form.",
                    ""));
                    _systemFunctions.Add("fgl_set_arr_curr", new BuiltinFunction("fgl_set_arr_curr", null, new List<ParameterResult>
{new ParameterResult("row", "", "integer"),
}, new List<string> { },
                    "Moves to a specific row in a record list.",
                    ""));
                    _systemFunctions.Add("fgl_setenv", new BuiltinFunction("fgl_setenv", null, new List<ParameterResult>
{new ParameterResult("variable", "", "string"),
new ParameterResult("value", "", "string"),
}, new List<string> { },
                    "Sets the value of an environment variable.",
                    ""));
                    _systemFunctions.Add("fgl_setkeylabel", new BuiltinFunction("fgl_setkeylabel", null, new List<ParameterResult>
{new ParameterResult("keyname", "", "string"),
new ParameterResult("label", "", "string"),
}, new List<string> { },
                    "Sets the default label associated to a key.",
                    ""));
                    _systemFunctions.Add("fgl_setsize", new BuiltinFunction("fgl_setsize", null, new List<ParameterResult>
{new ParameterResult("height", "", "integer"),
new ParameterResult("width", "", "integer"),
}, new List<string> { },
                    "Sets the size of the main application window.",
                    ""));
                    _systemFunctions.Add("fgl_settitle", new BuiltinFunction("fgl_settitle", null, new List<ParameterResult>
{new ParameterResult("label", "", "string"),
}, new List<string> { },
                    "Sets the title of the current application window.",
                    ""));
                    _systemFunctions.Add("fgl_system", new BuiltinFunction("fgl_system", null, new List<ParameterResult>
{new ParameterResult("command", "", "string"),
}, new List<string> { },
                    "Runs a command on the application server.",
                    ""));
                    _systemFunctions.Add("fgl_width", new BuiltinFunction("fgl_width", null, new List<ParameterResult>
{new ParameterResult("expression", "", "string"),
}, new List<string> { "integer", },
                    "Returns the number of columns needed to represent the printed version of the expression.",
                    ""));
                    _systemFunctions.Add("fgl_window_getoption", new BuiltinFunction("fgl_window_getoption", null, new List<ParameterResult>
{new ParameterResult("attribute", "", "string"),
}, new List<string> { "string", },
                    "Returns attributes of the current window.",
                    ""));
                    _systemFunctions.Add("length", new BuiltinFunction("length", null, new List<ParameterResult>
{new ParameterResult("expression", "", "string"),
}, new List<string> { "integer", },
                    "Returns the number of the character string passed as parameter.",
                    ""));
                    _systemFunctions.Add("num_args", new BuiltinFunction("num_args", null, new List<ParameterResult>
                    { }, new List<string> { "integer", },
                    "Returns the number of program arguments.",
                    ""));
                    _systemFunctions.Add("scr_line", new BuiltinFunction("scr_line", null, new List<ParameterResult>
                    { }, new List<string> { "integer", },
                    "Returns the index of the current row in the screen array.",
                    ""));
                    _systemFunctions.Add("set_count", new BuiltinFunction("set_count", null, new List<ParameterResult>
{new ParameterResult("nbrows", "", "integer"),
}, new List<string> { },
                    "Defines the number of rows containing explicit data in a static array used by the next dialog.",
                    ""));
                    _systemFunctions.Add("show_help", new BuiltinFunction("show_help", null, new List<ParameterResult>
{new ParameterResult("helpnum", "", "integer"),
}, new List<string> { },
                    "Displays a runtime help text.",
                    ""));
                    _systemFunctions.Add("startlog", new BuiltinFunction("startlog", null, new List<ParameterResult>
{new ParameterResult("filename", "", "string"),
}, new List<string> { },
                    "Initializes error logging and opens the error log file passed as the parameter.",
                    ""));
                    _systemFunctions.Add("upshift", new BuiltinFunction("upshift", null, new List<ParameterResult>
{new ParameterResult("source", "", "string"),
}, new List<string> { "string", },
                    "Converts a string to uppercase.",
                    ""));

                    #endregion
                    _builtinsInitialized = true;
                }
            }
        }
    }
}
