using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST
{
    public partial class GeneroAst
    {
        private static bool _builtinsInitialized = false;
        private static object _builtinsInitLock = new object();

        private static Dictionary<string, IAnalysisResult> _systemVariables;
        public static IDictionary<string, IAnalysisResult> SystemVariables
        {
            get
            {
                if (_systemVariables == null)
                    _systemVariables = new Dictionary<string, IAnalysisResult>();
                return _systemVariables;
            }
        }

        private static Dictionary<string, IAnalysisResult> _systemConstants;
        public static IDictionary<string, IAnalysisResult> SystemConstants
        {
            get
            {
                if (_systemConstants == null)
                    _systemConstants = new Dictionary<string, IAnalysisResult>();
                return _systemConstants;
            }
        }

        private static Dictionary<string, IFunctionResult> _systemFunctions;
        public static IDictionary<string, IFunctionResult> SystemFunctions
        {
            get
            {
                if (_systemFunctions == null)
                    _systemFunctions = new Dictionary<string, IFunctionResult>();
                return _systemFunctions;
            }
        }

        private static Dictionary<string, IFunctionResult> _arrayFunctions;
        public static IDictionary<string, IFunctionResult> ArrayFunctions
        {
            get
            {
                if (_arrayFunctions == null)
                    _arrayFunctions = new Dictionary<string, IFunctionResult>();
                return _arrayFunctions;
            }
        }

        private static Dictionary<string, IFunctionResult> _stringFunctions;
        public static IDictionary<string, IFunctionResult> StringFunctions
        {
            get
            {
                if (_stringFunctions == null)
                    _stringFunctions = new Dictionary<string, IFunctionResult>();
                return _stringFunctions;
            }
        }

        private static void InitializeBuiltins()
        {
            lock (_builtinsInitLock)
            {
                if (!_builtinsInitialized)
                {
                    // System variables
                    SystemVariables.Add("status", new ProgramRegister("status", "int"));
                    SystemVariables.Add("int_flag", new ProgramRegister("int_flag", "boolean"));
                    SystemVariables.Add("quit_flag", new ProgramRegister("quit_flag", "boolean"));
                    SystemVariables.Add("sqlca", new ProgramRegister("sqlca", "record", new List<ProgramRegister>
                        {
                            new ProgramRegister("sqlcode", "int"),
                            new ProgramRegister("sqlerrm", "char(71)"),
                            new ProgramRegister("sqlerrp", "char(8)"),
                            new ProgramRegister("sqlerrd", "array[6] of int"),
                            new ProgramRegister("sqlawarn", "char(7)")
                        }));

                    // System constants
                    SystemConstants.Add("null", new SystemConstant("null", null, null));
                    SystemConstants.Add("true", new SystemConstant("true", "int", 1));
                    SystemConstants.Add("false", new SystemConstant("false", "int", 2));
                    SystemConstants.Add("notfound", new SystemConstant("notfound", "int", 100));

                    // Generated Builtin Functions
                    ArrayFunctions.Add("appendElement", new BuiltinFunction("appendElement", new List<ParameterResult> { }, new List<string> { },
"Adds a new element at the end of a dynamic array. This method has no effect on a static array."));
                    ArrayFunctions.Add("clear", new BuiltinFunction("clear", new List<ParameterResult> { }, new List<string> { },
                    "Removes all elements in a dynamic array. Sets all elements to NULL in a static array."));
                    ArrayFunctions.Add("deleteElement", new BuiltinFunction("deleteElement", new List<ParameterResult>
{new ParameterResult("pos", "", "integer"),
}, new List<string> { },
                    "Removes an element at the given position. In a static or dynamic array, the elements after the given position are moved up. In a dynamic array, the number of elements is decremented by 1."));
                    ArrayFunctions.Add("getLength", new BuiltinFunction("getLength", new List<ParameterResult> { }, new List<string> { "integer", },
                    "Returns the length of a one-dimensional array."));
                    ArrayFunctions.Add("insertElement", new BuiltinFunction("insertElement", new List<ParameterResult>
{new ParameterResult("pos", "", "integer"),
}, new List<string> { },
                    "Inserts a new element at the given position. In a static or dynamic array, the elements after the given position are moved down. In a dynamic array, the number of elements increments by 1."));
                    StringFunctions.Add("append", new BuiltinFunction("append", new List<ParameterResult>
{new ParameterResult("part", "", "string"),
}, new List<string> { "string", },
                    "Returns a new string made by adding part to the end of the current string."));
                    StringFunctions.Add("equals", new BuiltinFunction("equals", new List<ParameterResult>
{new ParameterResult("source", "", "string"),
}, new List<string> { "integer", },
                    "Returns TRUE if the string passed as parameters matches the current string. If one of the strings is NULL the method returns FALSE."));
                    StringFunctions.Add("equalsIgnoreCase", new BuiltinFunction("equalsIgnoreCase", new List<ParameterResult>
{new ParameterResult("source", "", "string"),
}, new List<string> { "integer", },
                    "Returns TRUE if the string passed as parameters matches the current string, ignoring character case. If one of the strings is NULL the method returns FALSE."));
                    StringFunctions.Add("getCharAt", new BuiltinFunction("getCharAt", new List<ParameterResult>
{new ParameterResult("pos", "", "integer"),
}, new List<string> { "string", },
                    "Returns the character at the position pos (starts at 1). The unit for character positions depend on the length semantics. In BLS, the method returns NULL if pos does not match a valid character-byte position in the current string."));
                    StringFunctions.Add("getIndexOf", new BuiltinFunction("getIndexOf", new List<ParameterResult>
{new ParameterResult("part", "", "string"),
new ParameterResult("spos", "", "integer"),
}, new List<string> { "integer", },
                    "Returns the position of the substring part in the current string, starting from position spos. The unit for character positions depend on the length semantics used. Returns zero if the substring was not found. Returns -1 if string is NULL."));
                    StringFunctions.Add("getLength", new BuiltinFunction("getLength", new List<ParameterResult> { }, new List<string> { "integer", },
                    "Returns the lenfth of the string, including trailing blanks (Note that the LENGTH() built-in function ignores trailing blanks). The unit for character length depend on the length semantics used."));
                    StringFunctions.Add("subString", new BuiltinFunction("subString", new List<ParameterResult>
{new ParameterResult("spos", "", "integer"),
new ParameterResult("epos", "", "integer"),
}, new List<string> { "string", },
                    "Returns the substring starting at position spos and ending at epos. The unit for character positions depend on the length semantics used. In BLS, returns NULL if the positions do not delimit a valid substring in the current string."));
                    StringFunctions.Add("toLowerCase", new BuiltinFunction("toLowerCase", new List<ParameterResult> { }, new List<string> { "string", },
                    "Converts the current string to lowercase. Returns NULL if the string is null."));
                    StringFunctions.Add("toUpperCase", new BuiltinFunction("toUpperCase", new List<ParameterResult> { }, new List<string> { "string", },
                    "Converts the current string to uppercase. Returns NULL if the string is null."));
                    StringFunctions.Add("trim", new BuiltinFunction("trim", new List<ParameterResult> { }, new List<string> { "string", },
                    "Removes white space characters from the beginning and end of the current string. Returns NULL if the string is null."));
                    StringFunctions.Add("trimLeft", new BuiltinFunction("trimLeft", new List<ParameterResult> { }, new List<string> { "string", },
                    "Removes white space characters from the beginning of the current string. Returns NULL if the string is null."));
                    StringFunctions.Add("trimRight", new BuiltinFunction("trimRight", new List<ParameterResult> { }, new List<string> { "string", },
                    "Removes white space characters from the end of the current string. Returns NULL if the string is null."));
                    SystemFunctions.Add("arg_val", new BuiltinFunction("arg_val", new List<ParameterResult>
{new ParameterResult("position", "", "integer"),
}, new List<string> { "string", },
                    "Returns a command line argument by position."));
                    SystemFunctions.Add("arr_count", new BuiltinFunction("arr_count", new List<ParameterResult> { }, new List<string> { "integer", },
                    "Returns the number of rows entered during a INPUT ARRAY statement."));
                    SystemFunctions.Add("arr_curr", new BuiltinFunction("arr_curr", new List<ParameterResult> { }, new List<string> { "integer", },
                    "Returns the current row in a DISPLAY ARRAY or INPUT ARRAY."));
                    SystemFunctions.Add("downshift", new BuiltinFunction("downshift", new List<ParameterResult>
{new ParameterResult("source", "", "string"),
}, new List<string> { "string", },
                    "Converts a string to lowercase."));
                    SystemFunctions.Add("err_get", new BuiltinFunction("err_get", new List<ParameterResult>
{new ParameterResult("errnum", "", "integer"),
}, new List<string> { "string", },
                    "Returns the text corresponding to an error number."));
                    SystemFunctions.Add("err_print", new BuiltinFunction("err_print", new List<ParameterResult>
{new ParameterResult("errnum", "", "integer"),
}, new List<string> { },
                    "Prints in the error line the text corresponding to an error number."));
                    SystemFunctions.Add("err_quit", new BuiltinFunction("err_quit", new List<ParameterResult>
{new ParameterResult("errnum", "", "integer"),
}, new List<string> { },
                    "Prints in the error line the text corresponding to an error number and terminates the program"));
                    SystemFunctions.Add("errorlog", new BuiltinFunction("errorlog", new List<ParameterResult>
{new ParameterResult("text", "", "string"),
}, new List<string> { },
                    "Copies the string passed as parameter into the error log file."));
                    SystemFunctions.Add("fgl_buffertouched", new BuiltinFunction("fgl_buffertouched", new List<ParameterResult> { }, new List<string> { "integer", },
                    "Returns TRUE if the input buffer was modified in the current field"));
                    SystemFunctions.Add("fgl_db_driver_type", new BuiltinFunction("fgl_db_driver_type", new List<ParameterResult> { }, new List<string> { "char(3)", },
                    "Returns the 3-letter identifier/code of the current database driver."));
                    SystemFunctions.Add("fgl_decimal_truncate", new BuiltinFunction("fgl_decimal_truncate", new List<ParameterResult>
{new ParameterResult("value", "", "decimal"),
new ParameterResult("decimals", "", "integer"),
}, new List<string> { "decimal", },
                    "Returns a decimal truncated to the precision passed as parameter."));
                    SystemFunctions.Add("fgl_decimal_sqrt", new BuiltinFunction("fgl_decimal_sqrt", new List<ParameterResult>
{new ParameterResult("value", "", "decimal"),
}, new List<string> { "decimal", },
                    "Computes the square root of the decimal passed as parameter."));
                    SystemFunctions.Add("fgl_decimal_exp", new BuiltinFunction("fgl_decimal_exp", new List<ParameterResult>
{new ParameterResult("value", "", "decimal"),
}, new List<string> { "decimal", },
                    "Returns the value of Euler's constant (e) raised to the power of the decimal passed as parameter"));
                    SystemFunctions.Add("fgl_decimal_logn", new BuiltinFunction("fgl_decimal_logn", new List<ParameterResult>
{new ParameterResult("value", "", "decimal"),
}, new List<string> { "decimal", },
                    "Returns the natural logarithm of the decimal passed as parameter."));
                    SystemFunctions.Add("fgl_decimal_power", new BuiltinFunction("fgl_decimal_power", new List<ParameterResult>
{new ParameterResult("base", "", "decimal"),
new ParameterResult("exp", "", "decimal"),
}, new List<string> { "decimal", },
                    "Raises decimal to the power of the real exponent."));
                    SystemFunctions.Add("fgl_dialog_getbuffer", new BuiltinFunction("fgl_dialog_getbuffer", new List<ParameterResult> { }, new List<string> { "string", },
                    "Returns the text of the input buffer of the current field."));
                    SystemFunctions.Add("fgl_dialog_getbufferlength", new BuiltinFunction("fgl_dialog_getbufferlength", new List<ParameterResult> { }, new List<string> { "integer", },
                    "Returns the number of rows to feed a paged DISPLAY ARRAY."));
                    SystemFunctions.Add("fgl_dialog_getbufferstart", new BuiltinFunction("fgl_dialog_getbufferstart", new List<ParameterResult> { }, new List<string> { "integer", },
                    "Returns the row offset of the page to feed a paged display array."));
                    SystemFunctions.Add("fgl_dialog_getcursor", new BuiltinFunction("fgl_dialog_getcursor", new List<ParameterResult> { }, new List<string> { "integer", },
                    "Returns the position of the edit cursor in the current field."));
                    SystemFunctions.Add("fgl_dialog_getfieldname", new BuiltinFunction("fgl_dialog_getfieldname", new List<ParameterResult> { }, new List<string> { "string", },
                    "Returns the name of the current input field."));
                    SystemFunctions.Add("fgl_dialog_getkeylabel", new BuiltinFunction("fgl_dialog_getkeylabel", new List<ParameterResult>
{new ParameterResult("keyname", "", "string"),
}, new List<string> { "string", },
                    "Returns the label associated to a key for the current interactive instruction"));
                    SystemFunctions.Add("fgl_dialog_getselectionend", new BuiltinFunction("fgl_dialog_getselectionend", new List<ParameterResult> { }, new List<string> { "integer", },
                    "Returns the position of the last selected character in the current field."));
                    SystemFunctions.Add("fgl_dialog_infield", new BuiltinFunction("fgl_dialog_infield", new List<ParameterResult>
{new ParameterResult("field-name", "", "string"),
}, new List<string> { "integer", },
                    "This function checks for the current input field."));
                    SystemFunctions.Add("fgl_dialog_setbuffer", new BuiltinFunction("fgl_dialog_setbuffer", new List<ParameterResult>
{new ParameterResult("value", "", "string"),
}, new List<string> { },
                    "Sets the input buffer of the current field."));
                    SystemFunctions.Add("fgl_dialog_setcurrline", new BuiltinFunction("fgl_dialog_setcurrline", new List<ParameterResult>
{new ParameterResult("line", "", "integer"),
new ParameterResult("row", "", "integer"),
}, new List<string> { },
                    "This function moves to a specific row in a record list."));
                    SystemFunctions.Add("fgl_dialog_setcursor", new BuiltinFunction("fgl_dialog_setcursor", new List<ParameterResult>
{new ParameterResult("position", "", "integer"),
}, new List<string> { },
                    "This function sets the position of the edit cursor in the current field."));
                    SystemFunctions.Add("fgl_dialog_setfieldorder", new BuiltinFunction("fgl_dialog_setfieldorder", new List<ParameterResult>
{new ParameterResult("active", "", "integer"),
}, new List<string> { },
                    "This function enables or disables field order constraint."));
                    SystemFunctions.Add("fgl_dialog_setkeylabel", new BuiltinFunction("fgl_dialog_setkeylabel", new List<ParameterResult>
{new ParameterResult("keyname", "", "string"),
new ParameterResult("label", "", "string"),
}, new List<string> { },
                    "Sets the label associated to a key for the current interactive instruction."));
                    SystemFunctions.Add("fgl_dialog_setselection", new BuiltinFunction("fgl_dialog_setselection", new List<ParameterResult>
{new ParameterResult("cursor", "", "integer"),
new ParameterResult("end", "", "integer"),
}, new List<string> { },
                    "Selects the text in the current field."));
                    SystemFunctions.Add("fgl_drawbox", new BuiltinFunction("fgl_drawbox", new List<ParameterResult>
{new ParameterResult("height", "", "integer"),
new ParameterResult("width", "", "integer"),
new ParameterResult("line", "", "integer"),
new ParameterResult("column", "", "integer"),
new ParameterResult("color", "", "integer"),
}, new List<string> { },
                    "Draws a rectangle in the current window."));
                    SystemFunctions.Add("fgl_drawline", new BuiltinFunction("fgl_drawline", new List<ParameterResult>
{new ParameterResult("column", "", "integer"),
new ParameterResult("line", "", "integer"),
new ParameterResult("width", "", "integer"),
new ParameterResult("type", "", "char(1)"),
new ParameterResult("color", "", "integer"),
}, new List<string> { },
                    "Draws a line in the current window (TUI and traditional mode)."));
                    SystemFunctions.Add("fgl_getenv", new BuiltinFunction("fgl_getenv", new List<ParameterResult>
{new ParameterResult("variable", "", "string"),
}, new List<string> { "string", },
                    "Returns the value of the environment variable."));
                    SystemFunctions.Add("fgl_getfile", new BuiltinFunction("fgl_getfile", new List<ParameterResult>
{new ParameterResult("src", "", "string"),
new ParameterResult("dest", "", "string"),
}, new List<string> { },
                    "Transfers a file from the front end workstation to the application server machine."));
                    SystemFunctions.Add("fgl_gethelp", new BuiltinFunction("fgl_gethelp", new List<ParameterResult>
{new ParameterResult("help-id", "", "integer"),
}, new List<string> { "string", },
                    "Returns the help text according to its identifier by reading the current help file."));
                    SystemFunctions.Add("fgl_getkey", new BuiltinFunction("fgl_getkey", new List<ParameterResult> { }, new List<string> { "integer", },
                    "Waits for a keystroke and returns the key number."));
                    SystemFunctions.Add("fgl_getkeylabel", new BuiltinFunction("fgl_getkeylabel", new List<ParameterResult>
{new ParameterResult("keyname", "", "string"),
}, new List<string> { "string", },
                    "Returns the default label associated to a key."));
                    SystemFunctions.Add("fgl_getpid", new BuiltinFunction("fgl_getpid", new List<ParameterResult> { }, new List<string> { "integer", },
                    "Returns the system process identifier."));
                    SystemFunctions.Add("fgl_getresource", new BuiltinFunction("fgl_getresource", new List<ParameterResult>
{new ParameterResult("name", "", "string"),
}, new List<string> { "string", },
                    "Returns the value of an FGLPROFILE entry."));
                    SystemFunctions.Add("fgl_getversion", new BuiltinFunction("fgl_getversion", new List<ParameterResult> { }, new List<string> { "string", },
                    "Returns the build number of the runtime system."));
                    SystemFunctions.Add("fgl_getwin_height", new BuiltinFunction("fgl_getwin_height", new List<ParameterResult> { }, new List<string> { "integer", },
                    "Returns the number of rows of the current window."));
                    SystemFunctions.Add("fgl_getwin_width", new BuiltinFunction("fgl_getwin_width", new List<ParameterResult> { }, new List<string> { "integer", },
                    "Returns the width of the current window as a number of columns."));
                    SystemFunctions.Add("fgl_getwin_x", new BuiltinFunction("fgl_getwin_x", new List<ParameterResult> { }, new List<string> { "integer", },
                    "Returns the horizontal position of the current window."));
                    SystemFunctions.Add("fgl_getwin_y", new BuiltinFunction("fgl_getwin_y", new List<ParameterResult> { }, new List<string> { "integer", },
                    "Returns the vertical position of the current window."));
                    SystemFunctions.Add("fgl_keyval", new BuiltinFunction("fgl_keyval", new List<ParameterResult>
{new ParameterResult("string", "", "string"),
}, new List<string> { "integer", },
                    "Returns the key code of a logical or physical key."));
                    SystemFunctions.Add("fgl_lastkey", new BuiltinFunction("fgl_lastkey", new List<ParameterResult> { }, new List<string> { "integer", },
                    "Returns the key code corresponding to the logical key that the user most recently typed in the form."));
                    SystemFunctions.Add("fgl_putfile", new BuiltinFunction("fgl_putfile", new List<ParameterResult>
{new ParameterResult("src", "", "string"),
new ParameterResult("dest", "", "string"),
}, new List<string> { },
                    "Transfers a file from the application server machine to the front end workstation."));
                    SystemFunctions.Add("fgl_report_print_binary_file", new BuiltinFunction("fgl_report_print_binary_file", new List<ParameterResult>
{new ParameterResult("filename", "", "string"),
}, new List<string> { },
                    "Prints a file containing binary data during a report."));
                    SystemFunctions.Add("fgl_report_set_document_handler", new BuiltinFunction("fgl_report_set_document_handler", new List<ParameterResult>
{new ParameterResult("handler", "", "om.SaxDocumentHandler"),
}, new List<string> { },
                    "Redirects the next report to an XML document handler."));
                    SystemFunctions.Add("fgl_scr_size", new BuiltinFunction("fgl_scr_size", new List<ParameterResult>
{new ParameterResult("screen-array", "", "string"),
}, new List<string> { "integer", },
                    "Returns the size of the specified screen array in the current form."));
                    SystemFunctions.Add("fgl_set_arr_curr", new BuiltinFunction("fgl_set_arr_curr", new List<ParameterResult>
{new ParameterResult("row", "", "integer"),
}, new List<string> { },
                    "Moves to a specific row in a record list."));
                    SystemFunctions.Add("fgl_setenv", new BuiltinFunction("fgl_setenv", new List<ParameterResult>
{new ParameterResult("variable", "", "string"),
new ParameterResult("value", "", "string"),
}, new List<string> { },
                    "Sets the value of an environment variable."));
                    SystemFunctions.Add("fgl_setkeylabel", new BuiltinFunction("fgl_setkeylabel", new List<ParameterResult>
{new ParameterResult("keyname", "", "string"),
new ParameterResult("label", "", "string"),
}, new List<string> { },
                    "Sets the default label associated to a key."));
                    SystemFunctions.Add("fgl_setsize", new BuiltinFunction("fgl_setsize", new List<ParameterResult>
{new ParameterResult("height", "", "integer"),
new ParameterResult("width", "", "integer"),
}, new List<string> { },
                    "Sets the size of the main application window."));
                    SystemFunctions.Add("fgl_settitle", new BuiltinFunction("fgl_settitle", new List<ParameterResult>
{new ParameterResult("label", "", "string"),
}, new List<string> { },
                    "Sets the title of the current application window."));
                    SystemFunctions.Add("fgl_system", new BuiltinFunction("fgl_system", new List<ParameterResult>
{new ParameterResult("command", "", "string"),
}, new List<string> { },
                    "Runs a command on the application server."));
                    SystemFunctions.Add("fgl_width", new BuiltinFunction("fgl_width", new List<ParameterResult>
{new ParameterResult("expression", "", "string"),
}, new List<string> { "integer", },
                    "Returns the number of columns needed to represent the printed version of the expression."));
                    SystemFunctions.Add("fgl_window_getoption", new BuiltinFunction("fgl_window_getoption", new List<ParameterResult>
{new ParameterResult("attribute", "", "string"),
}, new List<string> { "string", },
                    "Returns attributes of the current window."));
                    SystemFunctions.Add("length", new BuiltinFunction("length", new List<ParameterResult>
{new ParameterResult("expression", "", "string"),
}, new List<string> { "integer", },
                    "Returns the number of the character string passed as parameter."));
                    SystemFunctions.Add("num_args", new BuiltinFunction("num_args", new List<ParameterResult> { }, new List<string> { "integer", },
                    "Returns the number of program arguments."));
                    SystemFunctions.Add("scr_line", new BuiltinFunction("scr_line", new List<ParameterResult> { }, new List<string> { "integer", },
                    "Returns the index of the current row in the screen array."));
                    SystemFunctions.Add("set_count", new BuiltinFunction("set_count", new List<ParameterResult>
{new ParameterResult("nbrows", "", "integer"),
}, new List<string> { },
                    "Defines the number of rows containing explicit data in a static array used by the next dialog."));
                    SystemFunctions.Add("show_help", new BuiltinFunction("show_help", new List<ParameterResult>
{new ParameterResult("helpnum", "", "integer"),
}, new List<string> { },
                    "Displays a runtime help text."));
                    SystemFunctions.Add("startlog", new BuiltinFunction("startlog", new List<ParameterResult>
{new ParameterResult("filename", "", "string"),
}, new List<string> { },
                    "Initializes error logging and opens the error log file passed as the parameter."));
                    SystemFunctions.Add("upshift", new BuiltinFunction("upshift", new List<ParameterResult>
{new ParameterResult("source", "", "string"),
}, new List<string> { "string", },
                    "Converts a string to uppercase."));
                    // end generated Builtin Functions


                    _builtinsInitialized = true;
                }
            }
        }
    }

    public class ProgramRegister : IAnalysisResult
    {
        private ProgramRegister _parentRegister;
        private readonly string _name;
        private readonly string _typeName;
        private readonly Dictionary<string, ProgramRegister> _childRegisters;

        public ProgramRegister(string name, string typeName, IEnumerable<ProgramRegister> childRegisters = null)
        {
            _parentRegister = null;
            _name = name;
            _typeName = typeName;
            _childRegisters = new Dictionary<string, ProgramRegister>(StringComparer.OrdinalIgnoreCase);
            if (childRegisters != null)
            {
                foreach (var reg in childRegisters)
                {
                    reg._parentRegister = this;
                    _childRegisters.Add(reg._name, reg);
                }
            }
        }

        const string _scope = "program register";
        public string Scope
        {
            get
            {
                return _scope;
            }
            set
            {
            }
        }

        public string Name
        {
            get { return _name; }
        }

        public string Documentation
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                if (!string.IsNullOrWhiteSpace(Scope))
                {
                    sb.AppendFormat("({0}) ", Scope);
                }
                if (_parentRegister != null)
                {
                    sb.AppendFormat("{0}.", _parentRegister.Name);
                }
                sb.AppendFormat("{0} {1}", Name, _typeName);
                return sb.ToString();
            }
        }

        public int LocationIndex
        {
            get { return -1; }
        }


        public IAnalysisResult GetMember(string name, GeneroAst ast)
        {
            ProgramRegister progReg = null;
            if (_childRegisters != null)
            {
                _childRegisters.TryGetValue(name, out progReg);
            }
            return progReg;
        }

        public IEnumerable<MemberResult> GetMembers(GeneroAst ast)
        {
            if (_childRegisters != null)
            {
                return _childRegisters.Values.Select(x => new MemberResult(x.Name, x, GeneroMemberType.Variable, ast));
            }
            return null;
        }

        public bool HasChildFunctions
        {
            get { return false; }
        }
    }

    public class SystemConstant : IAnalysisResult
    {
        private readonly string _name;
        private readonly string _typeName;
        private readonly object _value;

        public SystemConstant(string name, string typeName, object value)
        {
            _name = name;
            _typeName = typeName;
            _value = value;
        }

        const string _scope = "system constant";
        public string Scope
        {
            get
            {
                return _scope;
            }
            set
            {
            }
        }

        public string Name
        {
            get { return _name; }
        }

        public string Documentation
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                if (!string.IsNullOrWhiteSpace(Scope))
                {
                    sb.AppendFormat("({0}) ", Scope);
                }
                sb.Append(Name);
                if (!string.IsNullOrWhiteSpace(_typeName))
                {
                    sb.AppendFormat(" ({0})", _typeName);
                }
                if (_value != null)
                    sb.AppendFormat(" = {0}", _value);
                return sb.ToString();
            }
        }

        public int LocationIndex
        {
            get { return -1; }
        }

        public IAnalysisResult GetMember(string name, GeneroAst ast)
        {
            return null;
        }

        public IEnumerable<MemberResult> GetMembers(GeneroAst ast)
        {
            return null;
        }

        public bool HasChildFunctions
        {
            get { return false; }
        }
    }

    public class BuiltinFunction : IFunctionResult
    {
        private readonly string _name;
        private readonly List<ParameterResult> _parameters;
        private readonly List<string> _returns;
        private readonly string _description;

        public BuiltinFunction(string name, IEnumerable<ParameterResult> parameters, IEnumerable<string> returns, string description)
        {
            _name = name;
            _description = description;
            _parameters = new List<ParameterResult>(parameters);
            _returns = new List<string>(returns);
        }

        public ParameterResult[] Parameters
        {
            get { return _parameters.ToArray(); }
        }

        public AccessModifier AccessModifier
        {
            get { return Analysis.AccessModifier.Public; }
        }

        public string FunctionDocumentation
        {
            get { return _description; }
        }

        private Dictionary<string, IAnalysisResult> _dummyDict = new Dictionary<string, IAnalysisResult>();
        public IDictionary<string, IAnalysisResult> Variables
        {
            get { return _dummyDict; }
        }

        public IDictionary<string, IAnalysisResult> Types
        {
            get { return _dummyDict; }
        }

        public IDictionary<string, IAnalysisResult> Constants
        {
            get { return _dummyDict; }
        }

        private string _scope = "system function";
        public string Scope
        {
            get
            {
                return _scope;
            }
            set
            {
            }
        }

        public string Name
        {
            get { return _name; }
        }

        public string Documentation
        {
            get
            {
                StringBuilder sb = new StringBuilder();

                if (!string.IsNullOrWhiteSpace(Scope))
                {
                    sb.AppendFormat("({0}) ", Scope);
                }

                sb.Append(Name);
                sb.Append('(');

                // if there are any parameters put them in
                int total = _parameters.Count;
                int i = 0;
                foreach (var varDef in _parameters)
                {
                    sb.AppendFormat("{0} {1}", varDef.Type, varDef.Name);
                    if (i + 1 < total)
                    {
                        sb.Append(", ");
                    }
                    i++;
                }

                sb.Append(')');

                if (_returns.Count > 0)
                {
                    sb.AppendLine();
                    sb.Append("returning ");
                    foreach (var ret in _returns)
                    {
                        sb.Append(ret);
                        if (i + 1 < total)
                        {
                            sb.Append(", ");
                        }
                        i++;
                    }
                }
                return sb.ToString();
            }
        }

        public int LocationIndex
        {
            get { return -1; }
        }

        public IAnalysisResult GetMember(string name, GeneroAst ast)
        {
            return null;
        }

        public IEnumerable<MemberResult> GetMembers(GeneroAst ast)
        {
            return null;
        }

        public bool HasChildFunctions
        {
            get { return false; }
        }

        public bool CanOutline
        {
            get { return false; }
        }

        public int StartIndex
        {
            get
            {
                return -1;
            }
            set
            {
            }
        }

        public int EndIndex
        {
            get
            {
                return -1;
            }
            set
            {
            }
        }

        public int DecoratorEnd
        {
            get
            {
                return -1;
            }
            set
            {
            }
        }
    }

    public class SystemClass : IAnalysisResult
    {
        private readonly string _name;
        private readonly Dictionary<string, IFunctionResult> _memberFunctions;

        public SystemClass(string name, IEnumerable<IFunctionResult> memberFunctions)
        {
            _name = name;
            _memberFunctions = new Dictionary<string, IFunctionResult>(StringComparer.OrdinalIgnoreCase);
            foreach (var func in memberFunctions)
                _memberFunctions.Add(func.Name, func);
        }

        private string _scope = "system class";
        public string Scope
        {
            get
            {
                return _scope;
            }
            set
            {
            }
        }

        public string Name
        {
            get { return _name; }
        }

        public string Documentation
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                if (!string.IsNullOrWhiteSpace(Scope))
                {
                    sb.AppendFormat("({0}) ", Scope);
                }
                sb.Append(Name);
                return sb.ToString();
            }
        }

        public int LocationIndex
        {
            get { return -1; }
        }

        public IAnalysisResult GetMember(string name, GeneroAst ast)
        {
            IFunctionResult funcRes = null;
            _memberFunctions.TryGetValue(name, out funcRes);
            return funcRes;
        }

        public IEnumerable<MemberResult> GetMembers(GeneroAst ast)
        {
            return _memberFunctions.Values.Select(x => new MemberResult(x.Name, x, GeneroMemberType.Method, ast));
        }

        public bool HasChildFunctions
        {
            get { return false; }
        }
    }
}
