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

namespace VSGenero.Analysis.Parsing.AST
{
    public partial class GeneroAst
    {
        private static bool _packagesInitialized = false;
        private static object _packagesInitLock = new object();

        private Dictionary<string, bool> _importedPackages;

        private static Dictionary<string, GeneroPackage> _packages;
        public static IDictionary<string, GeneroPackage> Packages
        {
            get
            {
                if (_packages == null)
                    _packages = new Dictionary<string, GeneroPackage>(StringComparer.OrdinalIgnoreCase);
                return _packages;
            }
        }

        private void InitializeImportedPackages()
        {
            string[] enabledPkgs = { "base", "ui", "om" };
            string[] disabledPkgs = { "util", "os", "com", "xml", "security" };
            _importedPackages = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
            foreach (var enable in enabledPkgs)
                _importedPackages.Add(enable, true);
            foreach (var disable in disabledPkgs)
                _importedPackages.Add(disable, false);
        }

        private static void InitializePackages()
        {
            lock(_packagesInitLock)
            {
                if(!_packagesInitialized)
                {
                    #region Generated Package Init Code
                    Packages.Add("base", new GeneroPackage("base", false, new List<GeneroPackageClass>
{
	new GeneroPackageClass("Application", "base", true, new List<GeneroPackageClassMethod>
	{
		new GeneroPackageClassMethod("getArgument", "base.Application", true, "Returns the command line argument by position.", new List<ParameterResult>
		{
			new ParameterResult("index", "", "integer")
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("getArgumentCount", "base.Application", true, "Returns the total number of command line arguments.", new List<ParameterResult>
		{
		}, new List<string> {"integer"}),
		new GeneroPackageClassMethod("getProgramDir", "base.Application", true, "Returns the directory path of the current program.", new List<ParameterResult>
		{
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("getProgramName", "base.Application", true, "Returns the name of the current program.", new List<ParameterResult>
		{
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("getFglDir", "base.Application", true, "Returns the path to the FGLDIR installation directory.", new List<ParameterResult>
		{
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("getResourceEntry", "base.Application", true, "Returns the value of an FGLPROFILE entry.", new List<ParameterResult>
		{
			new ParameterResult("entry", "", "string")
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("getStackTrace", "base.Application", true, "Returns the function call stack trace.", new List<ParameterResult>
		{
		}, new List<string> {"string"})
	}),
	new GeneroPackageClass("Channel", "base", false, new List<GeneroPackageClassMethod>
	{
		new GeneroPackageClassMethod("create", "base.Channel", true, "Create a new channel object.", new List<ParameterResult>
		{
		}, new List<string> {"base.Channel"}),
		new GeneroPackageClassMethod("close", "base.Channel", false, "Closes the channel object.", new List<ParameterResult>
		{
		}, new List<string> {}),
		new GeneroPackageClassMethod("isEof", "base.Channel", false, "Detect the end of a file.", new List<ParameterResult>
		{
		}, new List<string> {"boolean"}),
		new GeneroPackageClassMethod("openFile", "base.Channel", false, "Opening a file channel.", new List<ParameterResult>
		{
			new ParameterResult("path", "", "string"),
			new ParameterResult("mode", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("openPipe", "base.Channel", false, "Opening a pipe channel to a sub-process.", new List<ParameterResult>
		{
			new ParameterResult("cmd", "", "string"),
			new ParameterResult("mode", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("openClientSocket", "base.Channel", false, "Open a TCP client socket channel.", new List<ParameterResult>
		{
			new ParameterResult("host", "", "string"),
			new ParameterResult("port", "", "integer"),
			new ParameterResult("mode", "", "string"),
			new ParameterResult("timeout", "", "integer")
		}, new List<string> {}),
		new GeneroPackageClassMethod("setDelimiter", "base.Channel", false, "Define the value delimiter for a channel.", new List<ParameterResult>
		{
			new ParameterResult("delim", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("dataAvailable", "base.Channel", false, "Tests if some data can be read from the channel.", new List<ParameterResult>
		{
		}, new List<string> {"boolean"}),
		new GeneroPackageClassMethod("readLine", "base.Channel", false, "Read a complete line from the channel.", new List<ParameterResult>
		{
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("writeLine", "base.Channel", false, "Write a complete line to the channel.", new List<ParameterResult>
		{
			new ParameterResult("line", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("read", "base.Channel", false, "Reads a list of data delimited by a separator from the ch", new List<ParameterResult>
		{
			new ParameterResult("variableList", "", "square-brace-list")
		}, new List<string> {"integer"}),
		new GeneroPackageClassMethod("write", "base.Channel", false, "Writes a list of data delimited by a separator to the channel.", new List<ParameterResult>
		{
			new ParameterResult("variableList", "", "square-brace-list")
		}, new List<string> {})
	}),
	new GeneroPackageClass("StringBuffer", "base", false, new List<GeneroPackageClassMethod>
	{
		new GeneroPackageClassMethod("create", "base.StringBuffer", true, "Create a string buffer object.", new List<ParameterResult>
		{
		}, new List<string> {"base.StringBuffer"}),
		new GeneroPackageClassMethod("append", "base.StringBuffer", false, "Append a string at the end of the current string.", new List<ParameterResult>
		{
			new ParameterResult("part", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("clear", "base.StringBuffer", false, "Clear the string buffer.", new List<ParameterResult>
		{
		}, new List<string> {}),
		new GeneroPackageClassMethod("equals", "base.StringBuffer", false, "Compare strings (case sensitive).", new List<ParameterResult>
		{
			new ParameterResult("reference", "", "string")
		}, new List<string> {"boolean"}),
		new GeneroPackageClassMethod("equalsIgnoreCase", "base.StringBuffer", false, "Compare strings (case insensitive).", new List<ParameterResult>
		{
			new ParameterResult("reference", "", "string")
		}, new List<string> {"boolean"}),
		new GeneroPackageClassMethod("getCharAt", "base.StringBuffer", false, "Return the character at a specified position.", new List<ParameterResult>
		{
			new ParameterResult("position", "", "integer")
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("getIndexOf", "base.StringBuffer", false, "Return the position of a substring.", new List<ParameterResult>
		{
			new ParameterResult("substr", "", "string"),
			new ParameterResult("start", "", "integer")
		}, new List<string> {"integer"}),
		new GeneroPackageClassMethod("getLength", "base.StringBuffer", false, "Return the length of a string.", new List<ParameterResult>
		{
		}, new List<string> {"integer"}),
		new GeneroPackageClassMethod("insertAt", "base.StringBuffer", false, "Insert a string at a given position.", new List<ParameterResult>
		{
			new ParameterResult("part", "", "string"),
			new ParameterResult("pos", "", "integer")
		}, new List<string> {}),
		new GeneroPackageClassMethod("replace", "base.StringBuffer", false, "Replace one string with another.", new List<ParameterResult>
		{
			new ParameterResult("old", "", "string"),
			new ParameterResult("new", "", "string"),
			new ParameterResult("numOccur", "", "integer")
		}, new List<string> {}),
		new GeneroPackageClassMethod("replaceAt", "base.StringBuffer", false, "Replace part of a string with another string.", new List<ParameterResult>
		{
			new ParameterResult("start", "", "integer"),
			new ParameterResult("length", "", "integer"),
			new ParameterResult("new", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("subString", "base.StringBuffer", false, "Return the substring at the specified position.", new List<ParameterResult>
		{
			new ParameterResult("start", "", "integer"),
			new ParameterResult("end", "", "integer")
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("toLowerCase", "base.StringBuffer", false, "Converts the string in the buffer to lower case.", new List<ParameterResult>
		{
		}, new List<string> {}),
		new GeneroPackageClassMethod("toUpperCase", "base.StringBuffer", false, "Converts the string in the buffer to upper case.", new List<ParameterResult>
		{
		}, new List<string> {}),
		new GeneroPackageClassMethod("toString", "base.StringBuffer", false, "Create a STRING from the string buffer.", new List<ParameterResult>
		{
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("trim", "base.StringBuffer", false, "Remove leading and trailing blanks.", new List<ParameterResult>
		{
		}, new List<string> {}),
		new GeneroPackageClassMethod("trimLeft", "base.StringBuffer", false, "Removes leading blanks.", new List<ParameterResult>
		{
		}, new List<string> {}),
		new GeneroPackageClassMethod("trimRight", "base.StringBuffer", false, "Removes trailing blanks.", new List<ParameterResult>
		{
		}, new List<string> {})
	}),
	new GeneroPackageClass("StringTokenizer", "base", false, new List<GeneroPackageClassMethod>
	{
		new GeneroPackageClassMethod("create", "base.StringTokenizer", true, "Create a string tokenizer object.", new List<ParameterResult>
		{
			new ParameterResult("source", "", "string"),
			new ParameterResult("delims", "", "string")
		}, new List<string> {"base.StringTokenizer"}),
		new GeneroPackageClassMethod("createExt", "base.StringTokenizer", true, "Create a string tokenizer object with escape char and null handling.", new List<ParameterResult>
		{
			new ParameterResult("source", "", "string"),
			new ParameterResult("delims", "", "string"),
			new ParameterResult("escape", "", "string"),
			new ParameterResult("nulls", "", "boolean")
		}, new List<string> {"base.StringTokenizer"}),
		new GeneroPackageClassMethod("countTokens", "base.StringTokenizer", false, "Returns the number of tokens left to be returned.", new List<ParameterResult>
		{
		}, new List<string> {"integer"}),
		new GeneroPackageClassMethod("hasMoreTokens", "base.StringTokenizer", false, "Returns TRUE if there are more tokens to return.", new List<ParameterResult>
		{
		}, new List<string> {"boolean"}),
		new GeneroPackageClassMethod("nextToken", "base.StringTokenizer", false, "Returns the next token found in the source string.", new List<ParameterResult>
		{
		}, new List<string> {"string"})
	}),
	new GeneroPackageClass("TypeInfo", "base", true, new List<GeneroPackageClassMethod>
	{
		new GeneroPackageClassMethod("create", "base.TypeInfo", true, "Create a DomNode from a structured program variable.", new List<ParameterResult>
		{
			new ParameterResult("variable", "", "object")
		}, new List<string> {"om.DomNode"})
	}),
	new GeneroPackageClass("MessageServer", "base", true, new List<GeneroPackageClassMethod>
	{
		new GeneroPackageClassMethod("connect", "base.MessageServer", true, "Connects to the group of programs to be notified by a message.", new List<ParameterResult>
		{
		}, new List<string> {}),
		new GeneroPackageClassMethod("send", "base.MessageServer", true, "Sends a key event to the group of programs connected together.", new List<ParameterResult>
		{
			new ParameterResult("keyname", "", "string")
		}, new List<string> {})
	})
}));
                    Packages.Add("ui", new GeneroPackage("ui", false, new List<GeneroPackageClass>
{
	new GeneroPackageClass("Interface", "ui", true, new List<GeneroPackageClassMethod>
	{
		new GeneroPackageClassMethod("frontCall", "ui.Interface", true, "Performs function calls to the current front end.", new List<ParameterResult>
		{
			new ParameterResult("module", "", "string"),
			new ParameterResult("function", "", "string"),
			new ParameterResult("parameterList", "", "square-brace-list"),
			new ParameterResult("returningList", "", "square-brace-list")
		}, new List<string> {}),
		new GeneroPackageClassMethod("getDocument", "ui.Interface", true, "Returns the DOM document of the abstract user interface tree.", new List<ParameterResult>
		{
		}, new List<string> {"om.DomDocument"}),
		new GeneroPackageClassMethod("getFrontEndName", "ui.Interface", true, "Returns the type of the front-end currently in use.", new List<ParameterResult>
		{
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("getFrontEndVersion", "ui.Interface", true, "Returns the version of the front-end currently in use.", new List<ParameterResult>
		{
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("getRootNode", "ui.Interface", true, "Get the root DOM node of the abstract user interface.", new List<ParameterResult>
		{
		}, new List<string> {"om.DomNode"}),
		new GeneroPackageClassMethod("loadStartMenu", "ui.Interface", true, "Load the start menu file.", new List<ParameterResult>
		{
			new ParameterResult("filename", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("loadToolBar", "ui.Interface", true, "Load a default toolbar file.", new List<ParameterResult>
		{
			new ParameterResult("filename", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("loadActionDefaults", "ui.Interface", true, "Load the default action defaults file.", new List<ParameterResult>
		{
			new ParameterResult("filename", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("loadStyles", "ui.Interface", true, "Load the presentation styles file.", new List<ParameterResult>
		{
			new ParameterResult("filename", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("setName", "ui.Interface", true, "Define the name of the current program for the front-end.", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("getName", "ui.Interface", true, "Performs function calls to the current front end.", new List<ParameterResult>
		{
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("setText", "ui.Interface", true, "Defines the title for the program.", new List<ParameterResult>
		{
			new ParameterResult("title", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("getText", "ui.Interface", true, "Returns the title of the program.", new List<ParameterResult>
		{
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("setImage", "ui.Interface", true, "Defines the icon image of the program.", new List<ParameterResult>
		{
			new ParameterResult("icon", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("getImage", "ui.Interface", true, "Returns the icon image of the program.", new List<ParameterResult>
		{
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("setType", "ui.Interface", true, "Defines the type of the program for the front-end.", new List<ParameterResult>
		{
			new ParameterResult("type", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("getType", "ui.Interface", true, "Returns the type of the program.", new List<ParameterResult>
		{
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("setSize", "ui.Interface", true, "Specify the initial size of the parent container window.", new List<ParameterResult>
		{
			new ParameterResult("height", "", "integer"),
			new ParameterResult("width", "", "integer")
		}, new List<string> {}),
		new GeneroPackageClassMethod("setContainer", "ui.Interface", true, "Define the parent container for the current program.", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("getContainer", "ui.Interface", true, "Get the parent container of the current program.", new List<ParameterResult>
		{
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("getChildCount", "ui.Interface", true, "Get the number of children in a parent container.", new List<ParameterResult>
		{
		}, new List<string> {"integer"}),
		new GeneroPackageClassMethod("getChildInstances", "ui.Interface", true, "Get the number of child instances for a given application name.", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string")
		}, new List<string> {"integer"}),
		new GeneroPackageClassMethod("refresh", "ui.Interface", true, "Synchronize the user interface with the front-end.", new List<ParameterResult>
		{
		}, new List<string> {})
	}),
	new GeneroPackageClass("Window", "ui", false, new List<GeneroPackageClassMethod>
	{
		new GeneroPackageClassMethod("forName", "ui.Window", true, "Get a window object by name.", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string")
		}, new List<string> {"ui.Window"}),
		new GeneroPackageClassMethod("getCurrent", "ui.Window", true, "Get the current window object.", new List<ParameterResult>
		{
		}, new List<string> {"ui.Window"}),
		new GeneroPackageClassMethod("getForm", "ui.Window", false, "Get the current form of a window.", new List<ParameterResult>
		{
		}, new List<string> {"ui.Form"}),
		new GeneroPackageClassMethod("getNode", "ui.Window", false, "Get the DOM node of a window.", new List<ParameterResult>
		{
		}, new List<string> {"ui.DomNode"}),
		new GeneroPackageClassMethod("findNode", "ui.Window", false, "Search for a specific element in the window.", new List<ParameterResult>
		{
			new ParameterResult("type", "", "string"),
			new ParameterResult("name", "", "string")
		}, new List<string> {"ui.DomNode"}),
		new GeneroPackageClassMethod("createForm", "ui.Window", false, "Create a new empty form in a window.", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string")
		}, new List<string> {"ui.Form"}),
		new GeneroPackageClassMethod("setText", "ui.Window", false, "Set the window title.", new List<ParameterResult>
		{
			new ParameterResult("text", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("getText", "ui.Window", false, "Get the window title.", new List<ParameterResult>
		{
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("setImage", "ui.Window", false, "Set the window icon.", new List<ParameterResult>
		{
			new ParameterResult("image", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("getIcon", "ui.Window", false, "Get the window icon.", new List<ParameterResult>
		{
		}, new List<string> {"string"})
	}),
	new GeneroPackageClass("Form", "ui", false, new List<GeneroPackageClassMethod>
	{
		new GeneroPackageClassMethod("setDefaultInitializer", "ui.Form", true, "Define the default initializer for all forms.", new List<ParameterResult>
		{
			new ParameterResult("funcname", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("getNode", "ui.Form", false, "Get the DOM node of the form.", new List<ParameterResult>
		{
		}, new List<string> {"om.DomNode"}),
		new GeneroPackageClassMethod("loadActionDefaults", "ui.Form", false, "Load form action defaults.", new List<ParameterResult>
		{
			new ParameterResult("filename", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("loadToolBar", "ui.Form", false, "Load the form toolbar.", new List<ParameterResult>
		{
			new ParameterResult("filename", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("loadTopMenu", "ui.Form", false, "Load the form topmenu.", new List<ParameterResult>
		{
			new ParameterResult("filename", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("findNode", "ui.Form", false, "Search for a child node in the form.", new List<ParameterResult>
		{
			new ParameterResult("type", "", "string"),
			new ParameterResult("name", "", "string")
		}, new List<string> {"om.DomNode"}),
		new GeneroPackageClassMethod("setElementText", "ui.Form", false, "Change the text of form elements.", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string"),
			new ParameterResult("text", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("setElementImage", "ui.Form", false, "Change the image of form elements.", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string"),
			new ParameterResult("text", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("setElementStyle", "ui.Form", false, "Change the style of form elements.", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string"),
			new ParameterResult("style", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("setElementHidden", "ui.Form", false, "Show or hide form elements.", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string"),
			new ParameterResult("hide", "", "integer")
		}, new List<string> {}),
		new GeneroPackageClassMethod("setFieldHidden", "ui.Form", false, "Show or hide form field.", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string"),
			new ParameterResult("hide", "", "integer")
		}, new List<string> {}),
		new GeneroPackageClassMethod("setFieldStyle", "ui.Form", false, "Change the style of a form field.", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string"),
			new ParameterResult("style", "", "integer")
		}, new List<string> {}),
		new GeneroPackageClassMethod("ensureFieldVisible", "ui.Form", false, "Ensure visibility of a form field.", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("ensureElementVisible", "ui.Form", false, "Ensure the visibility of a form element.", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string")
		}, new List<string> {})
	}),
	new GeneroPackageClass("Dialog", "ui", false, new List<GeneroPackageClassMethod>
	{
		new GeneroPackageClassMethod("getCurrent", "ui.Dialog", true, "Returns the current dialog object.", new List<ParameterResult>
		{
		}, new List<string> {"ui.Dialog"}),
		new GeneroPackageClassMethod("setDefaultUnbuffered", "ui.Dialog", true, "Set the default unbuffered mode for all dialogs.", new List<ParameterResult>
		{
			new ParameterResult("value", "", "boolean")
		}, new List<string> {}),
		new GeneroPackageClassMethod("accept", "ui.Dialog", false, "Validates and terminates the dialog.", new List<ParameterResult>
		{
		}, new List<string> {}),
		new GeneroPackageClassMethod("validate", "ui.Dialog", false, "Check form level validation rules.", new List<ParameterResult>
		{
			new ParameterResult("field-list", "", "string")
		}, new List<string> {"integer"}),
		new GeneroPackageClassMethod("appendRow", "ui.Dialog", false, "Appends a new row in the specified list.", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("appendNode", "ui.Dialog", false, "Appends a new node in the specified tree-view.", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string"),
			new ParameterResult("index", "", "integer")
		}, new List<string> {}),
		new GeneroPackageClassMethod("deleteRow", "ui.Dialog", false, "Deletes a row from the specified list.", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string"),
			new ParameterResult("index", "", "integer")
		}, new List<string> {}),
		new GeneroPackageClassMethod("deleteNode", "ui.Dialog", false, "Deletes a node from the specified tree-view.", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string"),
			new ParameterResult("index", "", "integer")
		}, new List<string> {}),
		new GeneroPackageClassMethod("deleteAllRows", "ui.Dialog", false, "Deletes all rows from the specified list.", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("getArrayLength", "ui.Dialog", false, "Returns the total number of rows in the specified list.", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string")
		}, new List<string> {"integer"}),
		new GeneroPackageClassMethod("getCurrentItem", "ui.Dialog", false, "Returns the current item having focus.", new List<ParameterResult>
		{
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("getCurrentRow", "ui.Dialog", false, "Returns the current row of the specified list.", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string")
		}, new List<string> {"integer"}),
		new GeneroPackageClassMethod("getFieldBuffer", "ui.Dialog", false, "Returns the input buffer of the specified field.", new List<ParameterResult>
		{
			new ParameterResult("field", "", "string")
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("getFieldTouched", "ui.Dialog", false, "Returns the modification flag for a field.", new List<ParameterResult>
		{
			new ParameterResult("field-list", "", "string")
		}, new List<string> {"boolean"}),
		new GeneroPackageClassMethod("getForm", "ui.Dialog", false, "Returns the current form used by the dialog.", new List<ParameterResult>
		{
		}, new List<string> {"ui.Form"}),
		new GeneroPackageClassMethod("insertRow", "ui.Dialog", false, "Inserts a new row in the specified list.", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string"),
			new ParameterResult("index", "", "integer")
		}, new List<string> {}),
		new GeneroPackageClassMethod("insertNode", "ui.Dialog", false, "Inserts a new node in the specified tree.", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string"),
			new ParameterResult("index", "", "integer")
		}, new List<string> {}),
		new GeneroPackageClassMethod("isRowSelected", "ui.Dialog", false, "Queries row selection for a give list and row.", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string"),
			new ParameterResult("index", "", "integer")
		}, new List<string> {"boolean"}),
		new GeneroPackageClassMethod("nextField", "ui.Dialog", false, "Registering the next field to jump to.", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("selectionToString", "ui.Dialog", false, "Serializes data of the selected rows.", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string")
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("setArrayLength", "ui.Dialog", false, "Sets the total number of rows in the specified list.", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string"),
			new ParameterResult("len", "", "integer")
		}, new List<string> {}),
		new GeneroPackageClassMethod("setActionActive", "ui.Dialog", false, "Enabling and disabling dialog actions.", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string"),
			new ParameterResult("active", "", "boolean")
		}, new List<string> {}),
		new GeneroPackageClassMethod("setActionHidden", "ui.Dialog", false, "Handling default action view visibility.", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string"),
			new ParameterResult("hide", "", "integer")
		}, new List<string> {}),
		new GeneroPackageClassMethod("setCurrentRow", "ui.Dialog", false, "Sets the current row in the specified list.", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string"),
			new ParameterResult("row", "", "integer")
		}, new List<string> {}),
		new GeneroPackageClassMethod("setFieldActive", "ui.Dialog", false, "Enable and disable form fields.", new List<ParameterResult>
		{
			new ParameterResult("field-list", "", "string"),
			new ParameterResult("active", "", "boolean")
		}, new List<string> {}),
		new GeneroPackageClassMethod("setFieldTouched", "ui.Dialog", false, "Sets the modification flag of the specified field.", new List<ParameterResult>
		{
			new ParameterResult("field-list", "", "string"),
			new ParameterResult("touched", "", "boolean")
		}, new List<string> {}),
		new GeneroPackageClassMethod("setArrayAttributes", "ui.Dialog", false, "Define cell decoration attributes array for the specified list (singular or multiple dialogs).", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string"),
			new ParameterResult("array", "", "array")
		}, new List<string> {}),
		new GeneroPackageClassMethod("setCellAttributes", "ui.Dialog", false, "Define cell decoration attributes array for the specified list (singular dialog only).", new List<ParameterResult>
		{
			new ParameterResult("program-array", "", "array")
		}, new List<string> {}),
		new GeneroPackageClassMethod("setSelectionMode", "ui.Dialog", false, "Defines the row selection mode for the specified list.", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string"),
			new ParameterResult("mode", "", "integer")
		}, new List<string> {}),
		new GeneroPackageClassMethod("setSelectionRange", "ui.Dialog", false, "Sets the row selection flags for a range of rows.", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string"),
			new ParameterResult("start", "", "integer"),
			new ParameterResult("end", "", "integer"),
			new ParameterResult("value", "", "boolean")
		}, new List<string> {})
	}),
	new GeneroPackageClass("ComboBox", "ui", false, new List<GeneroPackageClassMethod>
	{
		new GeneroPackageClassMethod("setDefaultInitializer", "ui.ComboBox", true, "Define the default initializer for combobox form items.", new List<ParameterResult>
		{
			new ParameterResult("funcname", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("forName", "ui.ComboBox", true, "Search for a combobox in the current form.", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string")
		}, new List<string> {"ui.ComboBox"}),
		new GeneroPackageClassMethod("clear", "ui.ComboBox", false, "Clear the item list of a combobox.", new List<ParameterResult>
		{
		}, new List<string> {}),
		new GeneroPackageClassMethod("addItem", "ui.ComboBox", false, "Add an element to the item list.", new List<ParameterResult>
		{
			new ParameterResult("value", "", "string"),
			new ParameterResult("label", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("getColumnName", "ui.ComboBox", false, "Get the column name of the form field.", new List<ParameterResult>
		{
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("getTableName", "ui.ComboBox", false, "Get the table prefix of the form field.", new List<ParameterResult>
		{
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("getIndexOf", "ui.ComboBox", false, "Get an item position by name.", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string")
		}, new List<string> {"integer"}),
		new GeneroPackageClassMethod("getItemCount", "ui.ComboBox", false, "Get the number of items.", new List<ParameterResult>
		{
		}, new List<string> {"integer"}),
		new GeneroPackageClassMethod("getItemName", "ui.ComboBox", false, "Get an item name by position.", new List<ParameterResult>
		{
			new ParameterResult("position", "", "integer")
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("getItemText", "ui.ComboBox", false, "Get an item text by position.", new List<ParameterResult>
		{
			new ParameterResult("position", "", "integer")
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("getTag", "ui.ComboBox", false, "Get the combobox tag value.", new List<ParameterResult>
		{
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("getTextOf", "ui.ComboBox", false, "Get the item text by name.", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string")
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("removeItem", "ui.ComboBox", false, "Remove an item by name.", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string")
		}, new List<string> {})
	}),
	new GeneroPackageClass("DragDrop", "ui", false, new List<GeneroPackageClassMethod>
	{
		new GeneroPackageClassMethod("addPossibleOperation", "ui.DragDrop", false, "Add a possible operation.", new List<ParameterResult>
		{
			new ParameterResult("oper", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("getLocationRow", "ui.DragDrop", false, "Get the index of the target row where the object was dropped.", new List<ParameterResult>
		{
		}, new List<string> {"integer"}),
		new GeneroPackageClassMethod("getLocationParent", "ui.DragDrop", false, "Get the index of the parent node where the object was dropped.", new List<ParameterResult>
		{
		}, new List<string> {"integer"}),
		new GeneroPackageClassMethod("getOperation", "ui.DragDrop", false, "Identify the type of operation on drop.", new List<ParameterResult>
		{
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("setOperation", "ui.DragDrop", false, "Define the type of Drag & Drop operation.", new List<ParameterResult>
		{
			new ParameterResult("oper", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("setFeedback", "ui.DragDrop", false, "Define the appearance of the target during Drag & Drop.", new List<ParameterResult>
		{
			new ParameterResult("type", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("selectMimeType", "ui.DragDrop", false, "Select the MIME type before getting the data.", new List<ParameterResult>
		{
			new ParameterResult("type", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("getSelectedMimeType", "ui.DragDrop", false, "Get the previously selected MIME type.", new List<ParameterResult>
		{
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("getBuffer", "ui.DragDrop", false, "Get drag & drop data from the buffer.", new List<ParameterResult>
		{
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("setMimeType", "ui.DragDrop", false, "Define the MIME type of the dragged object.", new List<ParameterResult>
		{
			new ParameterResult("type", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("setBuffer", "ui.DragDrop", false, "Set the text data of the dragged object.", new List<ParameterResult>
		{
			new ParameterResult("data", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("dropInternal", "ui.DragDrop", false, "Perform built-in row drop in trees.", new List<ParameterResult>
		{
		}, new List<string> {})
	})
}));
                    Packages.Add("om", new GeneroPackage("om", false, new List<GeneroPackageClass>
{
	new GeneroPackageClass("DomDocument", "om", false, new List<GeneroPackageClassMethod>
	{
		new GeneroPackageClassMethod("create", "om.DomDocument", true, "Create a new empty om.DomDocument object.", new List<ParameterResult>
		{
			new ParameterResult("tag", "", "string")
		}, new List<string> {"om.DomDocument"}),
		new GeneroPackageClassMethod("createFromString", "om.DomDocument", true, "Create a new om.DomDocument object from an XML string.", new List<ParameterResult>
		{
			new ParameterResult("string", "", "string")
		}, new List<string> {"om.DomDocument"}),
		new GeneroPackageClassMethod("createFromXmlFile", "om.DomDocument", true, "Create a new om.DomDocument object from an XML file.", new List<ParameterResult>
		{
			new ParameterResult("filename", "", "string")
		}, new List<string> {"om.DomDocument"}),
		new GeneroPackageClassMethod("createChars", "om.DomDocument", false, "Create a new text node in the DOM document.", new List<ParameterResult>
		{
			new ParameterResult("string", "", "string")
		}, new List<string> {"om.DomNode"}),
		new GeneroPackageClassMethod("createElement", "om.DomDocument", false, "Create a new element node in the DOM document.", new List<ParameterResult>
		{
			new ParameterResult("tag", "", "string")
		}, new List<string> {"om.DomNode"}),
		new GeneroPackageClassMethod("createEntity", "om.DomDocument", false, "Create a new entity node in the DOM document.", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string")
		}, new List<string> {"om.DomNode"}),
		new GeneroPackageClassMethod("copy", "om.DomDocument", false, "Create a new element node by copying an existing node.", new List<ParameterResult>
		{
			new ParameterResult("source", "", "om.DomNode"),
			new ParameterResult("deep", "", "integer")
		}, new List<string> {"om.DomNode"}),
		new GeneroPackageClassMethod("getDocumentElement", "om.DomDocument", false, "Returns the root node element of the DOM document.", new List<ParameterResult>
		{
		}, new List<string> {"om.DomNode"}),
		new GeneroPackageClassMethod("getDocumentById", "om.DomDocument", false, "Returns a node element according to the internal AUI tree id.", new List<ParameterResult>
		{
			new ParameterResult("id", "", "integer")
		}, new List<string> {"om.DomNode"}),
		new GeneroPackageClassMethod("removeElement", "om.DomDocument", false, "Remove a DomNode object and all its descendants.", new List<ParameterResult>
		{
			new ParameterResult("element", "", "om.DomNode")
		}, new List<string> {})
	}),
	new GeneroPackageClass("DomNode", "om", false, new List<GeneroPackageClassMethod>
	{
		new GeneroPackageClassMethod("appendChild", "om.DomNode", false, "Adds an existing node at the end of the list of children in the current node.", new List<ParameterResult>
		{
			new ParameterResult("node", "", "om.DomNode")
		}, new List<string> {}),
		new GeneroPackageClassMethod("createChild", "om.DomNode", false, "Creates and adds an node at the end of the list of children in the current node.", new List<ParameterResult>
		{
			new ParameterResult("tag", "", "string")
		}, new List<string> {"om.DomNode"}),
		new GeneroPackageClassMethod("insertBefore", "om.DomNode", false, "Inserts an existing node before the existing node specified.", new List<ParameterResult>
		{
			new ParameterResult("new", "", "om.DomNode"),
			new ParameterResult("existing", "", "om.DomNode")
		}, new List<string> {}),
		new GeneroPackageClassMethod("removeChild", "om.DomNode", false, "Deletes the specified child node from the current node.", new List<ParameterResult>
		{
			new ParameterResult("node", "", "om.DomNode")
		}, new List<string> {}),
		new GeneroPackageClassMethod("replaceChild", "om.DomNode", false, "Replaces a node by another in the children nodes of the current node.", new List<ParameterResult>
		{
			new ParameterResult("new", "", "om.DomNode"),
			new ParameterResult("old", "", "om.DomNode")
		}, new List<string> {}),
		new GeneroPackageClassMethod("loadXml", "om.DomNode", false, "Load an XML file into the current node.", new List<ParameterResult>
		{
			new ParameterResult("filename", "", "string")
		}, new List<string> {"om.DomNode"}),
		new GeneroPackageClassMethod("parse", "om.DomNode", false, "Parses an XML formatted string and creates the DOM structure in the current node.", new List<ParameterResult>
		{
			new ParameterResult("string", "", "string")
		}, new List<string> {"om.DomNode"}),
		new GeneroPackageClassMethod("toString", "om.DomNode", false, "Serializes the current node into an XML formatted string.", new List<ParameterResult>
		{
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("writeXml", "om.DomNode", false, "Creates an XML file from the current DOM node.", new List<ParameterResult>
		{
			new ParameterResult("filename", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("write", "om.DomNode", false, "Processes a DOM document with a SAX document handler.", new List<ParameterResult>
		{
			new ParameterResult("sdh", "", "om.SaxDocumentHandler")
		}, new List<string> {}),
		new GeneroPackageClassMethod("getId", "om.DomNode", false, "Returns the internal AUI tree id of a DOM node.", new List<ParameterResult>
		{
		}, new List<string> {"integer"}),
		new GeneroPackageClassMethod("getTagName", "om.DomNode", false, "Returns the XML tag name of a DOM node.", new List<ParameterResult>
		{
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("setAttribute", "om.DomNode", false, "Sets the value of a DOM node attribute.", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string"),
			new ParameterResult("value", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("getAttribute", "om.DomNode", false, "Returns the value of a DOM node attribute.", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string")
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("getAttributeInteger", "om.DomNode", false, "Returns the value of a DOM node attribute, with default integer value.", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string"),
			new ParameterResult("def", "", "string")
		}, new List<string> {"integer"}),
		new GeneroPackageClassMethod("getAttributeString", "om.DomNode", false, "Returns the value of a DOM node attribute, with default string value.", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string"),
			new ParameterResult("def", "", "string")
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("getAttributesCount", "om.DomNode", false, "Returns the number of attributes in the DOM node.", new List<ParameterResult>
		{
		}, new List<string> {"integer"}),
		new GeneroPackageClassMethod("getAttributeName", "om.DomNode", false, "Returns the name of a DOM node attribute by position.", new List<ParameterResult>
		{
			new ParameterResult("index", "", "integer")
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("getAttributeValue", "om.DomNode", false, "Returns the value of a DOM node attribute by position.", new List<ParameterResult>
		{
			new ParameterResult("index", "", "integer")
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("removeAttribute", "om.DomNode", false, "Delete the specified attribute from the DOM node.", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string")
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("getChildCount", "om.DomNode", false, "Returns the number of children nodes.", new List<ParameterResult>
		{
		}, new List<string> {"integer"}),
		new GeneroPackageClassMethod("getChildByIndex", "om.DomNode", false, "Returns a child DOM node by position.", new List<ParameterResult>
		{
			new ParameterResult("index", "", "integer")
		}, new List<string> {"om.DomNode"}),
		new GeneroPackageClassMethod("getFirstChild", "om.DomNode", false, "Returns the first child DOM node.", new List<ParameterResult>
		{
		}, new List<string> {"om.DomNode"}),
		new GeneroPackageClassMethod("getLastChild", "om.DomNode", false, "Returns the last child DOM node.", new List<ParameterResult>
		{
		}, new List<string> {"om.DomNode"}),
		new GeneroPackageClassMethod("getNext", "om.DomNode", false, "Returns the next sibling DOM node of this node.", new List<ParameterResult>
		{
		}, new List<string> {"om.DomNode"}),
		new GeneroPackageClassMethod("getParent", "om.DomNode", false, "Returns the parent DOM node.", new List<ParameterResult>
		{
		}, new List<string> {"om.DomNode"}),
		new GeneroPackageClassMethod("getPrevious", "om.DomNode", false, "Returns previous sibling DOM node of this node.", new List<ParameterResult>
		{
		}, new List<string> {"om.DomNode"}),
		new GeneroPackageClassMethod("selectByTagName", "om.DomNode", false, "Finds descendant DOM nodes according to a tag name.", new List<ParameterResult>
		{
			new ParameterResult("tagname", "", "string")
		}, new List<string> {"om.NodeList"}),
		new GeneroPackageClassMethod("selectByPath", "om.DomNode", false, "Finds descendant DOM nodes according to an XPath-like pattern.", new List<ParameterResult>
		{
			new ParameterResult("xpath", "", "string")
		}, new List<string> {"om.NodeList"})
	}),
	new GeneroPackageClass("NodeList", "om", false, new List<GeneroPackageClassMethod>
	{
		new GeneroPackageClassMethod("item", "om.NodeList", false, "Returns a DOM node element by position in the node list.", new List<ParameterResult>
		{
			new ParameterResult("index", "", "om.DomNode")
		}, new List<string> {"om.DomNode"}),
		new GeneroPackageClassMethod("getLength", "om.NodeList", false, "Returns the number of elements in the node list.", new List<ParameterResult>
		{
		}, new List<string> {"integer"})
	}),
	new GeneroPackageClass("SaxAttributes", "om", false, new List<GeneroPackageClassMethod>
	{
		new GeneroPackageClassMethod("copy", "om.SaxAttributes", true, "Clones an existing SAX attributes object.", new List<ParameterResult>
		{
			new ParameterResult("attrs", "", "om.SaxAttributes")
		}, new List<string> {"om.SaxAttributes"}),
		new GeneroPackageClassMethod("create", "om.SaxAttributes", true, "Create a new SAX attributes object.", new List<ParameterResult>
		{
		}, new List<string> {"om.SaxAttributes"}),
		new GeneroPackageClassMethod("addAttribute", "om.SaxAttributes", false, "Appends a new attribute to the end of the list.", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string"),
			new ParameterResult("value", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("clear", "om.SaxAttributes", false, "Clears the SAX attribute list.", new List<ParameterResult>
		{
		}, new List<string> {}),
		new GeneroPackageClassMethod("getLength", "om.SaxAttributes", false, "Returns the number of attributes in the list.", new List<ParameterResult>
		{
		}, new List<string> {"integer"}),
		new GeneroPackageClassMethod("getName", "om.SaxAttributes", false, "Returns the name of an attribute by position.", new List<ParameterResult>
		{
			new ParameterResult("index", "", "integer")
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("getValue", "om.SaxAttributes", false, "Returns the value of an attribute by name.", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string")
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("getValueByIndex", "om.SaxAttributes", false, "Returns an attribute value by position.", new List<ParameterResult>
		{
			new ParameterResult("index", "", "integer")
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("removeAttribute", "om.SaxAttributes", false, "Delete an attribute by position.", new List<ParameterResult>
		{
			new ParameterResult("index", "", "integer")
		}, new List<string> {}),
		new GeneroPackageClassMethod("setAttributes", "om.SaxAttributes", false, "Clears the list and copies the attributes passed.", new List<ParameterResult>
		{
			new ParameterResult("attrs", "", "om.SaxAttributes")
		}, new List<string> {})
	}),
	new GeneroPackageClass("SaxDocumentHandler", "om", false, new List<GeneroPackageClassMethod>
	{
		new GeneroPackageClassMethod("createForName", "om.SaxDocumentHandler", true, "Creates a new SAX document handler object for the given 4gl module.", new List<ParameterResult>
		{
			new ParameterResult("module", "", "string")
		}, new List<string> {"om.SaxDocumentHandler"}),
		new GeneroPackageClassMethod("readXmlFile", "om.SaxDocumentHandler", false, "Reads and processes an XML file with the SAX document handler.", new List<ParameterResult>
		{
			new ParameterResult("filename", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("setIndent", "om.SaxDocumentHandler", false, "Controls indentation in XML output.", new List<ParameterResult>
		{
			new ParameterResult("on", "", "boolean")
		}, new List<string> {}),
		new GeneroPackageClassMethod("startDocument", "om.SaxDocumentHandler", false, "Processes the beginning of the document.", new List<ParameterResult>
		{
		}, new List<string> {}),
		new GeneroPackageClassMethod("startElement", "om.SaxDocumentHandler", false, "Processes the beginning of an element.", new List<ParameterResult>
		{
			new ParameterResult("tagname", "", "string"),
			new ParameterResult("attrs", "", "om.SaxAttributes")
		}, new List<string> {}),
		new GeneroPackageClassMethod("endElement", "om.SaxDocumentHandler", false, "Processes the end of an element.", new List<ParameterResult>
		{
			new ParameterResult("tagname", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("endDocument", "om.SaxDocumentHandler", false, "Processes the end of the document.", new List<ParameterResult>
		{
		}, new List<string> {}),
		new GeneroPackageClassMethod("characters", "om.SaxDocumentHandler", false, "Processes a text node.", new List<ParameterResult>
		{
			new ParameterResult("data", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("skippedEntity", "om.SaxDocumentHandler", false, "Processes an unresolved entity.", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("processingInstruction", "om.SaxDocumentHandler", false, "Processes a processing instruction.", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string"),
			new ParameterResult("data", "", "string")
		}, new List<string> {})
	}),
	new GeneroPackageClass("XmlReader", "om", false, new List<GeneroPackageClassMethod>
	{
		new GeneroPackageClassMethod("createFileReader", "om.XmlReader", true, "Creates an XML reader object from a file.", new List<ParameterResult>
		{
			new ParameterResult("filename", "", "string")
		}, new List<string> {"om.XmlReader"}),
		new GeneroPackageClassMethod("getAttributes", "om.XmlReader", false, "Builds an attribute list for the current processed element.", new List<ParameterResult>
		{
		}, new List<string> {"om.SaxAttributes"}),
		new GeneroPackageClassMethod("getCharacters", "om.XmlReader", false, "Returns the character data of the current processed element.", new List<ParameterResult>
		{
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("skippedEntity", "om.XmlReader", false, "Returns the name of an unresolved entity.", new List<ParameterResult>
		{
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("getTagName", "om.XmlReader", false, "Returns the tag name of the current processed element.", new List<ParameterResult>
		{
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("read", "om.XmlReader", false, "Reads the next SAX event to process.", new List<ParameterResult>
		{
		}, new List<string> {"string"})
	}),
	new GeneroPackageClass("XmlWriter", "om", true, new List<GeneroPackageClassMethod>
	{
		new GeneroPackageClassMethod("createFileWriter", "om.XmlWriter", true, "Creates an om.SaxDocumentHandler object writing to a file.", new List<ParameterResult>
		{
			new ParameterResult("filename", "", "string")
		}, new List<string> {"om.SaxDocumentHandler"}),
		new GeneroPackageClassMethod("createPipeWriter", "om.XmlWriter", true, "Creates an om.SaxDocumentHandler object writing to a pipe created for a process.", new List<ParameterResult>
		{
			new ParameterResult("command", "", "string")
		}, new List<string> {"om.SaxDocumentHandler"}),
		new GeneroPackageClassMethod("createSocketWriter", "om.XmlWriter", true, "Creates an om.SaxDocumentHandler object writing to a socket.", new List<ParameterResult>
		{
			new ParameterResult("host", "", "string"),
			new ParameterResult("port", "", "integer")
		}, new List<string> {"om.SaxDocumentHandler"}),
		new GeneroPackageClassMethod("createChannelWriter", "om.XmlWriter", true, "Creates an om.SaxDocumentHandler object writing to a channel object.", new List<ParameterResult>
		{
			new ParameterResult("channel", "", "base.Channel")
		}, new List<string> {"om.SaxDocumentHandler"})
	})
}));
                    Packages.Add("util", new GeneroPackage("util", true, new List<GeneroPackageClass>
{
	new GeneroPackageClass("DateTime", "util", true, new List<GeneroPackageClassMethod>
	{
		new GeneroPackageClassMethod("toLocalTime", "util.DateTime", true, "Converts a UTC datetime to the local time.", new List<ParameterResult>
		{
			new ParameterResult("utc_datetime", "", "datetime q1 to q2")
		}, new List<string> {"datetime q1 to q2"}),
		new GeneroPackageClassMethod("toUTC", "util.DateTime", true, "Converts a datetime value to the UTC datetime.", new List<ParameterResult>
		{
			new ParameterResult("local_datetime", "", "datetime q1 to q2")
		}, new List<string> {"datetime q1 to q2"})
	}),
	new GeneroPackageClass("Math", "util", true, new List<GeneroPackageClassMethod>
	{
		new GeneroPackageClassMethod("sqrt", "util.Math", true, "Returns the square root of the argument provided.", new List<ParameterResult>
		{
			new ParameterResult("val", "", "float")
		}, new List<string> {"float"}),
		new GeneroPackageClassMethod("pow", "util.Math", true, "Computes the value of x raised to the power y.", new List<ParameterResult>
		{
			new ParameterResult("x", "", "float"),
			new ParameterResult("y", "", "float")
		}, new List<string> {"float"}),
		new GeneroPackageClassMethod("exp", "util.Math", true, "Computes the base-e exponential of the value passed as parameter.", new List<ParameterResult>
		{
			new ParameterResult("val", "", "float")
		}, new List<string> {"float"}),
		new GeneroPackageClassMethod("srand", "util.Math", true, "Initializes the pseudo-random numbers generator.", new List<ParameterResult>
		{
		}, new List<string> {}),
		new GeneroPackageClassMethod("rand", "util.Math", true, "Returns a positive pseudo-random number.", new List<ParameterResult>
		{
			new ParameterResult("max", "", "integer")
		}, new List<string> {"integer"}),
		new GeneroPackageClassMethod("sin", "util.Math", true, "Computes the sine of the passed value, measured in radians.", new List<ParameterResult>
		{
			new ParameterResult("val", "", "float")
		}, new List<string> {"float"}),
		new GeneroPackageClassMethod("cos", "util.Math", true, "Computes the cosine of the passed value, measured in radians.", new List<ParameterResult>
		{
			new ParameterResult("val", "", "float")
		}, new List<string> {"float"}),
		new GeneroPackageClassMethod("tan", "util.Math", true, "Computes the tangent of the passed value, measured in radians.", new List<ParameterResult>
		{
			new ParameterResult("val", "", "float")
		}, new List<string> {"float"}),
		new GeneroPackageClassMethod("asin", "util.Math", true, "Computes the arc sine of the passed value, measured in radians.", new List<ParameterResult>
		{
			new ParameterResult("val", "", "float")
		}, new List<string> {"float"}),
		new GeneroPackageClassMethod("acos", "util.Math", true, "Computes the arc cosine of the passed value, measured in radians.", new List<ParameterResult>
		{
			new ParameterResult("val", "", "float")
		}, new List<string> {"float"}),
		new GeneroPackageClassMethod("atan", "util.Math", true, "Computes the arc tangent of the passed value, measured in radians.", new List<ParameterResult>
		{
			new ParameterResult("val", "", "float")
		}, new List<string> {"float"}),
		new GeneroPackageClassMethod("log", "util.Math", true, "Computes the natural logarithm of the passed value.", new List<ParameterResult>
		{
			new ParameterResult("val", "", "float")
		}, new List<string> {"float"}),
		new GeneroPackageClassMethod("toDegrees", "util.Math", true, "Converts an angle measured in radians to an approximately equivalent angle measured in degrees.", new List<ParameterResult>
		{
			new ParameterResult("val", "", "float")
		}, new List<string> {"float"}),
		new GeneroPackageClassMethod("toRadians", "util.Math", true, "Converts an angle measured in degrees to an approximately equivalent angle measured in radians.", new List<ParameterResult>
		{
			new ParameterResult("val", "", "float")
		}, new List<string> {"float"}),
		new GeneroPackageClassMethod("pi", "util.Math", true, "Returns the FLOAT value of PI.", new List<ParameterResult>
		{
		}, new List<string> {"float"})
	}),
	new GeneroPackageClass("JSON", "util", true, new List<GeneroPackageClassMethod>
	{
		new GeneroPackageClassMethod("parse", "util.JSON", true, "Parses a JSON string and fills program variables with the values.", new List<ParameterResult>
		{
			new ParameterResult("source", "", "string"),
			new ParameterResult("destination", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("format", "util.JSON", true, "Formats JSON string with indentation.", new List<ParameterResult>
		{
			new ParameterResult("source", "", "string")
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("stringify", "util.JSON", true, "Transforms a record variable to a flat JSON formatted string.", new List<ParameterResult>
		{
			new ParameterResult("source", "", "record")
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("proposeType", "util.JSON", true, "Describes the record structure that can hold a given JSON data string.", new List<ParameterResult>
		{
			new ParameterResult("source", "", "string")
		}, new List<string> {"string"})
	}),
	new GeneroPackageClass("JSONObject", "util", false, new List<GeneroPackageClassMethod>
	{
		new GeneroPackageClassMethod("create", "util.JSONObject", true, "Creates a new JSON object.", new List<ParameterResult>
		{
		}, new List<string> {"util.JSONObject"}),
		new GeneroPackageClassMethod("fromFGL", "util.JSONObject", true, "Creates a new JSON object from a RECORD.", new List<ParameterResult>
		{
			new ParameterResult("source", "", "record")
		}, new List<string> {"util.JSONObject"}),
		new GeneroPackageClassMethod("parse", "util.JSONObject", true, "Parses a JSON string and creates a JSON object from it.", new List<ParameterResult>
		{
			new ParameterResult("source", "", "string")
		}, new List<string> {"util.JSONObject"}),
		new GeneroPackageClassMethod("get", "util.JSONObject", false, "Returns the value corresponding to the specified entry name.", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string")
		}, new List<string> {"result-type"}),
		new GeneroPackageClassMethod("getType", "util.JSONObject", false, "Returns the type of a JSON object element.", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string")
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("has", "util.JSONObject", false, "Checks if the JSON object contains a specific entry name.", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string")
		}, new List<string> {"boolean"}),
		new GeneroPackageClassMethod("getLength", "util.JSONObject", false, "Returns the number of name-value pairs in the JSON object.", new List<ParameterResult>
		{
		}, new List<string> {"integer"}),
		new GeneroPackageClassMethod("name", "util.JSONObject", false, "Returns the name of a JSON object entry by position.", new List<ParameterResult>
		{
			new ParameterResult("index", "", "integer")
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("put", "util.JSONObject", false, "Sets a name-value pair in the JSON object.", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string"),
			new ParameterResult("value", "", "value-type")
		}, new List<string> {}),
		new GeneroPackageClassMethod("remove", "util.JSONObject", false, "Removes the specified element in the JSON object.", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("toString", "util.JSONObject", false, "Builds a JSON string from the values contained in the JSON object.", new List<ParameterResult>
		{
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("toFGL", "util.JSONObject", false, "Fills a record variable with the entries contained in the JSON object.", new List<ParameterResult>
		{
			new ParameterResult("dest", "", "record")
		}, new List<string> {})
	}),
	new GeneroPackageClass("JSONArray", "util", false, new List<GeneroPackageClassMethod>
	{
		new GeneroPackageClassMethod("create", "util.JSONArray", true, "Creates a new JSON array object.", new List<ParameterResult>
		{
		}, new List<string> {"util.JSONArray"}),
		new GeneroPackageClassMethod("fromFGL", "util.JSONArray", true, "Creates a new JSON array object from a DYNAMIC ARRAY.", new List<ParameterResult>
		{
			new ParameterResult("source", "", "dynamic array")
		}, new List<string> {"util.JSONArray"}),
		new GeneroPackageClassMethod("parse", "util.JSONArray", true, "Parses a JSON string and creates a JSON array object from it.", new List<ParameterResult>
		{
			new ParameterResult("source", "", "string")
		}, new List<string> {"util.JSONArray"}),
		new GeneroPackageClassMethod("get", "util.JSONArray", false, "Returns the value of a JSON array element.", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string")
		}, new List<string> {"result-type"}),
		new GeneroPackageClassMethod("getType", "util.JSONArray", false, "Returns the type of a JSON array element.", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string")
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("getLength", "util.JSONArray", false, "Returns the number of elements in the JSON array object.", new List<ParameterResult>
		{
		}, new List<string> {"integer"}),
		new GeneroPackageClassMethod("put", "util.JSONArray", false, "Sets an element by position in the JSON array object.", new List<ParameterResult>
		{
			new ParameterResult("index", "", "integer"),
			new ParameterResult("value", "", "value-type")
		}, new List<string> {}),
		new GeneroPackageClassMethod("remove", "util.JSONArray", false, "Removes the specified entry in the JSON array object.", new List<ParameterResult>
		{
			new ParameterResult("index", "", "integer")
		}, new List<string> {}),
		new GeneroPackageClassMethod("toString", "util.JSONArray", false, "Builds a JSON string from the elements contained in the JSON array object.", new List<ParameterResult>
		{
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("toFGL", "util.JSONArray", false, "Fills a dynamic array variable with the elements contained in the JSON array object.", new List<ParameterResult>
		{
			new ParameterResult("dest", "", "dynamic record")
		}, new List<string> {})
	})
}));
                    Packages.Add("os", new GeneroPackage("os", true, new List<GeneroPackageClass>
{
	new GeneroPackageClass("Path", "os", true, new List<GeneroPackageClassMethod>
	{
		new GeneroPackageClassMethod("separator", "os.Path", true, "Returns the character used to separate path segments.", new List<ParameterResult>
		{
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("pathseparator", "os.Path", true, "Returns the character used in environment variables to separate path elements.", new List<ParameterResult>
		{
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("basename", "os.Path", true, "Returns the last element of a path.", new List<ParameterResult>
		{
			new ParameterResult("filename", "", "string")
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("dirname", "os.Path", true, "Returns all components of a path excluding the last one.", new List<ParameterResult>
		{
			new ParameterResult("filename", "", "string")
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("rootname", "os.Path", true, "Returns the file path without the file extension of the last element of the file path.", new List<ParameterResult>
		{
			new ParameterResult("filename", "", "string")
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("join", "os.Path", true, "Joins two path segments adding the platform-dependent separator.", new List<ParameterResult>
		{
			new ParameterResult("begin", "", "string"),
			new ParameterResult("end", "", "string")
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("pathtype", "os.Path", true, "Checks if a path is a relative path or an absolute path.", new List<ParameterResult>
		{
			new ParameterResult("path", "", "string")
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("fullpath", "os.Path", true, "Returns the canonical equivalent of a path.", new List<ParameterResult>
		{
			new ParameterResult("path", "", "string")
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("exists", "os.Path", true, "Checks if a file exists.", new List<ParameterResult>
		{
			new ParameterResult("fname", "", "string")
		}, new List<string> {"integer"}),
		new GeneroPackageClassMethod("extension", "os.Path", true, "Returns the file extension.", new List<ParameterResult>
		{
			new ParameterResult("fname", "", "string")
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("readable", "os.Path", true, "Checks if a file is readable.", new List<ParameterResult>
		{
			new ParameterResult("fname", "", "string")
		}, new List<string> {"integer"}),
		new GeneroPackageClassMethod("writable", "os.Path", true, "Checks if a file is writable.", new List<ParameterResult>
		{
			new ParameterResult("fname", "", "string")
		}, new List<string> {"integer"}),
		new GeneroPackageClassMethod("executable", "os.Path", true, "Checks if a file is executable.", new List<ParameterResult>
		{
			new ParameterResult("fname", "", "string")
		}, new List<string> {"integer"}),
		new GeneroPackageClassMethod("isfile", "os.Path", true, "Checks if a file is a regular file.", new List<ParameterResult>
		{
			new ParameterResult("fname", "", "string")
		}, new List<string> {"boolean"}),
		new GeneroPackageClassMethod("isdirectory", "os.Path", true, "Checks if a file is hidden.", new List<ParameterResult>
		{
			new ParameterResult("fname", "", "string")
		}, new List<string> {"boolean"}),
		new GeneroPackageClassMethod("islink", "os.Path", true, "Checks if a file is UNIX symbolic link.", new List<ParameterResult>
		{
			new ParameterResult("fname", "", "string")
		}, new List<string> {"boolean"}),
		new GeneroPackageClassMethod("isroot", "os.Path", true, "Checks if a file path is a root path.", new List<ParameterResult>
		{
			new ParameterResult("fname", "", "string")
		}, new List<string> {"boolean"}),
		new GeneroPackageClassMethod("type", "os.Path", true, "Returns the file type as a string.", new List<ParameterResult>
		{
			new ParameterResult("fname", "", "string")
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("size", "os.Path", true, "Returns the size of a file.", new List<ParameterResult>
		{
			new ParameterResult("fname", "", "string")
		}, new List<string> {"integer"}),
		new GeneroPackageClassMethod("atime", "os.Path", true, "Returns the time of the last file access.", new List<ParameterResult>
		{
			new ParameterResult("fname", "", "string")
		}, new List<string> {"integer"}),
		new GeneroPackageClassMethod("chown", "os.Path", true, "Changes the UNIX owner and group of a file.", new List<ParameterResult>
		{
			new ParameterResult("fname", "", "string"),
			new ParameterResult("uid", "", "integer"),
			new ParameterResult("gui", "", "integer")
		}, new List<string> {"integer"}),
		new GeneroPackageClassMethod("uid", "os.Path", true, "Returns the UNIX user id of a file.", new List<ParameterResult>
		{
			new ParameterResult("fname", "", "string")
		}, new List<string> {"integer"}),
		new GeneroPackageClassMethod("gid", "os.Path", true, "Returns the UNIX group id of a file.", new List<ParameterResult>
		{
			new ParameterResult("fname", "", "string")
		}, new List<string> {"integer"}),
		new GeneroPackageClassMethod("rwx", "os.Path", true, "Returns the UNIX file permissions of a file.", new List<ParameterResult>
		{
			new ParameterResult("fname", "", "string")
		}, new List<string> {"integer"}),
		new GeneroPackageClassMethod("chrwx", "os.Path", true, "Changes the UNIX permissions of a file.", new List<ParameterResult>
		{
			new ParameterResult("fname", "", "string"),
			new ParameterResult("mode", "", "integer")
		}, new List<string> {"integer"}),
		new GeneroPackageClassMethod("mtime", "os.Path", true, "Returns the time of the last file modification.", new List<ParameterResult>
		{
			new ParameterResult("fname", "", "string")
		}, new List<string> {"integer"}),
		new GeneroPackageClassMethod("homedir", "os.Path", true, "Returns the path to the HOME directory of the current user.", new List<ParameterResult>
		{
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("rootdir", "os.Path", true, "Returns the root directory of the current working path.", new List<ParameterResult>
		{
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("dirfmask", "os.Path", true, "Defines a filter mask for os.Path.diropen().", new List<ParameterResult>
		{
			new ParameterResult("mask", "", "integer")
		}, new List<string> {}),
		new GeneroPackageClassMethod("dirsort", "os.Path", true, "Defines the sort criteria and sort order for os.Path.diropen().", new List<ParameterResult>
		{
			new ParameterResult("criteria", "", "string"),
			new ParameterResult("order", "", "integer")
		}, new List<string> {}),
		new GeneroPackageClassMethod("diropen", "os.Path", true, "Opens a directory and returns an integer handle to this directory.", new List<ParameterResult>
		{
			new ParameterResult("dname", "", "string")
		}, new List<string> {"integer"}),
		new GeneroPackageClassMethod("dirclose", "os.Path", true, "Closes the directory referenced by the directory opened by os.Path.diropen().", new List<ParameterResult>
		{
			new ParameterResult("dirhandle", "", "integer")
		}, new List<string> {}),
		new GeneroPackageClassMethod("dirnext", "os.Path", true, "Reads the next entry in the directory opened with os.Path.diropen().", new List<ParameterResult>
		{
			new ParameterResult("dirhandle", "", "integer")
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("pwd", "os.Path", true, "Returns the current working directory.", new List<ParameterResult>
		{
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("chdir", "os.Path", true, "Changes the current working directory.", new List<ParameterResult>
		{
			new ParameterResult("newdir", "", "string")
		}, new List<string> {"integer"}),
		new GeneroPackageClassMethod("volumes", "os.Path", true, "Returns the available volumes.", new List<ParameterResult>
		{
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("chvolumes", "os.Path", true, "Changes the current working volume.", new List<ParameterResult>
		{
			new ParameterResult("new", "", "string")
		}, new List<string> {"integer"}),
		new GeneroPackageClassMethod("mkdir", "os.Path", true, "Creates a new directory.", new List<ParameterResult>
		{
			new ParameterResult("dname", "", "string")
		}, new List<string> {"integer"}),
		new GeneroPackageClassMethod("delete", "os.Path", true, "Deletes a file or a directory.", new List<ParameterResult>
		{
			new ParameterResult("dname", "", "string")
		}, new List<string> {"integer"}),
		new GeneroPackageClassMethod("rename", "os.Path", true, "Renames a file or a directory.", new List<ParameterResult>
		{
			new ParameterResult("oldname", "", "string"),
			new ParameterResult("newname", "", "string")
		}, new List<string> {"integer"}),
		new GeneroPackageClassMethod("copy", "os.Path", true, "Creates a new file by copying an existing file.", new List<ParameterResult>
		{
			new ParameterResult("source", "", "string"),
			new ParameterResult("dest", "", "string")
		}, new List<string> {"integer"})
	})
}));
                    Packages.Add("com", new GeneroPackage("com", true, new List<GeneroPackageClass>
{
	new GeneroPackageClass("WebService", "com", false, new List<GeneroPackageClassMethod>
	{
		new GeneroPackageClassMethod("CreateWebService", "com.WebService", true, "Creates a new object to implement a Web Service.", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string"),
			new ParameterResult("namespace", "", "string")
		}, new List<string> {"com.WebService"}),
		new GeneroPackageClassMethod("CreateStatefulWebService", "com.WebService", true, "Creates a new object to implement a stateful Web Service.", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string"),
			new ParameterResult("namespace", "", "string"),
			new ParameterResult("state", "", "state-type")
		}, new List<string> {"com.WebService"}),
		new GeneroPackageClassMethod("setComment", "com.WebService", false, "Defines the comment for the Web Service object.", new List<ParameterResult>
		{
			new ParameterResult("comment", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("publishOperation", "com.WebService", false, "Publishes a Web Operation.", new List<ParameterResult>
		{
			new ParameterResult("operation", "", "com.WebOperation")
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("saveWSDL", "com.WebService", false, "Writes to a file the WSDL corresponding to the Web Service object.", new List<ParameterResult>
		{
			new ParameterResult("location", "", "string")
		}, new List<string> {"integer"}),
		new GeneroPackageClassMethod("generateWSDL", "com.WebService", false, "Creates a xml.DomDocument object with the WSDL corresponding to the Web Service object.", new List<ParameterResult>
		{
			new ParameterResult("location", "", "string")
		}, new List<string> {"xml.DomDocument"}),
		new GeneroPackageClassMethod("createHeader", "com.WebService", false, "Defines the header for the Web Service object.", new List<ParameterResult>
		{
			new ParameterResult("header", "", "header-type"),
			new ParameterResult("encoded", "", "boolean")
		}, new List<string> {}),
		new GeneroPackageClassMethod("createFault", "com.WebService", false, "Creates a new object to implement a Web Service.", new List<ParameterResult>
		{
			new ParameterResult("fault", "", "fault-type"),
			new ParameterResult("encoded", "", "boolean")
		}, new List<string> {}),
		new GeneroPackageClassMethod("registerWSDLHandler", "com.WebService", false, "Registers the function to be executed when a WSDL is generated.", new List<ParameterResult>
		{
			new ParameterResult("funcname", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("registerInputRequestHandler", "com.WebService", false, "Registers the function to be executed on incoming SOAP requests.", new List<ParameterResult>
		{
			new ParameterResult("funcname", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("registerOutputRequestHandler", "com.WebService", false, "Registers the function to be executed just before the SOAP response is forwarded to the client.", new List<ParameterResult>
		{
			new ParameterResult("funcname", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("registerInputHTTPVariable", "com.WebService", false, "Registers the record variable for HTTP input.", new List<ParameterResult>
		{
			new ParameterResult("http-in", "", "http-in-type")
		}, new List<string> {}),
		new GeneroPackageClassMethod("registerOutputHTTPVariable", "com.WebService", false, "Registers the record variable for HTTP output.", new List<ParameterResult>
		{
			new ParameterResult("http-out", "", "http-out-type")
		}, new List<string> {}),
		new GeneroPackageClassMethod("setFeature", "com.WebService", false, "Defines a feature for the current Web Service object.", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string"),
			new ParameterResult("value", "", "string")
		}, new List<string> {})
	}),
	new GeneroPackageClass("WebOperation", "com", false, new List<GeneroPackageClassMethod>
	{
		new GeneroPackageClassMethod("CreateRPCStyle", "com.WebOperation", true, "Creates a new Web Operation object with RPC style.", new List<ParameterResult>
		{
			new ParameterResult("function", "", "string"),
			new ParameterResult("operation", "", "string"),
			new ParameterResult("input", "", "record"),
			new ParameterResult("output", "", "record")
		}, new List<string> {"com.WebOperation"}),
		new GeneroPackageClassMethod("CreateDOCStyle", "com.WebOperation", true, "Creates a new Web Operation object with Document style.", new List<ParameterResult>
		{
			new ParameterResult("function", "", "string"),
			new ParameterResult("operation", "", "string"),
			new ParameterResult("input", "", "record"),
			new ParameterResult("output", "", "record")
		}, new List<string> {"com.WebOperation"}),
		new GeneroPackageClassMethod("CreateOneWayRPCStyle", "com.WebOperation", true, "Creates a new Web Operation object with One-Way RPC style.", new List<ParameterResult>
		{
			new ParameterResult("function", "", "string"),
			new ParameterResult("operation", "", "string"),
			new ParameterResult("input", "", "record")
		}, new List<string> {"com.WebOperation"}),
		new GeneroPackageClassMethod("CreateOneWayDOCStyle", "com.WebOperation", true, "Creates a new Web Operation object with One-Way DOC style.", new List<ParameterResult>
		{
			new ParameterResult("function", "", "string"),
			new ParameterResult("operation", "", "string"),
			new ParameterResult("input", "", "record")
		}, new List<string> {"com.WebOperation"}),
		new GeneroPackageClassMethod("setInputEncoded", "com.WebOperation", false, "Defines the encoding mechanism for Web Operation input parameters.", new List<ParameterResult>
		{
			new ParameterResult("encoded", "", "boolean")
		}, new List<string> {}),
		new GeneroPackageClassMethod("setOutputEncoded", "com.WebOperation", false, "Defines the encoding mechanism for Web Operation output parameters.", new List<ParameterResult>
		{
			new ParameterResult("encoded", "", "boolean")
		}, new List<string> {}),
		new GeneroPackageClassMethod("addInputHeader", "com.WebOperation", false, "Adds an input header for the current Web Operation definition.", new List<ParameterResult>
		{
			new ParameterResult("header", "", "header-type")
		}, new List<string> {}),
		new GeneroPackageClassMethod("addOutputHeader", "com.WebOperation", false, "Adds an output header for the current Web Operation definition.", new List<ParameterResult>
		{
			new ParameterResult("header", "", "header-type")
		}, new List<string> {}),
		new GeneroPackageClassMethod("addFault", "com.WebOperation", false, "Adds a fault to the current Web Operation definition.", new List<ParameterResult>
		{
			new ParameterResult("fault", "", "fault-type"),
			new ParameterResult("vsaaction", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("setInputAction", "com.WebOperation", false, "Sets the WS-Addressing action identifier of the input operation.", new List<ParameterResult>
		{
			new ParameterResult("indent", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("setOutputAction", "com.WebOperation", false, "Sets the WS-Addressing action identifier of the output operation.", new List<ParameterResult>
		{
			new ParameterResult("indent", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("setComment", "com.WebOperation", false, "Sets the comment for the Web Operation object.", new List<ParameterResult>
		{
			new ParameterResult("comment", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("initiateSession", "com.WebOperation", false, "Defines the Web Operation as session initiator.", new List<ParameterResult>
		{
			new ParameterResult("initiator", "", "boolean")
		}, new List<string> {})
	}),
	new GeneroPackageClass("WebServiceEngine", "com", true, new List<GeneroPackageClassMethod>
	{
		new GeneroPackageClassMethod("RegisterService", "com.WebServiceEngine", true, "Registers a service in the engine.", new List<ParameterResult>
		{
			new ParameterResult("service", "", "com.WebService")
		}, new List<string> {}),
		new GeneroPackageClassMethod("Start", "com.WebServiceEngine", true, "Starts the Web Service engine.", new List<ParameterResult>
		{
		}, new List<string> {}),
		new GeneroPackageClassMethod("ProcessServices", "com.WebServiceEngine", true, "Specifies the wait period for an HTTP input request, to process an operation of one of the registered Web Services.", new List<ParameterResult>
		{
			new ParameterResult("timeout", "", "integer")
		}, new List<string> {"integer"}),
		new GeneroPackageClassMethod("GetHTTPServiceRequest", "com.WebServiceEngine", true, "Get a handle for an incoming HTTP service request.", new List<ParameterResult>
		{
			new ParameterResult("timeout", "", "integer")
		}, new List<string> {"com.HTTPServiceRequest"}),
		new GeneroPackageClassMethod("HandleRequest", "com.WebServiceEngine", true, "Get a handle for an incoming HTTP service request.", new List<ParameterResult>
		{
			new ParameterResult("timeout", "", "integer"),
			new ParameterResult("status", "", "integer")
		}, new List<string> {"com.HTTPServiceRequest"}),
		new GeneroPackageClassMethod("SetFaultCode", "com.WebServiceEngine", true, "Get a handle for an incoming HTTP service request.", new List<ParameterResult>
		{
			new ParameterResult("code", "", "string"),
			new ParameterResult("code_ns", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("SetFaultString", "com.WebServiceEngine", true, "Defines the description of a SOAP Fault.", new List<ParameterResult>
		{
			new ParameterResult("desc", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("SetFaultDetail", "com.WebServiceEngine", true, "Defines the published SOAP Fault.", new List<ParameterResult>
		{
			new ParameterResult("fault", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("SetOption", "com.WebServiceEngine", true, "Sets an option for the Web Service engine.", new List<ParameterResult>
		{
			new ParameterResult("option", "", "string"),
			new ParameterResult("value", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("GetOption", "com.WebServiceEngine", true, "Returns the value of a Web Service engine option.", new List<ParameterResult>
		{
			new ParameterResult("option", "", "string")
		}, new List<string> {"string"})
	}),
	new GeneroPackageClass("HTTPServiceRequest", "com", false, new List<GeneroPackageClassMethod>
	{
		new GeneroPackageClassMethod("getURL", "com.HTTPServiceRequest", false, "Returns the URL of the HTTP service request.", new List<ParameterResult>
		{
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("getMethod", "com.HTTPServiceRequest", false, "Returns the HTTP method of the service request.", new List<ParameterResult>
		{
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("getRequestVersion", "com.HTTPServiceRequest", false, "Returns the HTTP version of the service request.", new List<ParameterResult>
		{
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("hasRequestKeepConnection", "com.HTTPServiceRequest", false, "Returns TRUE if the connection remains after sending a response.", new List<ParameterResult>
		{
		}, new List<string> {"boolean"}),
		new GeneroPackageClassMethod("getRequestHeader", "com.HTTPServiceRequest", false, "Returns the request header name.", new List<ParameterResult>
		{
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("getRequestHeaderCount", "com.HTTPServiceRequest", false, "Returns number of request headers.", new List<ParameterResult>
		{
		}, new List<string> {"integer"}),
		new GeneroPackageClassMethod("getRequestHeaderName", "com.HTTPServiceRequest", false, "Returns a request header name by position.", new List<ParameterResult>
		{
			new ParameterResult("index", "", "integer")
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("getRequestHeaderValue", "com.HTTPServiceRequest", false, "Returns a request header value by position.", new List<ParameterResult>
		{
			new ParameterResult("index", "", "integer")
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("readFromEncodedRequest", "com.HTTPServiceRequest", false, "Returns the string of a GET request with UTF-8 conversion option.", new List<ParameterResult>
		{
			new ParameterResult("utf8", "", "boolean")
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("readDataRequest", "com.HTTPServiceRequest", false, "Returns the body of a request into a BYTE.", new List<ParameterResult>
		{
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("readTextRequest", "com.HTTPServiceRequest", false, "Returns the request body as a plain string.", new List<ParameterResult>
		{
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("readXmlRequest", "com.HTTPServiceRequest", false, "Returns the request body as an XML document.", new List<ParameterResult>
		{
		}, new List<string> {"xml.DomDocument"}),
		new GeneroPackageClassMethod("beginXmlRequest", "com.HTTPServiceRequest", false, "Starts an HTTP streaming request.", new List<ParameterResult>
		{
		}, new List<string> {"xml.StaxReader"}),
		new GeneroPackageClassMethod("endXmlRequest", "com.HTTPServiceRequest", false, "Terminates an HTTP streaming request.", new List<ParameterResult>
		{
			new ParameterResult("reader", "", "xml.StaxReader")
		}, new List<string> {}),
		new GeneroPackageClassMethod("setResponseCharset", "com.HTTPServiceRequest", false, "Defines the HTTP response character set.", new List<ParameterResult>
		{
			new ParameterResult("charset", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("setResponseVersion", "com.HTTPServiceRequest", false, "Defines the HTTP response version.", new List<ParameterResult>
		{
			new ParameterResult("version", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("setResponseHeader", "com.HTTPServiceRequest", false, "Defines a header for the HTTP response.", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string"),
			new ParameterResult("value", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("sendDataResponse", "com.HTTPServiceRequest", false, "Sends an HTTP response with data of a BYTE variable.", new List<ParameterResult>
		{
			new ParameterResult("code", "", "integer"),
			new ParameterResult("desc", "", "string"),
			new ParameterResult("data", "", "byte")
		}, new List<string> {}),
		new GeneroPackageClassMethod("sendTextResponse", "com.HTTPServiceRequest", false, "Sends an HTTP response with data from a plain string.", new List<ParameterResult>
		{
			new ParameterResult("code", "", "integer"),
			new ParameterResult("desc", "", "string"),
			new ParameterResult("data", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("sendXmlResponse", "com.HTTPServiceRequest", false, "Sends an HTTP response with data from a XML document object.", new List<ParameterResult>
		{
			new ParameterResult("code", "", "integer"),
			new ParameterResult("desc", "", "string"),
			new ParameterResult("data", "", "xml.DomDocument")
		}, new List<string> {}),
		new GeneroPackageClassMethod("sendResponse", "com.HTTPServiceRequest", false, "Sends an HTTP response without body.", new List<ParameterResult>
		{
			new ParameterResult("code", "", "integer"),
			new ParameterResult("desc", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("beginXmlResponse", "com.HTTPServiceRequest", false, "Starts an HTTP streaming response.", new List<ParameterResult>
		{
			new ParameterResult("code", "", "integer"),
			new ParameterResult("desc", "", "string")
		}, new List<string> {"xml.StaxWriter"}),
		new GeneroPackageClassMethod("endXmlResponse", "com.HTTPServiceRequest", false, "Terminates an HTTP streaming response.", new List<ParameterResult>
		{
			new ParameterResult("writer", "", "xml.StaxWriter")
		}, new List<string> {}),
		new GeneroPackageClassMethod("getRequestMultipartType", "com.HTTPServiceRequest", false, "Returns the multipart type of an incoming request.", new List<ParameterResult>
		{
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("getRequestPartCount", "com.HTTPServiceRequest", false, "Returns the number of additional multipart elements.", new List<ParameterResult>
		{
		}, new List<string> {"integer"}),
		new GeneroPackageClassMethod("getRequestPart", "com.HTTPServiceRequest", false, "Returns the HTTPPart object at the specified index position.", new List<ParameterResult>
		{
			new ParameterResult("idx", "", "integer")
		}, new List<string> {"com.HTTPPart"}),
		new GeneroPackageClassMethod("getRequestPartFromContentID", "com.HTTPServiceRequest", false, "Returns the HTTPPart object of the given Content-ID value.", new List<ParameterResult>
		{
			new ParameterResult("id", "", "string")
		}, new List<string> {"com.HTTPPart"}),
		new GeneroPackageClassMethod("setResponseMultipartType", "com.HTTPServiceRequest", false, "Sets HTTP response in multipart mode of given type.", new List<ParameterResult>
		{
			new ParameterResult("type", "", "string"),
			new ParameterResult("start", "", "string"),
			new ParameterResult("boundary", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("addResponsePart", "com.HTTPServiceRequest", false, "Adds a new part to the HTTP root part response.", new List<ParameterResult>
		{
			new ParameterResult("part-object", "", "com.HTTPPart")
		}, new List<string> {})
	}),
	new GeneroPackageClass("HTTPRequest", "com", false, new List<GeneroPackageClassMethod>
	{
		new GeneroPackageClassMethod("Create", "com.HTTPRequest", true, "Creates an new HTTPRequest object from a URL.", new List<ParameterResult>
		{
			new ParameterResult("url", "", "string")
		}, new List<string> {"com.HTTPRequest"}),
		new GeneroPackageClassMethod("setVersion", "com.HTTPRequest", false, "Sets the HTTP version of the request.", new List<ParameterResult>
		{
			new ParameterResult("version", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("setMethod", "com.HTTPRequest", false, "Sets the HTTP method of the request.", new List<ParameterResult>
		{
			new ParameterResult("method", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("setHeader", "com.HTTPRequest", false, "Sets an HTTP header for the request.", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string"),
			new ParameterResult("value", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("removeHeader", "com.HTTPRequest", false, "Removes an HTTP header for the request according to a name.", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("clearHeaders", "com.HTTPRequest", false, "Removes all user-defined HTTP request headers.", new List<ParameterResult>
		{
		}, new List<string> {}),
		new GeneroPackageClassMethod("setCharset", "com.HTTPRequest", false, "Defines the charset used when sending text or XML.", new List<ParameterResult>
		{
			new ParameterResult("charset", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("setAuthentication", "com.HTTPRequest", false, "Defines the user login and password to authenticate to the server.", new List<ParameterResult>
		{
			new ParameterResult("login", "", "string"),
			new ParameterResult("pass", "", "string"),
			new ParameterResult("scheme", "", "string"),
			new ParameterResult("realm", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("clearAuthentication", "com.HTTPRequest", false, "Removes user-defined authentication.", new List<ParameterResult>
		{
		}, new List<string> {}),
		new GeneroPackageClassMethod("setKeepConnection", "com.HTTPRequest", false, "Defines if connection is kept open if a new request occurs.", new List<ParameterResult>
		{
			new ParameterResult("keep", "", "boolean")
		}, new List<string> {}),
		new GeneroPackageClassMethod("setTimeOut", "com.HTTPRequest", false, "Defines the timeout for a reading or writing operation.", new List<ParameterResult>
		{
			new ParameterResult("timeout", "", "integer")
		}, new List<string> {}),
		new GeneroPackageClassMethod("setConnectionTimeOut", "com.HTTPRequest", false, "Defines the timeout for the establishment of the connection.", new List<ParameterResult>
		{
			new ParameterResult("timeout", "", "integer")
		}, new List<string> {}),
		new GeneroPackageClassMethod("setMaximumResponseLength", "com.HTTPRequest", false, "Defines the maximum size in Kbyte a response.", new List<ParameterResult>
		{
			new ParameterResult("length", "", "integer")
		}, new List<string> {}),
		new GeneroPackageClassMethod("setAutoReply", "com.HTTPRequest", false, "Defines the auto reply option for response methods.", new List<ParameterResult>
		{
			new ParameterResult("reply", "", "boolean")
		}, new List<string> {}),
		new GeneroPackageClassMethod("doRequest", "com.HTTPRequest", false, "Performs the HTTP request.", new List<ParameterResult>
		{
		}, new List<string> {}),
		new GeneroPackageClassMethod("doTextRequest", "com.HTTPRequest", false, "Performs the request by sending an entire string at once.", new List<ParameterResult>
		{
			new ParameterResult("data", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("doXmlRequest", "com.HTTPRequest", false, "Performs the request by sending an entire XML document at once.", new List<ParameterResult>
		{
			new ParameterResult("data", "", "xml.DomDocument")
		}, new List<string> {}),
		new GeneroPackageClassMethod("doDataRequest", "com.HTTPRequest", false, "Performs the request by sending binary data.", new List<ParameterResult>
		{
			new ParameterResult("data", "", "byte")
		}, new List<string> {}),
		new GeneroPackageClassMethod("doFormEncodedRequest", "com.HTTPRequest", false, "Performs an \"application/x-www-form-urlencoded forms\" encoded query.", new List<ParameterResult>
		{
			new ParameterResult("query", "", "string"),
			new ParameterResult("utf8", "", "boolean")
		}, new List<string> {}),
		new GeneroPackageClassMethod("beginXmlRequest", "com.HTTPRequest", false, "Starts a streaming HTTP request.", new List<ParameterResult>
		{
		}, new List<string> {"xml.StaxWriter"}),
		new GeneroPackageClassMethod("endXmlRequest", "com.HTTPRequest", false, "Terminates a streaming HTTP request.", new List<ParameterResult>
		{
			new ParameterResult("writer", "", "xml.StaxWriter")
		}, new List<string> {}),
		new GeneroPackageClassMethod("getResponse", "com.HTTPRequest", false, "Returns the response produced by one of request methods.", new List<ParameterResult>
		{
		}, new List<string> {"com.HTTPResponse"}),
		new GeneroPackageClassMethod("getAsyncResponse", "com.HTTPRequest", false, "Returns (asynchronously) the response produced by one of request methods.", new List<ParameterResult>
		{
		}, new List<string> {"com.HTTPResponse"}),
		new GeneroPackageClassMethod("setMultipartType", "com.HTTPRequest", false, "Switch HTTPRequest in multipart mode of given type.", new List<ParameterResult>
		{
			new ParameterResult("type", "", "string"),
			new ParameterResult("start", "", "string"),
			new ParameterResult("boundary", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("addPart", "com.HTTPRequest", false, "Adds a new part to the HTTP root part request.", new List<ParameterResult>
		{
			new ParameterResult("part", "", "com.HTTPPart")
		}, new List<string> {})
	}),
	new GeneroPackageClass("HTTPResponse", "com", false, new List<GeneroPackageClassMethod>
	{
		new GeneroPackageClassMethod("getStatusCode", "com.HTTPResponse", false, "Returns the HTTP status code.", new List<ParameterResult>
		{
		}, new List<string> {"integer"}),
		new GeneroPackageClassMethod("getStatusDescription", "com.HTTPResponse", false, "Returns the HTTP status description.", new List<ParameterResult>
		{
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("beginXmlResponse", "com.HTTPResponse", false, "Starts a streaming HTTP response.", new List<ParameterResult>
		{
		}, new List<string> {"xml.StaxWriter"}),
		new GeneroPackageClassMethod("endXmlResponse", "com.HTTPResponse", false, "Performs the HTTP request.", new List<ParameterResult>
		{
			new ParameterResult("writer", "", "xml.StaxWriter")
		}, new List<string> {}),
		new GeneroPackageClassMethod("getHeader", "com.HTTPResponse", false, "Returns the value of the HTTP header name, or NULL if there is none.", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string")
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("getHeaderCount", "com.HTTPResponse", false, "Returns the number of headers.", new List<ParameterResult>
		{
		}, new List<string> {"integer"}),
		new GeneroPackageClassMethod("getHeaderName", "com.HTTPResponse", false, "Returns the name of a header by position.", new List<ParameterResult>
		{
			new ParameterResult("index", "", "integer")
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("getHeaderValue", "com.HTTPResponse", false, "Returns the value of a header by position.", new List<ParameterResult>
		{
			new ParameterResult("index", "", "integer")
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("getDataResponse", "com.HTTPResponse", false, "Performs the HTTP request.", new List<ParameterResult>
		{
			new ParameterResult("data", "", "byte")
		}, new List<string> {}),
		new GeneroPackageClassMethod("getTextResponse", "com.HTTPResponse", false, "Returns an entire HTTP string as response from the server.", new List<ParameterResult>
		{
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("getXmlResponse", "com.HTTPResponse", false, "Returns an entire DOM document as HTTP response from the server.", new List<ParameterResult>
		{
		}, new List<string> {"xml.DomDocument"}),
		new GeneroPackageClassMethod("getMultipartType", "com.HTTPResponse", false, "Returns whether a response is multipart or not, and the kind of multipart if any.", new List<ParameterResult>
		{
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("getPartCount", "com.HTTPResponse", false, "Returns the number of additional parts in the HTTP response.", new List<ParameterResult>
		{
		}, new List<string> {"integer"}),
		new GeneroPackageClassMethod("getPart", "com.HTTPResponse", false, "Returns the HTTP part object at the specified index of the current HTTP response.", new List<ParameterResult>
		{
			new ParameterResult("index", "", "integer")
		}, new List<string> {"com.HTTPPart"}),
		new GeneroPackageClassMethod("getPartFromContentID", "com.HTTPResponse", false, "Returns the HTTP part object marked with the given Content-ID value as identifier, or NULL if none.", new List<ParameterResult>
		{
			new ParameterResult("id", "", "string")
		}, new List<string> {"com.HTTPPart"})
	}),
	new GeneroPackageClass("HTTPPart", "com", false, new List<GeneroPackageClassMethod>
	{
		new GeneroPackageClassMethod("CreateFromString", "com.HTTPPart", true, "Creates a new HTTPPart object based on given string.", new List<ParameterResult>
		{
			new ParameterResult("s", "", "string")
		}, new List<string> {"com.HTTPPart"}),
		new GeneroPackageClassMethod("CreateFromDomDocument", "com.HTTPPart", true, "Creates a new HTTPPart object based on given XML document.", new List<ParameterResult>
		{
			new ParameterResult("x", "", "xml.DomDocument")
		}, new List<string> {"com.HTTPPart"}),
		new GeneroPackageClassMethod("CreateFromData", "com.HTTPPart", true, "Creates a new HTTPPart object based on given BYTE located in memory.", new List<ParameterResult>
		{
			new ParameterResult("b", "", "byte")
		}, new List<string> {"com.HTTPPart"}),
		new GeneroPackageClassMethod("CreateAttachment", "com.HTTPPart", true, "Creates a new HTTPPart object based on given filename located on disk.", new List<ParameterResult>
		{
			new ParameterResult("filename", "", "string")
		}, new List<string> {"com.HTTPPart"}),
		new GeneroPackageClassMethod("getContentAsString", "com.HTTPPart", false, "Returns the HTTP part as a string.", new List<ParameterResult>
		{
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("getContentAsDomDocument", "com.HTTPPart", false, "Returns the HTTP part as a XML document.", new List<ParameterResult>
		{
		}, new List<string> {"xml.DomDocument"}),
		new GeneroPackageClassMethod("getContentAsData", "com.HTTPPart", false, "Returns the HTTP part as a BYTE.", new List<ParameterResult>
		{
			new ParameterResult("b", "", "byte")
		}, new List<string> {}),
		new GeneroPackageClassMethod("getAttachment", "com.HTTPPart", false, "Returns the temporary filename located on disk of the HTTP part.", new List<ParameterResult>
		{
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("setHeader", "com.HTTPPart", false, "Setter to handle HTTP multipart headers.", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string"),
			new ParameterResult("value", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("getHeader", "com.HTTPPart", false, "Setter to handle HTTP multipart headers.", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string")
		}, new List<string> {"string"})
	}),
	new GeneroPackageClass("TCPRequest", "com", false, new List<GeneroPackageClassMethod>
	{
		new GeneroPackageClassMethod("Create", "com.TCPRequest", true, "Creates a new TCP request object.", new List<ParameterResult>
		{
			new ParameterResult("url", "", "string")
		}, new List<string> {"com.TCPRequest"}),
		new GeneroPackageClassMethod("setTimeOut", "com.TCPRequest", false, "Defines the time out for read/write operations.", new List<ParameterResult>
		{
			new ParameterResult("seconds", "", "integer")
		}, new List<string> {}),
		new GeneroPackageClassMethod("setConnectionTimeOut", "com.TCPRequest", false, "Defines the connection time out.", new List<ParameterResult>
		{
			new ParameterResult("seconds", "", "integer")
		}, new List<string> {}),
		new GeneroPackageClassMethod("setMaximumResponseLength", "com.TCPRequest", false, "Defines the time out for read/write operations.", new List<ParameterResult>
		{
			new ParameterResult("length", "", "integer")
		}, new List<string> {}),
		new GeneroPackageClassMethod("doRequest", "com.TCPRequest", false, "Performs a TCP request.", new List<ParameterResult>
		{
		}, new List<string> {}),
		new GeneroPackageClassMethod("doXmlRequest", "com.TCPRequest", false, "Performs a request with a DOM document.", new List<ParameterResult>
		{
			new ParameterResult("document", "", "xml.DomDocument")
		}, new List<string> {}),
		new GeneroPackageClassMethod("doTextRequest", "com.TCPRequest", false, "Performs a request with a string.", new List<ParameterResult>
		{
			new ParameterResult("data", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("beginXmlRequest", "com.TCPRequest", false, "Starts a streaming XML request.", new List<ParameterResult>
		{
		}, new List<string> {"xml.StaxWriter"}),
		new GeneroPackageClassMethod("endXmlRequest", "com.TCPRequest", false, "Terminates a streaming TCP request.", new List<ParameterResult>
		{
			new ParameterResult("writer", "", "xml.StaxWriter")
		}, new List<string> {}),
		new GeneroPackageClassMethod("getResponse", "com.TCPRequest", false, "Returns the response after performing a TCP request.", new List<ParameterResult>
		{
		}, new List<string> {"com.TCPResponse"}),
		new GeneroPackageClassMethod("getAsyncResponse", "com.TCPRequest", false, "Returns the response after performing a TCP request, asynchronously.", new List<ParameterResult>
		{
		}, new List<string> {"com.TCPResponse"})
	}),
	new GeneroPackageClass("TCPResponse", "com", false, new List<GeneroPackageClassMethod>
	{
		new GeneroPackageClassMethod("beginXmlResponse", "com.TCPResponse", false, "Starts a streaming TCP response.", new List<ParameterResult>
		{
		}, new List<string> {"xml.StaxReader"}),
		new GeneroPackageClassMethod("endXmlResponse", "com.TCPResponse", false, "Ends a streaming TCP response.", new List<ParameterResult>
		{
			new ParameterResult("reader", "", "xml.StaxReader")
		}, new List<string> {}),
		new GeneroPackageClassMethod("getXmlResponse", "com.TCPResponse", false, "Returns an entire DOM document as TCP response from the server.", new List<ParameterResult>
		{
		}, new List<string> {"xml.DomDocument"}),
		new GeneroPackageClassMethod("getTextResponse", "com.TCPResponse", false, "Returns a string as TCP response from the server.", new List<ParameterResult>
		{
		}, new List<string> {"string"})
	}),
	new GeneroPackageClass("Util", "com", true, new List<GeneroPackageClassMethod>
	{
		new GeneroPackageClassMethod("CreateRandomString", "com.Util", true, "Creates a new random string for a given size (deprecated!)", new List<ParameterResult>
		{
			new ParameterResult("size", "", "integer")
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("CreateDigestString", "com.Util", true, "Creates a new Base64 digest string from a source and random string (deprecated!).", new List<ParameterResult>
		{
			new ParameterResult("source", "", "string"),
			new ParameterResult("random", "", "string")
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("CreateUUIDString", "com.Util", true, "Creates an UUID string (deprecated!)", new List<ParameterResult>
		{
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("UniqueApplicationInstance", "com.Util", true, "Creates a new random string (deprecated!).", new List<ParameterResult>
		{
			new ParameterResult("path", "", "string")
		}, new List<string> {"integer"})
	})
}));
                    Packages.Add("xml", new GeneroPackage("xml", true, new List<GeneroPackageClass>
{
	new GeneroPackageClass("DomDocument", "xml", false, new List<GeneroPackageClassMethod>
	{
		new GeneroPackageClassMethod("create", "xml.DomDocument", true, "Constructor of an empty DomDocument object.", new List<ParameterResult>
		{
		}, new List<string> {"xml.DomDocument"}),
		new GeneroPackageClassMethod("createDocument", "xml.DomDocument", true, "Constructor of a DomDocument with an XML root element.", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string")
		}, new List<string> {"xml.DomDocument"}),
		new GeneroPackageClassMethod("createDocumentNS", "xml.DomDocument", true, "Constructor of a DomDocument with a root namespace-qualified XML root element", new List<ParameterResult>
		{
			new ParameterResult("prefix", "", "string"),
			new ParameterResult("name", "", "string"),
			new ParameterResult("ns", "", "string")
		}, new List<string> {"xml.DomDocument"}),
		new GeneroPackageClassMethod("getDocumentElement", "xml.DomDocument", false, "Returns the root XML Element DomNode object for this DomDocument object.", new List<ParameterResult>
		{
		}, new List<string> {"xml.DomNode"}),
		new GeneroPackageClassMethod("getFirstDocumentNode", "xml.DomDocument", false, "Returns the first child DomNode object for a DomDocument object.", new List<ParameterResult>
		{
		}, new List<string> {"xml.DomNode"}),
		new GeneroPackageClassMethod("getLastDocumentNode", "xml.DomDocument", false, "Returns the last child DomNode object for a DomDocument object.", new List<ParameterResult>
		{
		}, new List<string> {"xml.DomNode"}),
		new GeneroPackageClassMethod("getDocumentNodesCount", "xml.DomDocument", false, "Returns the number of child DomNode objects for a DomDocument object.", new List<ParameterResult>
		{
		}, new List<string> {"integer"}),
		new GeneroPackageClassMethod("getDocumentNodeItem", "xml.DomDocument", false, "Returns the child DomNode object at a given position for this DomDocument object.", new List<ParameterResult>
		{
			new ParameterResult("pos", "", "integer")
		}, new List<string> {"xml.DomNode"}),
		new GeneroPackageClassMethod("getElementsByTagName", "xml.DomDocument", false, "Returns a DomNodeList object containing all XML Element DomNode objects with the same tag name in the entire document.", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string")
		}, new List<string> {"xml.DomNodeList"}),
		new GeneroPackageClassMethod("getElementsByTagNameNS", "xml.DomDocument", false, "Returns a DomNodeList object containing all namespace qualified XML Element DomNode objects with the same tag name and namespace in the entire document", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string"),
			new ParameterResult("ns", "", "string")
		}, new List<string> {"xml.DomNodeList"}),
		new GeneroPackageClassMethod("selectByXPath", "xml.DomDocument", false, "Returns a DomNodeList object containing all DomNode objects matching an XPath 1.0 expression.", new List<ParameterResult>
		{
			new ParameterResult("expr", "", "string"),
			new ParameterResult("nslist...", "", "string")
		}, new List<string> {"xml.DomNodeList"}),
		new GeneroPackageClassMethod("getElementById", "xml.DomDocument", false, "Returns the element that has an attribute of type ID with the given value", new List<ParameterResult>
		{
			new ParameterResult("id", "", "string")
		}, new List<string> {"xml.DomNode"}),
		new GeneroPackageClassMethod("importNode", "xml.DomDocument", false, "Imports a DomNode from a DomDocument object into its new context (attached to a DomDocument object).", new List<ParameterResult>
		{
			new ParameterResult("node", "", "xml.DomNode"),
			new ParameterResult("deep", "", "integer")
		}, new List<string> {"xml.DomNode"}),
		new GeneroPackageClassMethod("clone", "xml.DomDocument", false, "Returns a copy of a DomDocument object.", new List<ParameterResult>
		{
		}, new List<string> {"xml.DomDocument"}),
		new GeneroPackageClassMethod("appendDocumentNode", "xml.DomDocument", false, "Adds a child DomNode object to the end of the DomNode children for this DomDocument object.", new List<ParameterResult>
		{
			new ParameterResult("node", "", "xml.DomNode")
		}, new List<string> {}),
		new GeneroPackageClassMethod("prependDocumentNode", "xml.DomDocument", false, "Adds a child DomNode object to the beginning of the DomNode children for this DomDocument object.", new List<ParameterResult>
		{
			new ParameterResult("node", "", "xml.DomNode")
		}, new List<string> {}),
		new GeneroPackageClassMethod("insertBeforeDocumentNode", "xml.DomDocument", false, "Inserts a child DomNode object before another child DomNode for this DomDocument object.", new List<ParameterResult>
		{
			new ParameterResult("node", "", "xml.DomNode"),
			new ParameterResult("ref", "", "xml.DomNode")
		}, new List<string> {}),
		new GeneroPackageClassMethod("insertAfterDocumentNode", "xml.DomDocument", false, "Inserts a child DomNode object after another child DomNode for this DomDocument object.", new List<ParameterResult>
		{
			new ParameterResult("node", "", "xml.DomNode"),
			new ParameterResult("ref", "", "xml.DomNode")
		}, new List<string> {}),
		new GeneroPackageClassMethod("removeDocumentNode", "xml.DomDocument", false, "Removes a child DomNode object from the DomNode children for this DomDocument object.", new List<ParameterResult>
		{
			new ParameterResult("node", "", "xml.DomNode")
		}, new List<string> {}),
		new GeneroPackageClassMethod("declareNamespace", "xml.DomDocument", false, "Forces namespace declaration to an XML Element DomNode for a DomDocument object.", new List<ParameterResult>
		{
			new ParameterResult("node", "", "xml.DomNode"),
			new ParameterResult("alias", "", "string"),
			new ParameterResult("ns", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("createNode", "xml.DomDocument", false, "Creates an XML DomNode object from a string for a DomDocument object.", new List<ParameterResult>
		{
			new ParameterResult("str", "", "string")
		}, new List<string> {"xml.DomNode"}),
		new GeneroPackageClassMethod("createElement", "xml.DomDocument", false, "Creates an XML Element DomNode object for a DomDocument object", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string")
		}, new List<string> {"xml.DomNode"}),
		new GeneroPackageClassMethod("createElementNS", "xml.DomDocument", false, "Creates an XML namespace-qualified Element DomNode object for a DomDocument object.", new List<ParameterResult>
		{
			new ParameterResult("prefix", "", "string"),
			new ParameterResult("name", "", "string"),
			new ParameterResult("ns", "", "string")
		}, new List<string> {"xml.DomNode"}),
		new GeneroPackageClassMethod("createAttribute", "xml.DomDocument", false, "Creates an XML Attribute DomNode object for a DomDocument object.", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string")
		}, new List<string> {"xml.DomNode"}),
		new GeneroPackageClassMethod("createAttributeNS", "xml.DomDocument", false, "Creates an XML namespace-qualified Attribute DomNode object for a DomDocument object.", new List<ParameterResult>
		{
			new ParameterResult("prefix", "", "string"),
			new ParameterResult("name", "", "string"),
			new ParameterResult("ns", "", "string")
		}, new List<string> {"xml.DomNode"}),
		new GeneroPackageClassMethod("createTextNode", "xml.DomDocument", false, "Creates an XML Text DomNode object for a DomDocument object.", new List<ParameterResult>
		{
			new ParameterResult("text", "", "string")
		}, new List<string> {"xml.DomNode"}),
		new GeneroPackageClassMethod("createComment", "xml.DomDocument", false, "Creates an XML Comment DomNode object for a DomDocument object.", new List<ParameterResult>
		{
			new ParameterResult("comment", "", "string")
		}, new List<string> {"xml.DomNode"}),
		new GeneroPackageClassMethod("createCDATASection", "xml.DomDocument", false, "Creates an XML CData DomNode object for a DomDocument object.", new List<ParameterResult>
		{
			new ParameterResult("cdata", "", "string")
		}, new List<string> {"xml.DomNode"}),
		new GeneroPackageClassMethod("createEntityReference", "xml.DomDocument", false, "Creates an XML EntityReference DomNode object for a DomDocument object", new List<ParameterResult>
		{
			new ParameterResult("ref", "", "string")
		}, new List<string> {"xml.DomNode"}),
		new GeneroPackageClassMethod("createProcessingInstruction", "xml.DomDocument", false, "Creates an XML Processing Instruction DomNode object for this DomDocument object.", new List<ParameterResult>
		{
			new ParameterResult("target", "", "string"),
			new ParameterResult("data", "", "string")
		}, new List<string> {"xml.DomNode"}),
		new GeneroPackageClassMethod("createDocumentType", "xml.DomDocument", false, "Creates an XML Document Type (DTD) DomNode object for a DomDocument object.", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string"),
			new ParameterResult("publicID", "", "string"),
			new ParameterResult("systemID", "", "string"),
			new ParameterResult("internalDTD", "", "string")
		}, new List<string> {"xml.DomNode"}),
		new GeneroPackageClassMethod("createDocumentFragment", "xml.DomDocument", false, "Creates an XML Document Fragment DomNode object for a DomDocument object.", new List<ParameterResult>
		{
		}, new List<string> {"xml.DomNode"}),
		new GeneroPackageClassMethod("normalize", "xml.DomDocument", false, "Normalizes the entire Document.", new List<ParameterResult>
		{
		}, new List<string> {}),
		new GeneroPackageClassMethod("load", "xml.DomDocument", false, "Loads an XML Document into a DomDocument object from a file or an URL.", new List<ParameterResult>
		{
			new ParameterResult("url", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("loadFromString", "xml.DomDocument", false, "Loads an XML Document into a DomDocument object from a string.", new List<ParameterResult>
		{
			new ParameterResult("str", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("loadFromPipe", "xml.DomDocument", false, "Loads an XML Document into a DomDocument object from a PIPE.", new List<ParameterResult>
		{
			new ParameterResult("cmd", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("save", "xml.DomDocument", false, "Saves a DomDocument object as an XML Document to a file or URL.", new List<ParameterResult>
		{
			new ParameterResult("url", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("saveToString", "xml.DomDocument", false, "Saves a DomDocument object as an XML Document to a string.", new List<ParameterResult>
		{
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("saveToPipe", "xml.DomDocument", false, "Saves a DomDocument object as an XML Document to a PIPE.", new List<ParameterResult>
		{
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("setFeature", "xml.DomDocument", false, "Sets a feature for a DomDocument object.", new List<ParameterResult>
		{
			new ParameterResult("feature", "", "string"),
			new ParameterResult("value", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("getFeature", "xml.DomDocument", false, "Gets a feature for a DomDocument object.", new List<ParameterResult>
		{
			new ParameterResult("feature", "", "string")
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("getXmlVersion", "xml.DomDocument", false, "Returns the document version as defined in the XML document declaration.", new List<ParameterResult>
		{
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("getXmlEncoding", "xml.DomDocument", false, "Returns the document encoding as defined in the XML document declaration.", new List<ParameterResult>
		{
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("setXmlEncoding", "xml.DomDocument", false, "Sets the XML document encoding in the XML declaration.", new List<ParameterResult>
		{
			new ParameterResult("enc", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("isXmlStandalone", "xml.DomDocument", false, "Returns whether the XML standalone attribute is set in the XML declaration.", new List<ParameterResult>
		{
		}, new List<string> {"integer"}),
		new GeneroPackageClassMethod("setXmlStandalone", "xml.DomDocument", false, "Sets the XML standalone attribute in the XML declaration to yes or no in the XML declaration.", new List<ParameterResult>
		{
			new ParameterResult("alone", "", "integer")
		}, new List<string> {}),
		new GeneroPackageClassMethod("validate", "xml.DomDocument", false, "Performs a DTD or XML Schema validation for a DomDocument object.", new List<ParameterResult>
		{
		}, new List<string> {"integer"}),
		new GeneroPackageClassMethod("validateOneElement", "xml.DomDocument", false, "Performs a DTD or XML Schema validation of an XML Element DomNode object.", new List<ParameterResult>
		{
			new ParameterResult("node", "", "xml.DomNode")
		}, new List<string> {"integer"}),
		new GeneroPackageClassMethod("getErrorsCount", "xml.DomDocument", false, "Returns the number of errors encountered during the loading, saving or validation of an XML document.", new List<ParameterResult>
		{
		}, new List<string> {"integer"}),
		new GeneroPackageClassMethod("getErrorDescription", "xml.DomDocument", false, "Returns the error description at given position.", new List<ParameterResult>
		{
			new ParameterResult("pos", "", "integer")
		}, new List<string> {"string"})
	}),
	new GeneroPackageClass("DomNode", "xml", false, new List<GeneroPackageClassMethod>
	{
		new GeneroPackageClassMethod("getParentNode", "xml.DomNode", false, "Returns the parent DomNode object for this DomNode object.", new List<ParameterResult>
		{
		}, new List<string> {"xml.DomNode"}),
		new GeneroPackageClassMethod("getFirstChild", "xml.DomNode", false, "Returns the first child DomNode object for this XML Element DomNode object.", new List<ParameterResult>
		{
		}, new List<string> {"xml.DomNode"}),
		new GeneroPackageClassMethod("getFirstChildElement", "xml.DomNode", false, "Returns the first XML Element child DomNode object for this DomNode object.", new List<ParameterResult>
		{
		}, new List<string> {"xml.DomNode"}),
		new GeneroPackageClassMethod("getLastChild", "xml.DomNode", false, "Returns the last child DomNode object for a XML Element DomNode object.", new List<ParameterResult>
		{
		}, new List<string> {"xml.DomNode"}),
		new GeneroPackageClassMethod("getLastChildElement", "xml.DomNode", false, "Returns the last child XML element DomNode object for this DomNode object.", new List<ParameterResult>
		{
		}, new List<string> {"xml.DomNode"}),
		new GeneroPackageClassMethod("getNextSibling", "xml.DomNode", false, "Returns the DomNode object immediately following a DomNode object.", new List<ParameterResult>
		{
		}, new List<string> {"xml.DomNode"}),
		new GeneroPackageClassMethod("getNextSiblingElement", "xml.DomNode", false, "Returns the XML Element DomNode object immediately following a DomNode object.", new List<ParameterResult>
		{
		}, new List<string> {"xml.DomNode"}),
		new GeneroPackageClassMethod("getPreviousSibling", "xml.DomNode", false, "Returns the DomNode object immediately preceding a DomNode object.", new List<ParameterResult>
		{
		}, new List<string> {"xml.DomNode"}),
		new GeneroPackageClassMethod("getOwnerDocument", "xml.DomNode", false, "Returns the DomDocument object containing this DomNode object.", new List<ParameterResult>
		{
		}, new List<string> {"xml.DomDocument"}),
		new GeneroPackageClassMethod("hasChildNodes", "xml.DomNode", false, "Returns TRUE if a node has child nodes.", new List<ParameterResult>
		{
		}, new List<string> {"integer"}),
		new GeneroPackageClassMethod("getChildrenCount", "xml.DomNode", false, "Returns the number of child DomNode objects for a DomNode object.", new List<ParameterResult>
		{
		}, new List<string> {"integer"}),
		new GeneroPackageClassMethod("getChildNodeItem", "xml.DomNode", false, "Returns the child DomNode object at a given position for a DomNode object.", new List<ParameterResult>
		{
			new ParameterResult("pos", "", "integer")
		}, new List<string> {"xml.DomNode"}),
		new GeneroPackageClassMethod("prependChildElement", "xml.DomNode", false, "Creates and adds a child XML Element node to the beginning of the list of child nodes for this XML Element DomNode object.", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string")
		}, new List<string> {"xml.DomNode"}),
		new GeneroPackageClassMethod("prependChildElementNS", "xml.DomNode", false, "Creates and adds a child namespace-qualified XML Element node to the beginning of the list of child nodes for an XML Element DomNode object.", new List<ParameterResult>
		{
			new ParameterResult("prefix", "", "string"),
			new ParameterResult("name", "", "string"),
			new ParameterResult("ns", "", "string")
		}, new List<string> {"xml.DomNode"}),
		new GeneroPackageClassMethod("addPreviousSibling", "xml.DomNode", false, "Adds a DomNode object as the previous sibling of a DomNode object.", new List<ParameterResult>
		{
		}, new List<string> {"xml.DomNode"}),
		new GeneroPackageClassMethod("addNextSibling", "xml.DomNode", false, "Adds a DomNode object as the next sibling of a DomNode object.", new List<ParameterResult>
		{
			new ParameterResult("node", "", "xml.DomNode")
		}, new List<string> {}),
		new GeneroPackageClassMethod("prependChild", "xml.DomNode", false, "Adds a child DomNode object to the beginning of the child list for a DomNode object.", new List<ParameterResult>
		{
		}, new List<string> {"xml.DomNode"}),
		new GeneroPackageClassMethod("appendChild", "xml.DomNode", false, "Adds a child DomNode object to the end of the child list for a DomNode object", new List<ParameterResult>
		{
			new ParameterResult("node", "", "xml.DomNode")
		}, new List<string> {}),
		new GeneroPackageClassMethod("insertBeforeChild", "xml.DomNode", false, "Inserts a DomNode object before an existing child DomNode object.", new List<ParameterResult>
		{
			new ParameterResult("node", "", "xml.DomNode"),
			new ParameterResult("ref", "", "xml.DomNode")
		}, new List<string> {}),
		new GeneroPackageClassMethod("insertAfterChild", "xml.DomNode", false, "Inserts a DomNode object after an existing child DomNode object.", new List<ParameterResult>
		{
			new ParameterResult("node", "", "xml.DomNode"),
			new ParameterResult("ref", "", "xml.DomNode")
		}, new List<string> {}),
		new GeneroPackageClassMethod("removeChild", "xml.DomNode", false, "Removes a child DomNode object from the list of child DomNode objects.", new List<ParameterResult>
		{
			new ParameterResult("node", "", "xml.DomNode")
		}, new List<string> {}),
		new GeneroPackageClassMethod("removeAllChildren", "xml.DomNode", false, "Removes all child DomNode objects from a DomNode object.", new List<ParameterResult>
		{
		}, new List<string> {}),
		new GeneroPackageClassMethod("appendChildElement", "xml.DomNode", false, "Creates and adds a child XML Element node to the end of the list of child nodes for an XML Element DomNode object.", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string")
		}, new List<string> {"xml.DomNode"}),
		new GeneroPackageClassMethod("appendChildElementNS", "xml.DomNode", false, "Creates and adds a child namespace qualified XML Element node to the end of the list of child nodes for an XML Element DomNode object.", new List<ParameterResult>
		{
			new ParameterResult("prefix", "", "string"),
			new ParameterResult("name", "", "string"),
			new ParameterResult("ns", "", "string")
		}, new List<string> {"xml.DomNode"}),
		new GeneroPackageClassMethod("clone", "xml.DomNode", false, "Returns a duplicate DomNode object of a node.", new List<ParameterResult>
		{
			new ParameterResult("deep", "", "integer")
		}, new List<string> {"xml.DomNode"}),
		new GeneroPackageClassMethod("getNodeType", "xml.DomNode", false, "Gets the XML type for this DomNode object.", new List<ParameterResult>
		{
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("getLocalName", "xml.DomNode", false, "Gets the local name for a DomNode object.", new List<ParameterResult>
		{
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("getNodeName", "xml.DomNode", false, "Gets the name for a DomNode object.", new List<ParameterResult>
		{
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("getNodeValue", "xml.DomNode", false, "Gets the value for a DomNode object.", new List<ParameterResult>
		{
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("getNamespaceURI", "xml.DomNode", false, "Returns the namespace URI for a DomNode object.", new List<ParameterResult>
		{
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("getPrefix", "xml.DomNode", false, "Returns the prefix for a DomNode object.", new List<ParameterResult>
		{
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("isAttached", "xml.DomNode", false, "Returns whether the node is attached to the XML document.", new List<ParameterResult>
		{
		}, new List<string> {"integer"}),
		new GeneroPackageClassMethod("setNodeValue", "xml.DomNode", false, "Sets the node value for a DomNode object.", new List<ParameterResult>
		{
			new ParameterResult("val", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("setPrefix", "xml.DomNode", false, "Sets the prefix for a DomNode object.", new List<ParameterResult>
		{
			new ParameterResult("prefix", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("toString", "xml.DomNode", false, "Returns a string representation of a DomNode object.", new List<ParameterResult>
		{
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("hasAttribute", "xml.DomNode", false, "Checks whether an XML Element DomNode object has the XML Attribute specified by a specified name.", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string")
		}, new List<string> {"integer"}),
		new GeneroPackageClassMethod("hasAttributeNS", "xml.DomNode", false, "Checks whether a namespace qualified XML Attribute of a given name is carried by an XML Element DomNode object.", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string"),
			new ParameterResult("ns", "", "string")
		}, new List<string> {"integer"}),
		new GeneroPackageClassMethod("getAttributeNode", "xml.DomNode", false, "Returns an XML Attribute DomNode object for an XML Element DomNode object", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string")
		}, new List<string> {"xml.DomNode"}),
		new GeneroPackageClassMethod("getAttributeNodeNS", "xml.DomNode", false, "Returns a namespace-qualified XML Attribute DomNode object for an XML Element DomNode object", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string"),
			new ParameterResult("ns", "", "string")
		}, new List<string> {"xml.DomNode"}),
		new GeneroPackageClassMethod("setAttributeNode", "xml.DomNode", false, "Sets (or resets) an XML Attribute DomNode object to an XML Element DomNode object.", new List<ParameterResult>
		{
			new ParameterResult("node", "", "xml.DomNode")
		}, new List<string> {}),
		new GeneroPackageClassMethod("setAttributeNodeNS", "xml.DomNode", false, "Sets (or resets) a namespace-qualified XML Attribute DomNode object to an XML Element DomNode object.", new List<ParameterResult>
		{
			new ParameterResult("node", "", "xml.DomNode")
		}, new List<string> {}),
		new GeneroPackageClassMethod("getAttribute", "xml.DomNode", false, "Returns the value of an XML Attribute for an XML Element DomNode object", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string")
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("getAttributeNS", "xml.DomNode", false, "Returns the value of a namespace qualified XML Attribute for an XML Element DomNode object", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string"),
			new ParameterResult("ns", "", "string")
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("setAttribute", "xml.DomNode", false, "Sets (or resets) an XML Attribute for an XML Element DomNode object.", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string"),
			new ParameterResult("value", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("setAttributeNS", "xml.DomNode", false, "Sets (or resets) a namespace-qualified XML Attribute for an XML Element DomNode object.", new List<ParameterResult>
		{
			new ParameterResult("prefix", "", "string"),
			new ParameterResult("name", "", "string"),
			new ParameterResult("ns", "", "string"),
			new ParameterResult("value", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("setIdAttribute", "xml.DomNode", false, "Declare (or undeclare) the XML Attribute of given name to be of type ID.", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string"),
			new ParameterResult("isId", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("setIdAttributeNS", "xml.DomNode", false, "Declare (or undeclare) the namespace-qualified XML Attribute of given name and namespace to be of type ID.", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string"),
			new ParameterResult("ns", "", "string"),
			new ParameterResult("isId", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("removeAttribute", "xml.DomNode", false, "Removes an XML Attribute for an XML Element DomNode object.", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("removeAttributeNS", "xml.DomNode", false, "Removes a namespace qualified XML Attribute for an XML Element DomNode object", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string"),
			new ParameterResult("ns", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("hasAttributes", "xml.DomNode", false, "Identifies whether a node has XML Attribute nodes.", new List<ParameterResult>
		{
		}, new List<string> {"integer"}),
		new GeneroPackageClassMethod("getArributesCount", "xml.DomNode", false, "Returns the number of XML Attribute DomNode objects on this XML Element DomNode object.", new List<ParameterResult>
		{
		}, new List<string> {"integer"}),
		new GeneroPackageClassMethod("getAttributeNodeItem", "xml.DomNode", false, "Returns the XML Attribute DomNode object at a given position on this XML Element DomNode object.", new List<ParameterResult>
		{
			new ParameterResult("pos", "", "integer")
		}, new List<string> {"xml.DomNode"}),
		new GeneroPackageClassMethod("selectByXPath", "xml.DomNode", false, "Returns a DomNodeList object containing all DomNode objects matching an XPath 1.0 expression.", new List<ParameterResult>
		{
			new ParameterResult("expr", "", "string"),
			new ParameterResult("NamespacesList...", "", "nsList")
		}, new List<string> {"xml.DomNodeList"}),
		new GeneroPackageClassMethod("getElementsByTagName", "xml.DomNode", false, "Returns a DomNodeList object containing all XML Element DomNode objects with the same tag name.", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string")
		}, new List<string> {"xml.DomNodeList"}),
		new GeneroPackageClassMethod("getElementsByTagNameNS", "xml.DomNode", false, "Returns a DomNodeList object containing all namespace-qualified XML Element DomNode objects with the same tag name and namespace.", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string"),
			new ParameterResult("ns", "", "string")
		}, new List<string> {"xml.DomNodeList"}),
		new GeneroPackageClassMethod("isDefaultNamespace", "xml.DomNode", false, "Checks whether the specified namespace URI is the default namespace.", new List<ParameterResult>
		{
			new ParameterResult("ns", "", "string")
		}, new List<string> {"integer"}),
		new GeneroPackageClassMethod("lookupNamespaceURI", "xml.DomNode", false, "Looks up the namespace URI associated to a prefix, starting from a specified node.", new List<ParameterResult>
		{
			new ParameterResult("prefix", "", "string")
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("lookupPrefix", "xml.DomNode", false, "Looks up the prefix associated to a namespace URI, starting from the specified node.", new List<ParameterResult>
		{
			new ParameterResult("ns", "", "string")
		}, new List<string> {"string"})
	}),
	new GeneroPackageClass("DomNodeList", "xml", false, new List<GeneroPackageClassMethod>
	{
		new GeneroPackageClassMethod("getCount", "xml.DomNodeList", false, "Returns the number of DomNode objects in a DomNodeList object.", new List<ParameterResult>
		{
		}, new List<string> {"integer"}),
		new GeneroPackageClassMethod("getItem", "xml.DomNodeList", false, "Returns the DomNode object at a given position in a DomNodeList object.", new List<ParameterResult>
		{
			new ParameterResult("pos", "", "integer")
		}, new List<string> {"xml.DomNode"})
	}),
	new GeneroPackageClass("StaxWriter", "xml", false, new List<GeneroPackageClassMethod>
	{
		new GeneroPackageClassMethod("create", "xml.StaxWriter", true, "Constructor of a StaxWriter object.", new List<ParameterResult>
		{
		}, new List<string> {"xml.StaxWriter"}),
		new GeneroPackageClassMethod("setFeature", "xml.StaxWriter", false, "Sets a feature of a StaxWriter object.", new List<ParameterResult>
		{
			new ParameterResult("feature", "", "string"),
			new ParameterResult("value", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("getFeature", "xml.StaxWriter", false, "Gets a feature of a StaxWriter object.", new List<ParameterResult>
		{
			new ParameterResult("feature", "", "string")
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("writeTo", "xml.StaxWriter", false, "Sets the output stream of the StaxWriter object to a file or an URL, and starts the streaming.", new List<ParameterResult>
		{
			new ParameterResult("url", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("writeToDocument", "xml.StaxWriter", false, "Sets the output stream of the StaxWriter object to an xml.DomDocument object, and starts the streaming.", new List<ParameterResult>
		{
			new ParameterResult("doc", "", "xml.DomDocument")
		}, new List<string> {}),
		new GeneroPackageClassMethod("writeToText", "xml.StaxWriter", false, "Sets the output stream of the StaxWriter object to a TEXT large object, and starts the streaming.", new List<ParameterResult>
		{
			new ParameterResult("txt", "", "text")
		}, new List<string> {}),
		new GeneroPackageClassMethod("writeToPipe", "xml.StaxWriter", false, "Sets the output stream of the StaxWriter object to a PIPE, and starts the streaming.", new List<ParameterResult>
		{
			new ParameterResult("cmd", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("close", "xml.StaxWriter", false, "Closes the StaxWriter streaming, and releases all associated resources.", new List<ParameterResult>
		{
		}, new List<string> {}),
		new GeneroPackageClassMethod("startDocument", "xml.StaxWriter", false, "Writes an XML declaration to the StaxWriter stream.", new List<ParameterResult>
		{
			new ParameterResult("encoding", "", "string"),
			new ParameterResult("version", "", "string"),
			new ParameterResult("standalone", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("endDocument", "xml.StaxWriter", false, "Closes any open tags and writes corresponding end tags.", new List<ParameterResult>
		{
		}, new List<string> {}),
		new GeneroPackageClassMethod("dtd", "xml.StaxWriter", false, "Writes a DTD to the StaxWriter stream.", new List<ParameterResult>
		{
			new ParameterResult("data", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("setPrefix", "xml.StaxWriter", false, "Binds a namespace URI to a prefix.", new List<ParameterResult>
		{
			new ParameterResult("prefix", "", "string"),
			new ParameterResult("ns", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("setDefaultNamespace", "xml.StaxWriter", false, "Binds a namespace URI to the default namespace.", new List<ParameterResult>
		{
			new ParameterResult("defaultNS", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("declareNamespace", "xml.StaxWriter", false, "Binds a namespace URI to a prefix, and forces the output of the XML namespace definition to the StaxWriter stream.", new List<ParameterResult>
		{
			new ParameterResult("prefix", "", "string"),
			new ParameterResult("ns", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("declareDefaultNamespace", "xml.StaxWriter", false, "Binds a namespace URI to the default namespace, and forces the output of the default XML namespace definition to the StaxWriter stream.", new List<ParameterResult>
		{
			new ParameterResult("defaultNS", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("startElement", "xml.StaxWriter", false, "Writes an XML start element to the StaxWriter stream.", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("emptyElement", "xml.StaxWriter", false, "Writes an empty XML element to the StaxWriter stream.", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("startElementNS", "xml.StaxWriter", false, "Writes a namespace-qualified XML start element to the StaxWriter stream.", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string"),
			new ParameterResult("ns", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("emptyElementNS", "xml.StaxWriter", false, "Writes an empty namespace qualified XML element to the StaxWriter stream.", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string"),
			new ParameterResult("ns", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("endElement", "xml.StaxWriter", false, "Writes an end tag to the StaxWriter stream relying on the internal state to determine the prefix and local name of the last START_ELEMENT.", new List<ParameterResult>
		{
		}, new List<string> {}),
		new GeneroPackageClassMethod("attribute", "xml.StaxWriter", false, "Writes an XML attribute to the StaxWriter stream.", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string"),
			new ParameterResult("value", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("attributeNS", "xml.StaxWriter", false, "Writes an XML namespace qualified attribute to the StaxWriter stream.", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string"),
			new ParameterResult("ns", "", "string"),
			new ParameterResult("value", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("processingInstruction", "xml.StaxWriter", false, "Writes an XML ProcessingInstruction to the StaxWriter stream", new List<ParameterResult>
		{
			new ParameterResult("target", "", "string"),
			new ParameterResult("data", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("comment", "xml.StaxWriter", false, "Writes an XML comment to the StaxWriter stream.", new List<ParameterResult>
		{
			new ParameterResult("data", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("characters", "xml.StaxWriter", false, "Writes an XML text to the StaxWriter stream.", new List<ParameterResult>
		{
			new ParameterResult("text", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("cdata", "xml.StaxWriter", false, "Writes an XML CData to the StaxWriter stream.", new List<ParameterResult>
		{
			new ParameterResult("data", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("entityRef", "xml.StaxWriter", false, "Writes an XML EntityReference to the StaxWriter stream.", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string")
		}, new List<string> {})
	}),
	new GeneroPackageClass("StaxReader", "xml", false, new List<GeneroPackageClassMethod>
	{
		new GeneroPackageClassMethod("Create", "xml.StaxReader", true, "Constructor of a StaxReader object.", new List<ParameterResult>
		{
		}, new List<string> {"xml.StaxReader"}),
		new GeneroPackageClassMethod("setFeature", "xml.StaxReader", false, "Sets a feature of a StaxReader object.", new List<ParameterResult>
		{
			new ParameterResult("feature", "", "string"),
			new ParameterResult("value", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("getFeature", "xml.StaxReader", false, "Gets a feature of a StaxReader object.", new List<ParameterResult>
		{
			new ParameterResult("feature", "", "string")
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("readFrom", "xml.StaxReader", false, "Sets the input stream of the StaxReader object to a file or an URL and starts the streaming", new List<ParameterResult>
		{
			new ParameterResult("url", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("readFromDocument", "xml.StaxReader", false, "Sets the input stream of the StaxReader object to a DomDocument object and starts the streaming", new List<ParameterResult>
		{
			new ParameterResult("doc", "", "xml.DomDocument")
		}, new List<string> {}),
		new GeneroPackageClassMethod("readFromText", "xml.StaxReader", false, "Sets the input stream of the StaxReader object to a TEXT large object and starts the streaming.", new List<ParameterResult>
		{
			new ParameterResult("txt", "", "text")
		}, new List<string> {}),
		new GeneroPackageClassMethod("readFromPipe", "xml.StaxReader", false, "Sets the input stream of the StaxReader object to a PIPE and starts the streaming.", new List<ParameterResult>
		{
			new ParameterResult("cmd", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("close", "xml.StaxReader", false, "Closes the StaxReader streaming and releases all associated resources.", new List<ParameterResult>
		{
		}, new List<string> {}),
		new GeneroPackageClassMethod("getEventType", "xml.StaxReader", false, "Returns a string that indicates the type of event the cursor of the StaxReader object is pointing to.", new List<ParameterResult>
		{
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("hasName", "xml.StaxReader", false, "Checks whether the StaxReader cursor points to a node with a name.", new List<ParameterResult>
		{
		}, new List<string> {"integer"}),
		new GeneroPackageClassMethod("hasText", "xml.StaxReader", false, "Checks whether the StaxReader cursor points to a node with a text value.", new List<ParameterResult>
		{
		}, new List<string> {"integer"}),
		new GeneroPackageClassMethod("isEmptyElement", "xml.StaxReader", false, "Checks whether the StaxReader cursor points to an empty element node.", new List<ParameterResult>
		{
		}, new List<string> {"integer"}),
		new GeneroPackageClassMethod("isStartElement", "xml.StaxReader", false, "Checks whether the StaxReader cursor points to a start element node.", new List<ParameterResult>
		{
		}, new List<string> {"integer"}),
		new GeneroPackageClassMethod("isEndElement", "xml.StaxReader", false, "Checks whether the StaxReader cursor points to an end element node.", new List<ParameterResult>
		{
		}, new List<string> {"integer"}),
		new GeneroPackageClassMethod("isCharacters", "xml.StaxReader", false, "Checks whether the StaxReader cursor points to a text node.", new List<ParameterResult>
		{
		}, new List<string> {"integer"}),
		new GeneroPackageClassMethod("isIgnorableWhitespace", "xml.StaxReader", false, "Checks whether the StaxReader cursor points to ignorable whitespace.", new List<ParameterResult>
		{
		}, new List<string> {"integer"}),
		new GeneroPackageClassMethod("getEncoding", "xml.StaxReader", false, "Returns the document encoding defined in the XML Document declaration, or NULL.", new List<ParameterResult>
		{
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("getVersion", "xml.StaxReader", false, "Returns the document version defined in the XML Document declaration, or NULL.", new List<ParameterResult>
		{
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("isStandalone", "xml.StaxReader", false, "Checks whether the document standalone attribute defined in the XML Document declaration is set to yes.", new List<ParameterResult>
		{
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("standaloneSet", "xml.StaxReader", false, "Checks whether the document standalone attribute is defined in the XML Document declaration.", new List<ParameterResult>
		{
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("getPrefix", "xml.StaxReader", false, "Returns the prefix of the current XML node, or NULL.", new List<ParameterResult>
		{
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("getLocalName", "xml.StaxReader", false, "Returns the local name of the current XML node, or NULL.", new List<ParameterResult>
		{
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("getName", "xml.StaxReader", false, "Returns the qualified name of the current XML node, or NULL.", new List<ParameterResult>
		{
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("getNamespace", "xml.StaxReader", false, "Returns the namespace URI of the current XML node, or NULL.", new List<ParameterResult>
		{
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("getText", "xml.StaxReader", false, "Returns as a string the value of the current XML node, or NULL.", new List<ParameterResult>
		{
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("getPITarget", "xml.StaxReader", false, "Returns the target part of an XML ProcessingInstruction node, or NULL.", new List<ParameterResult>
		{
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("getPIData", "xml.StaxReader", false, "Returns the data part of an XML ProcessingInstruction node, or NULL.", new List<ParameterResult>
		{
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("getAttributeCount", "xml.StaxReader", false, "Returns the number of XML attributes defined on the current XML node, or zero.", new List<ParameterResult>
		{
		}, new List<string> {"integer"}),
		new GeneroPackageClassMethod("getAttributeLocalName", "xml.StaxReader", false, "Returns the local name of an XML attribute defined at a given position on the current XML node, or NULL.", new List<ParameterResult>
		{
			new ParameterResult("pos", "", "integer")
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("getAttributeNamespace", "xml.StaxReader", false, "Returns the namespace URI of an XML attribute defined at a given position on the current XML node, or NULL.", new List<ParameterResult>
		{
			new ParameterResult("pos", "", "integer")
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("getAttributePrefix", "xml.StaxReader", false, "Returns the prefix of an XML attribute defined at a given position on the current XML node, or NULL.", new List<ParameterResult>
		{
			new ParameterResult("pos", "", "integer")
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("getAttributeValue", "xml.StaxReader", false, "Returns the value of an XML attribute defined at a given position on the current XML node, or NULL.", new List<ParameterResult>
		{
			new ParameterResult("pos", "", "integer")
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("findAttributeValue", "xml.StaxReader", false, "Returns the value of an XML attribute of a given name and/or namespace on the current XML node, or NULL.", new List<ParameterResult>
		{
			new ParameterResult("name", "", "string"),
			new ParameterResult("ns", "", "string")
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("lookupNamespace", "xml.StaxReader", false, "Looks up the namespace URI associated with a given prefix starting from the current XML node the StaxReader cursor is pointing to.", new List<ParameterResult>
		{
			new ParameterResult("prefix", "", "string")
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("lookupPrefix", "xml.StaxReader", false, "Looks up the prefix associated with a given namespace URI, starting from the current XML node the StaxReader cursor is pointing to.", new List<ParameterResult>
		{
			new ParameterResult("ns", "", "string")
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("getNamespaceCount", "xml.StaxReader", false, "Returns the number of namespace declarations defined on the current XML node, or zero.", new List<ParameterResult>
		{
		}, new List<string> {"integer"}),
		new GeneroPackageClassMethod("getNamespacePrefix", "xml.StaxReader", false, "Returns the prefix of a namespace declaration defined at a given position on the current XML node, or NULL.", new List<ParameterResult>
		{
			new ParameterResult("pos", "", "integer")
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("getNamespaceURI", "xml.StaxReader", false, "Returns the URI of a namespace declaration defined at a given position on the current XML node, or NULL.", new List<ParameterResult>
		{
			new ParameterResult("pos", "", "integer")
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("hasNext", "xml.StaxReader", false, "Checks whether the StaxReader cursor can be moved to a XML node next to it.", new List<ParameterResult>
		{
		}, new List<string> {"integer"}),
		new GeneroPackageClassMethod("next", "xml.StaxReader", false, "Moves the StaxReader cursor to the next XML node.", new List<ParameterResult>
		{
		}, new List<string> {}),
		new GeneroPackageClassMethod("nextTag", "xml.StaxReader", false, "Moves the StaxReader cursor to the next XML open or end tag", new List<ParameterResult>
		{
		}, new List<string> {}),
		new GeneroPackageClassMethod("nextSibling", "xml.StaxReader", false, "Moves the StaxReader cursor to the immediate next sibling XML Element of the current node, skipping all its child nodes.", new List<ParameterResult>
		{
		}, new List<string> {})
	}),
	new GeneroPackageClass("Serializer", "xml", true, new List<GeneroPackageClassMethod>
	{
		new GeneroPackageClassMethod("SetOption", "xml.Serializer", true, "Sets a global option value for the serializer engine", new List<ParameterResult>
		{
			new ParameterResult("flag", "", "string"),
			new ParameterResult("value", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("GetOption", "xml.Serializer", true, "Gets a global option value from the serializer engine.", new List<ParameterResult>
		{
			new ParameterResult("flag", "", "string")
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("VariableToStax", "xml.Serializer", true, "Serializes a 4GL variable into an XML element node using a StaxWriter object.", new List<ParameterResult>
		{
			new ParameterResult("var", "", "variable"),
			new ParameterResult("stax", "", "xml.StaxWriter")
		}, new List<string> {}),
		new GeneroPackageClassMethod("StaxToVariable", "xml.Serializer", true, "Serializes an XML element node into a 4GL variable using a StaxReader object.", new List<ParameterResult>
		{
			new ParameterResult("stax", "", "xml.StaxReader"),
			new ParameterResult("var", "", "variable")
		}, new List<string> {}),
		new GeneroPackageClassMethod("VariableToDom", "xml.Serializer", true, "Serializes a 4GL variable into an XML element node using a DomNode object.", new List<ParameterResult>
		{
			new ParameterResult("var", "", "variable"),
			new ParameterResult("node", "", "xml.DomNode")
		}, new List<string> {}),
		new GeneroPackageClassMethod("DomToVariable", "xml.Serializer", true, "Serializes an XML element node into a 4GL variable using a DomNode object.", new List<ParameterResult>
		{
			new ParameterResult("node", "", "xml.DomNode"),
			new ParameterResult("var", "", "variable")
		}, new List<string> {}),
		new GeneroPackageClassMethod("VariableToSoapSection5", "xml.Serializer", true, "Serializes a 4GL variable into an XML element node in Soap Section 5 encoding.", new List<ParameterResult>
		{
			new ParameterResult("var", "", "variable"),
			new ParameterResult("node", "", "xml.DomNode")
		}, new List<string> {}),
		new GeneroPackageClassMethod("SoapSection5ToVariable", "xml.Serializer", true, "Serializes an XML element node into a 4GL variable in Soap Section 5 encoding.", new List<ParameterResult>
		{
			new ParameterResult("node", "", "xml.DomNode"),
			new ParameterResult("var", "", "variable")
		}, new List<string> {}),
		new GeneroPackageClassMethod("DomToStax", "xml.Serializer", true, "Serializes an XML node object to a StaxWriter object.", new List<ParameterResult>
		{
			new ParameterResult("node", "", "xml.DomNode"),
			new ParameterResult("stax", "", "xml.StaxWriter")
		}, new List<string> {}),
		new GeneroPackageClassMethod("StaxToDom", "xml.Serializer", true, "Serializes an XML element node into a DomNode object using a StaxReader object.", new List<ParameterResult>
		{
			new ParameterResult("stax", "", "xml.StaxWriter"),
			new ParameterResult("node", "", "xml.DomNode")
		}, new List<string> {}),
		new GeneroPackageClassMethod("CreateXmlSchemas", "xml.Serializer", true, "Creates XML schemas corresponding to the given variable var, and fills the dynamic array ar with xml.DomDocument objects each representing an XML schema.", new List<ParameterResult>
		{
			new ParameterResult("var", "", "variable"),
			new ParameterResult("ar", "", "dynamic array of xml.DomDocument")
		}, new List<string> {})
	}),
	new GeneroPackageClass("CryptoKey", "xml", false, new List<GeneroPackageClassMethod>
	{
		new GeneroPackageClassMethod("Create", "xml.CryptoKey", true, "Initializes an xml.CryptoKey object. Constructor of an empty CryptoKey object depending on a url.", new List<ParameterResult>
		{
			new ParameterResult("url", "", "string")
		}, new List<string> {"xml.CryptoKey"}),
		new GeneroPackageClassMethod("CreateFromNode", "xml.CryptoKey", true, "Constructor of a new CryptoKey object depending on a url and from a XML node, according to the XML-Signature and XML-Encryption specification.", new List<ParameterResult>
		{
			new ParameterResult("url", "", "string"),
			new ParameterResult("node", "", "xml.DomNode")
		}, new List<string> {"xml.CryptoKey"}),
		new GeneroPackageClassMethod("CreateDerivedKey", "xml.CryptoKey", true, "Constructor of an empty CryptoKey object intended to be derived before use, and depending on a url.", new List<ParameterResult>
		{
			new ParameterResult("url", "", "string")
		}, new List<string> {"xml.CryptoKey"}),
		new GeneroPackageClassMethod("getUrl", "xml.CryptoKey", false, "Returns the key identifier as an URL, as defined in the XML-Signature and XML-Encryption specification.", new List<ParameterResult>
		{
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("getType", "xml.CryptoKey", false, "Returns the type of key.", new List<ParameterResult>
		{
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("getUsage", "xml.CryptoKey", false, "Returns the usage of the key.", new List<ParameterResult>
		{
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("getSize", "xml.CryptoKey", false, "Returns the size of the key in bits.", new List<ParameterResult>
		{
		}, new List<string> {"integer"}),
		new GeneroPackageClassMethod("compareTo", "xml.CryptoKey", false, "Compares a CryptoKey object to a second key.", new List<ParameterResult>
		{
			new ParameterResult("secondKey", "", "xml.CryptoKey")
		}, new List<string> {"integer"}),
		new GeneroPackageClassMethod("getSHA1", "xml.CryptoKey", false, "Returns the SHA1 encoded key identifier in a base64 encoded STRING.", new List<ParameterResult>
		{
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("setKey", "xml.CryptoKey", false, "Defines the value of a HMAC or Symmetric key.", new List<ParameterResult>
		{
			new ParameterResult("key", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("generateKey", "xml.CryptoKey", false, "Generates a random key of given size (in bits).", new List<ParameterResult>
		{
			new ParameterResult("size", "", "integer")
		}, new List<string> {}),
		new GeneroPackageClassMethod("deriveKey", "xml.CryptoKey", false, "Derives the symmetric or HMAC CryptoKey object using the given method identifier and concatenating the optional label, the mandatory seed value and the optional created date as initial random value.", new List<ParameterResult>
		{
			new ParameterResult("method", "", "string"),
			new ParameterResult("label", "", "string"),
			new ParameterResult("seed", "", "string"),
			new ParameterResult("created", "", "string"),
			new ParameterResult("offset", "", "integer"),
			new ParameterResult("size", "", "integer")
		}, new List<string> {}),
		new GeneroPackageClassMethod("computeKey", "xml.CryptoKey", false, "Computes the shared secret based on the given modulus, generator, the private key and the other peer's public key. The returned key can be any of symmetric/HMAC or symmetric/encryption key type. It can be used for symmetric signature or symmetric encryption.", new List<ParameterResult>
		{
			new ParameterResult("otherPubKey", "", "xml.CryptoKey"),
			new ParameterResult("url", "", "string")
		}, new List<string> {"xml.CryptoKey"}),
		new GeneroPackageClassMethod("loadBIN", "xml.CryptoKey", false, "Loads a symmetric or HMAC key from a file in raw format.", new List<ParameterResult>
		{
			new ParameterResult("file", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("loadDER", "xml.CryptoKey", false, "Loads an asymmetric DSA key, an asymmetric RSA key or Diffie-Hellman parameters from a file in DER format.", new List<ParameterResult>
		{
			new ParameterResult("file", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("loadPEM", "xml.CryptoKey", false, "Loads an asymmetric DSA key, an asymmetric RSA key or Diffie-Hellman parameters from a file in PEM format.", new List<ParameterResult>
		{
			new ParameterResult("file", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("loadFromString", "xml.CryptoKey", false, "Loads the given key in BASE64 string format into a CryptoKey object.", new List<ParameterResult>
		{
			new ParameterResult("str", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("loadPrivate", "xml.CryptoKey", false, "Loads the private asymmetric RSA key in the given XML document into the private part of this CryptoKey object, according to the XKMS2.0 specification.", new List<ParameterResult>
		{
			new ParameterResult("xml", "", "xml.DomDocument")
		}, new List<string> {}),
		new GeneroPackageClassMethod("loadPublic", "xml.CryptoKey", false, "Loads the public asymmetric RSA or DSA key in the given XML document into the public part of this CryptoKey object, according to the XML-Signature specification for DSA and RSA key value.", new List<ParameterResult>
		{
			new ParameterResult("xml", "", "xml.DomDocument")
		}, new List<string> {}),
		new GeneroPackageClassMethod("loadPublicFromString", "xml.CryptoKey", false, "Populate the current CryptoKey object with the passed public key.", new List<ParameterResult>
		{
			new ParameterResult("pubKeyStr", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("savePrivate", "xml.CryptoKey", false, "Saves the private part of an asymmetric RSA CryptoKey object into a XML document according to the XKMS2.0 specification.", new List<ParameterResult>
		{
		}, new List<string> {"xml.DomDocument"}),
		new GeneroPackageClassMethod("savePublic", "xml.CryptoKey", false, "Saves the public part of an asymmetric RSA or DSA CryptoKey object or the parameters and the public key of the Diffie-Hellman object into a XML document according to the XML-Signature specification for DSA and RSA and Diffie-Hellman key values.", new List<ParameterResult>
		{
		}, new List<string> {"xml.DomDocument"}),
		new GeneroPackageClassMethod("savePublicToString", "xml.CryptoKey", false, "Save the current xml.CryptoKey's public part in the returned base64 string.", new List<ParameterResult>
		{
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("saveToString", "xml.CryptoKey", false, "Saves the CryptoKey object into a BASE64 string format.", new List<ParameterResult>
		{
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("setFeature", "xml.CryptoKey", false, "Sets or resets the value of a feature for a CryptoKey object.", new List<ParameterResult>
		{
			new ParameterResult("feature", "", "string"),
			new ParameterResult("value", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("getFeature", "xml.CryptoKey", false, "Returns the value of the given feature for this CryptoKey object, or NULL.", new List<ParameterResult>
		{
			new ParameterResult("feature", "", "string")
		}, new List<string> {"string"})
	}),
	new GeneroPackageClass("CryptoX509", "xml", false, new List<GeneroPackageClassMethod>
	{
		new GeneroPackageClassMethod("Create", "xml.CryptoX509", true, "Constructor of an empty CryptoX509 object.", new List<ParameterResult>
		{
		}, new List<string> {"xml.CryptoX509"}),
		new GeneroPackageClassMethod("CreateFromNode", "xml.CryptoX509", true, "Constructor of a new CryptoX509 object from a XML X509 certificate node, according to the XML-Signature specification", new List<ParameterResult>
		{
			new ParameterResult("node", "", "xml.DomNode")
		}, new List<string> {"xml.CryptoX509"}),
		new GeneroPackageClassMethod("getIdentifier", "xml.CryptoX509", false, "Gets the identification part of an X509 certificate", new List<ParameterResult>
		{
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("getThumbprintSHA1", "xml.CryptoX509", false, "Gets the SHA1 encoded thumbprint identifying this X509 certificate.", new List<ParameterResult>
		{
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("createPublicKey", "xml.CryptoX509", false, "Creates a new public CryptoKey object for the given url, from the public key embedded in a certificate.", new List<ParameterResult>
		{
			new ParameterResult("url", "", "string")
		}, new List<string> {"xml.CryptoX509"}),
		new GeneroPackageClassMethod("loadPEM", "xml.CryptoX509", false, "Loads a X509 certificate from a file in PEM format.", new List<ParameterResult>
		{
			new ParameterResult("file", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("loadDER", "xml.CryptoX509", false, "Loads a X509 certificate from a file in DER format.", new List<ParameterResult>
		{
			new ParameterResult("file", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("save", "xml.CryptoX509", false, "Saves the CryptoX509 certificate into a XML document with ds:X509Data as root node according to the XML-Signature specification.", new List<ParameterResult>
		{
		}, new List<string> {"xml.DomDocument"}),
		new GeneroPackageClassMethod("saveToString", "xml.CryptoX509", false, "Saves the CryptoX509 certificate into a BASE64 string format.", new List<ParameterResult>
		{
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("load", "xml.CryptoX509", false, "Loads the given XML document with ds:X509Data as root node according to the XML-Signature specification, into the CryptoX509 object.", new List<ParameterResult>
		{
			new ParameterResult("xml", "", "xml.DomDocument")
		}, new List<string> {}),
		new GeneroPackageClassMethod("loadFromString", "xml.CryptoX509", false, "Loads the given X509 certificate in BASE64 string format into this CryptoX509 object.", new List<ParameterResult>
		{
			new ParameterResult("str", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("setFeature", "xml.CryptoX509", false, "Sets or resets the given feature for this CryptoX509 object.", new List<ParameterResult>
		{
			new ParameterResult("feature", "", "string"),
			new ParameterResult("value", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("getFeature", "xml.CryptoX509", false, "Get the value of a given feature of a CryptoX509 object.", new List<ParameterResult>
		{
			new ParameterResult("feature", "", "string")
		}, new List<string> {"string"})
	}),
	new GeneroPackageClass("Signature", "xml", false, new List<GeneroPackageClassMethod>
	{
		new GeneroPackageClassMethod("Create", "xml.Signature", true, "Constructor of a blank Signature object.", new List<ParameterResult>
		{
		}, new List<string> {"xml.Signature"}),
		new GeneroPackageClassMethod("CreateFromNode", "xml.Signature", true, "Constructor of a new Signature object from a XML Signature node, according to the XML-Signature specification.", new List<ParameterResult>
		{
			new ParameterResult("signode", "", "xml.DomNode")
		}, new List<string> {"xml.Signature"}),
		new GeneroPackageClassMethod("RetrieveObjectDataListFromSignatureNode", "xml.Signature", true, "Returns a DomNodeList containing all embedded XML nodes related to the signature object of index ind in the XML Signature node sign.", new List<ParameterResult>
		{
			new ParameterResult("signode", "", "xml.DomNode"),
			new ParameterResult("ind", "", "int")
		}, new List<string> {"xml.DomNodeList"}),
		new GeneroPackageClassMethod("setKey", "xml.Signature", false, "Defines the key used for signing or validation.", new List<ParameterResult>
		{
			new ParameterResult("key", "", "xml.CryptoKey")
		}, new List<string> {}),
		new GeneroPackageClassMethod("setCertificate", "xml.Signature", false, "Defines the X509 certificate to be added to the Signature object when signing a document.", new List<ParameterResult>
		{
			new ParameterResult("cert", "", "xml.CryptoX509")
		}, new List<string> {}),
		new GeneroPackageClassMethod("setCanonicalization", "xml.Signature", false, "Sets the canonicalization method to use for the signature.", new List<ParameterResult>
		{
			new ParameterResult("url", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("setID", "xml.Signature", false, "Sets an ID value for the signature.", new List<ParameterResult>
		{
			new ParameterResult("id", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("getType", "xml.Signature", false, "Returns a string with the type of the Signature object.", new List<ParameterResult>
		{
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("getDocument", "xml.Signature", false, "Returns a new DomDocument object representing the signature in XML.", new List<ParameterResult>
		{
		}, new List<string> {"xml.DomDocument"}),
		new GeneroPackageClassMethod("getCanonicalization", "xml.Signature", false, "Returns one of the four canonicalization identifier of the signature.", new List<ParameterResult>
		{
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("getID", "xml.Signature", false, "Returns the ID value of the signature.", new List<ParameterResult>
		{
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("getSignatureMethod", "xml.Signature", false, "Returns the algorithm method of the signature.", new List<ParameterResult>
		{
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("createReference", "xml.Signature", false, "Creates a new reference that will be signed with the compute() method", new List<ParameterResult>
		{
			new ParameterResult("uri", "", "string"),
			new ParameterResult("digest", "", "string")
		}, new List<string> {"int"}),
		new GeneroPackageClassMethod("setReferenceID", "xml.Signature", false, "Sets an ID value for the signature reference of index ind.", new List<ParameterResult>
		{
			new ParameterResult("int", "", "int"),
			new ParameterResult("value", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("appendReferenceTransformation", "xml.Signature", false, "Appends a transformation related to the reference of index ind, and is executed before any computation", new List<ParameterResult>
		{
			new ParameterResult("int", "", "int"),
			new ParameterResult("trans", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("getReferenceCount", "xml.Signature", false, "Returns the number of references in this Signature object.", new List<ParameterResult>
		{
		}, new List<string> {"int"}),
		new GeneroPackageClassMethod("getReferenceURI", "xml.Signature", false, "Returns the URI of the reference of index ind in this Signature object.", new List<ParameterResult>
		{
			new ParameterResult("ind", "", "int")
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("getReferenceDigest", "xml.Signature", false, "Returns the digest algorithm identifier of the reference of index ind in this Signature object.", new List<ParameterResult>
		{
			new ParameterResult("ind", "", "int")
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("getReferenceID", "xml.Signature", false, "Returns the ID value of the reference of index ind in this Signature object, or NULL if there is none.", new List<ParameterResult>
		{
			new ParameterResult("ind", "", "int")
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("getReferenceTransformationCount", "xml.Signature", false, "Returns the number of transformation related to the reference of index ind.", new List<ParameterResult>
		{
			new ParameterResult("ind", "", "int")
		}, new List<string> {"int"}),
		new GeneroPackageClassMethod("getReferenceTransformation", "xml.Signature", false, "Gets the transformation identifier related to the reference of index ind at position pos in the list of transformation.", new List<ParameterResult>
		{
			new ParameterResult("ind", "", "int"),
			new ParameterResult("pos", "", "int")
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("createObject", "xml.Signature", false, "Creates a new object that will embed additional XML nodes.", new List<ParameterResult>
		{
		}, new List<string> {"int"}),
		new GeneroPackageClassMethod("setObjectID", "xml.Signature", false, "Sets an ID value for the signature object of index ind.", new List<ParameterResult>
		{
			new ParameterResult("ind", "", "int")
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("appendObjectData", "xml.Signature", false, "Appends a copy of a XML node node to the signature object of index ind.", new List<ParameterResult>
		{
			new ParameterResult("ind", "", "int")
		}, new List<string> {"xml.DomNode"}),
		new GeneroPackageClassMethod("getObjectCount", "xml.Signature", false, "Returns the number of objects in this Signature object.", new List<ParameterResult>
		{
		}, new List<string> {"int"}),
		new GeneroPackageClassMethod("getObjectID", "xml.Signature", false, "Returns the ID value of the signature object of index ind in this Signature object.", new List<ParameterResult>
		{
			new ParameterResult("ind", "", "int")
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("compute", "xml.Signature", false, "Computes the signature of all references set in this Signature object.", new List<ParameterResult>
		{
			new ParameterResult("doc", "", "xml.DomDocument")
		}, new List<string> {}),
		new GeneroPackageClassMethod("verify", "xml.Signature", false, "Verifies whether all references in this Signature object haven't changed.", new List<ParameterResult>
		{
			new ParameterResult("doc", "", "xml.DomDocument")
		}, new List<string> {"int"}),
		new GeneroPackageClassMethod("signString", "xml.Signature", false, "Sign the passed string according to the specified key.", new List<ParameterResult>
		{
			new ParameterResult("key", "", "xml.CryptoKey"),
			new ParameterResult("strToSign", "", "string")
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("verifyString", "xml.Signature", false, "Verify the signature is consistent with the given key and the original message.", new List<ParameterResult>
		{
			new ParameterResult("key", "", "xml.CryptoKey"),
			new ParameterResult("signedStr", "", "string"),
			new ParameterResult("signature", "", "string")
		}, new List<string> {"int"})
	}),
	new GeneroPackageClass("Encryption", "xml", false, new List<GeneroPackageClassMethod>
	{
		new GeneroPackageClassMethod("Create", "xml.Encryption", true, "Constructor of an Encryption object.", new List<ParameterResult>
		{
		}, new List<string> {"xml.Encryption"}),
		new GeneroPackageClassMethod("EncryptString", "xml.Encryption", true, "Encrypts the string str using the symmetric key key, and returns the encrypted string encoded in BASE64.", new List<ParameterResult>
		{
			new ParameterResult("key", "", "xml.CryptoKey"),
			new ParameterResult("str", "", "string")
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("DecryptString", "xml.Encryption", true, "Decrypts the encrypted string str encoded in BASE64, using the symmetric key key, and returns the string in clear text.", new List<ParameterResult>
		{
			new ParameterResult("key", "", "xml.CryptoKey"),
			new ParameterResult("str", "", "string")
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("RSAEncrypt", "xml.Encryption", true, "Encrypts the string str using the RSA key key and returns it encoded in BASE64.", new List<ParameterResult>
		{
			new ParameterResult("key", "", "string"),
			new ParameterResult("str", "", "string")
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("RSADecrypt", "xml.Encryption", true, "Decrypts the BASE64 encrypted string enc using the RSA key key and returns it in clear text", new List<ParameterResult>
		{
			new ParameterResult("key", "", "string"),
			new ParameterResult("str", "", "string")
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("setKey", "xml.Encryption", false, "Assigns a copy of the symmetric key to this Encryption object.", new List<ParameterResult>
		{
			new ParameterResult("key", "", "xml.CryptoKey")
		}, new List<string> {}),
		new GeneroPackageClassMethod("getEmbeddedKey", "xml.Encryption", false, "Get a copy of the embedded symmetric key that was used in the last decryption operation.", new List<ParameterResult>
		{
		}, new List<string> {"xml.CryptoKey"}),
		new GeneroPackageClassMethod("setKeyEncryptionKey", "xml.Encryption", false, "Assigns a copy of the key-encryption key to this Encryption object.", new List<ParameterResult>
		{
			new ParameterResult("key", "", "xml.CryptoKey")
		}, new List<string> {}),
		new GeneroPackageClassMethod("setCertificate", "xml.Encryption", false, "Assigns a copy of the X509 certificate to this Encryption object.", new List<ParameterResult>
		{
			new ParameterResult("cert", "", "xml.CryptoX509")
		}, new List<string> {}),
		new GeneroPackageClassMethod("encryptElement", "xml.Encryption", false, "Encrypts the ELEMENT DomNode node and all its children using the symmetric key.", new List<ParameterResult>
		{
			new ParameterResult("node", "", "xml.DomNode")
		}, new List<string> {}),
		new GeneroPackageClassMethod("decryptElement", "xml.Encryption", false, "Decrypts the EncryptedData DomNode enc using the symmetric key.", new List<ParameterResult>
		{
			new ParameterResult("enc", "", "xml.DomNode")
		}, new List<string> {}),
		new GeneroPackageClassMethod("encryptElementContent", "xml.Encryption", false, "Encrypts all child nodes of the ELEMENT DomNode node using the symmetric key.", new List<ParameterResult>
		{
			new ParameterResult("node", "", "xml.DomNode")
		}, new List<string> {}),
		new GeneroPackageClassMethod("decryptElementContent", "xml.Encryption", false, "Decrypts the EncryptedData DomNode enc using the symmetric key.", new List<ParameterResult>
		{
			new ParameterResult("enc", "", "xml.DomNode")
		}, new List<string> {}),
		new GeneroPackageClassMethod("encryptElementDetatched", "xml.Encryption", false, "Encrypts the ELEMENT DomNode node and all its children using the symmetric key, and returns them as one new EncryptedData node.", new List<ParameterResult>
		{
			new ParameterResult("node", "", "xml.DomNode")
		}, new List<string> {"xml.DomNode"}),
		new GeneroPackageClassMethod("decryptElementDetatched", "xml.Encryption", false, "Decrypts the EncryptedData DomNode enc using the symmetric key, and returns it in a new ELEMENT node", new List<ParameterResult>
		{
			new ParameterResult("enc", "", "xml.DomNode")
		}, new List<string> {"xml.DomNode"}),
		new GeneroPackageClassMethod("encryptElementContentDetatched", "xml.Encryption", false, "Encrypts all child nodes of the ELEMENT DomNode node using the symmetric key, and returns them as one new EncryptedData node.", new List<ParameterResult>
		{
			new ParameterResult("node", "", "xml.DomNode")
		}, new List<string> {"xml.DomNode"}),
		new GeneroPackageClassMethod("decryptElementContentDetatched", "xml.Encryption", false, "Decrypts the EncryptedData DomNode enc using the symmetric key, and returns all its children in one new DOCUMENT_FRAGMENT_NODE node.", new List<ParameterResult>
		{
			new ParameterResult("enc", "", "xml.DomNode")
		}, new List<string> {"xml.DomNode"}),
		new GeneroPackageClassMethod("encryptKey", "xml.Encryption", false, "Encrypts the given symmetric or HMAC key as an EncryptedKey node and returns it as root node of a new XML document.", new List<ParameterResult>
		{
			new ParameterResult("key", "", "xml.CryptoKey")
		}, new List<string> {"xml.DomDocument"}),
		new GeneroPackageClassMethod("decryptKey", "xml.Encryption", false, "Decrypts the EncryptedKey as root in the given XML document, and returns a new CryptoKey of the given kind.", new List<ParameterResult>
		{
			new ParameterResult("xml", "", "xml.DomDocument"),
			new ParameterResult("url", "", "string")
		}, new List<string> {"xml.CryptoKey"})
	}),
	new GeneroPackageClass("KeyStore", "xml", true, new List<GeneroPackageClassMethod>
	{
		new GeneroPackageClassMethod("AddTrustedCertificate", "xml.KeyStore", true, "Registers the given X509 certificate as a trusted certificate for the application. It will be used for signature verification if no other certificate was set for that purpose.", new List<ParameterResult>
		{
			new ParameterResult("cert", "", "xml.CryptoX509")
		}, new List<string> {}),
		new GeneroPackageClassMethod("AddCertificate", "xml.KeyStore", true, "Registers the given X509 certificate as a certificate for the application. It will be used when an incomplete X509 certificate is detected during signature or encryption to complete the process by checking the certificate issuer name and serial number.", new List<ParameterResult>
		{
			new ParameterResult("cert", "", "xml.CryptoX509")
		}, new List<string> {}),
		new GeneroPackageClassMethod("AddKey", "xml.KeyStore", true, "Registers the given key by name to the application. It is used for XML signature verification or XML decryption when a key name was specified in the XML KeyInfo node and no other key was set in the Signature or Encryption object.", new List<ParameterResult>
		{
			new ParameterResult("key", "", "xml.CryptoX509")
		}, new List<string> {})
	})
}));
                    Packages.Add("security", new GeneroPackage("security", true, new List<GeneroPackageClass>
{
	new GeneroPackageClass("RandomGenerator", "security", true, new List<GeneroPackageClassMethod>
	{
		new GeneroPackageClassMethod("CreateUUIDString", "security.RandomGenerator", true, "Creates a new universal unique identifier (UUID).", new List<ParameterResult>
		{
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("CreateRandomString", "security.RandomGenerator", true, "Creates a random base64 string.", new List<ParameterResult>
		{
			new ParameterResult("size", "", "int")
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("CreateRandomNumber", "security.RandomGenerator", true, "Generates a 8-byte strong random number.", new List<ParameterResult>
		{
		}, new List<string> {"bigint"})
	}),
	new GeneroPackageClass("Base64", "security", true, new List<GeneroPackageClassMethod>
	{
		new GeneroPackageClassMethod("ToString", "security.Base64", true, "Decodes the given base64 string.", new List<ParameterResult>
		{
			new ParameterResult("source", "", "string")
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("FromString", "security.Base64", true, "Encodes the given string in base64.", new List<ParameterResult>
		{
			new ParameterResult("source", "", "string")
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("ToHexBinary", "security.Base64", true, "Decodes the given base64 string to hexadecimal.", new List<ParameterResult>
		{
			new ParameterResult("source", "", "string")
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("FromHexBinary", "security.Base64", true, "Decodes the given hexadecimal string to base64.", new List<ParameterResult>
		{
			new ParameterResult("source", "", "string")
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("Xor", "security.Base64", true, "Computes the exclusive disjunction between two base64 encoded strings.", new List<ParameterResult>
		{
			new ParameterResult("b64str1", "", "string"),
			new ParameterResult("b64str2", "", "string")
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("LoadBinary", "security.Base64", true, "Reads data from a file and encodes to base64.", new List<ParameterResult>
		{
			new ParameterResult("path", "", "string")
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("SaveBinary", "security.Base64", true, "Decodes the given base64 string and writes the data to a file.", new List<ParameterResult>
		{
			new ParameterResult("path", "", "string"),
			new ParameterResult("data", "", "string")
		}, new List<string> {})
	}),
	new GeneroPackageClass("HexBinary", "security", true, new List<GeneroPackageClassMethod>
	{
		new GeneroPackageClassMethod("ToString", "security.HexBinary", true, "Decodes an hexadecimal string to a clear, human-readable string.", new List<ParameterResult>
		{
			new ParameterResult("source", "", "string")
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("FromString", "security.HexBinary", true, "Encodes a given string in hexadecimal.", new List<ParameterResult>
		{
			new ParameterResult("source", "", "string")
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("ToBase64", "security.HexBinary", true, "Converts an hexadecimal string to the base64 equivalent", new List<ParameterResult>
		{
			new ParameterResult("source", "", "string")
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("FromBase64", "security.HexBinary", true, "Converts a base64 string to the hexadecimal equivalent.", new List<ParameterResult>
		{
			new ParameterResult("source", "", "string")
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("Xor", "security.HexBinary", true, "Computes the exclusive disjunction between two hexadecimal encoded strings.", new List<ParameterResult>
		{
			new ParameterResult("hexstr1", "", "string"),
			new ParameterResult("hexstr2", "", "string")
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("LoadBinary", "security.HexBinary", true, "Reads binary data from a file and converts it to hexadecimal.", new List<ParameterResult>
		{
			new ParameterResult("path", "", "string")
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("SaveBinary", "security.HexBinary", true, "Reads binary data from a file and converts it to hexadecimal.", new List<ParameterResult>
		{
			new ParameterResult("path", "", "string"),
			new ParameterResult("data", "", "string")
		}, new List<string> {})
	}),
	new GeneroPackageClass("Digest", "security", true, new List<GeneroPackageClassMethod>
	{
		new GeneroPackageClassMethod("CreateDigest", "security.Digest", true, "Defines a new digest context by specifying the algorithm to be used.", new List<ParameterResult>
		{
			new ParameterResult("algo", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("AddData", "security.Digest", true, "Adds a data from a BYTE variable to the digest buffer.", new List<ParameterResult>
		{
			new ParameterResult("data", "", "byte")
		}, new List<string> {}),
		new GeneroPackageClassMethod("AddBase64Data", "security.Digest", true, "Adds a data in base64 format to the digest buffer.", new List<ParameterResult>
		{
			new ParameterResult("data", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("AddHexBinaryData", "security.Digest", true, "Adds a data in hexadecimal format to the digest buffer.", new List<ParameterResult>
		{
			new ParameterResult("data", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("AddStringData", "security.Digest", true, "Adds a data string to the digest buffer.", new List<ParameterResult>
		{
			new ParameterResult("data", "", "string")
		}, new List<string> {}),
		new GeneroPackageClassMethod("DoBase64Digest", "security.Digest", true, "Creates a digest of the buffered data and returns the result in base64 format.", new List<ParameterResult>
		{
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("DoHexBinaryDigest", "security.Digest", true, "Creates a digest of the buffered data and returns the result in hexadecimal format.", new List<ParameterResult>
		{
		}, new List<string> {"string"}),
		new GeneroPackageClassMethod("CreateDigestString", "security.Digest", true, "Creates a SHA1 digest from the given string.", new List<ParameterResult>
		{
			new ParameterResult("password", "", "string"),
			new ParameterResult("randBase64", "", "string")
		}, new List<string> {"string"})
	})
}));
                    #endregion
                    _packagesInitialized = true;
                }
            }
        }
    }

    public class GeneroPackage : IAnalysisResult
    {
        private readonly string _name;
        private readonly bool _extensionPackage;
        public bool ExtensionPackage
        {
            get { return _extensionPackage; }
        }
        private readonly Dictionary<string, GeneroPackageClass> _classes;

        public bool CanGetValueFromDebugger
        {
            get { return false; }
        }

        public bool IsPublic { get { return true; } }

        public GeneroPackage(string name, bool extension, IEnumerable<GeneroPackageClass> classes)
        {
            _name = name;
            _extensionPackage = extension;
            _classes = new Dictionary<string, GeneroPackageClass>(StringComparer.OrdinalIgnoreCase);
            foreach (var cls in classes)
                _classes.Add(cls.Name, cls);
        }

        private const string _scope = "package";
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

        public string Namespace { get { return null; } }

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

        public LocationInfo Location { get { return null; } }

        public IAnalysisResult GetMember(string name, GeneroAst ast, out IGeneroProject definingProject, out IProjectEntry projEntry, bool function)
        {
            definingProject = null;
            projEntry = null;
            GeneroPackageClass cls = null;
            _classes.TryGetValue(name, out cls);
            return cls;
        }

        public IEnumerable<MemberResult> GetMembers(GeneroAst ast, MemberType memberType, bool function)
        {
            return _classes.Values.Select(x => new MemberResult(x.Name, x, GeneroMemberType.Class, ast));
        }

        public bool HasChildFunctions(GeneroAst ast)
        {
            return true;
        }

        public bool ContainsInstanceMembers
        {
            get
            {
                return _classes.Values.Any(x => !x.IsStatic);
            }
        }

        public bool ContainsStaticClasses
        {
            get
            {
                return _classes.Values.Any(x => x.IsStatic);
            }
        }

        public string Typename
        {
            get { return null; }
        }
    }

    public class GeneroPackageClass : IAnalysisResult
    {
        private readonly string _parentPackage;
        private readonly string _name;
        private readonly bool _isStatic;
        public bool IsStatic { get { return _isStatic; } }
        private readonly Dictionary<string, GeneroPackageClassMethod> _methods;

        public bool CanGetValueFromDebugger
        {
            get { return false; }
        }

        public bool IsPublic { get { return true; } }

        public GeneroPackageClass(string name, string parentPackage, bool isStatic, IEnumerable<GeneroPackageClassMethod> methods)
        {
            _name = name;
            _parentPackage = parentPackage;
            _isStatic = isStatic;
            _methods = new Dictionary<string, GeneroPackageClassMethod>(StringComparer.OrdinalIgnoreCase);
            foreach (var method in methods)
                _methods.Add(method.Name, method);
        }

        private const string _scope = "package class";
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

        public string Namespace { get { return _parentPackage; } }

        public string Documentation
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                if (!string.IsNullOrWhiteSpace(Scope))
                {
                    sb.AppendFormat("({0}) ", Scope);
                }
                sb.AppendFormat("{0}.{1}", Namespace, Name);
                return sb.ToString();
            }
        }

        public int LocationIndex
        {
            get { return -1; }
        }

        public LocationInfo Location { get { return null; } }

        public IAnalysisResult GetMember(string name, GeneroAst ast, out IGeneroProject definingProject, out IProjectEntry projEntry, bool function)
        {
            definingProject = null;
            projEntry = null;
            GeneroPackageClassMethod method = null;
            _methods.TryGetValue(name, out method);
            return method;
        }

        public IEnumerable<MemberResult> GetMembers(GeneroAst ast, MemberType memberType, bool function)
        {
            return _methods.Values.Select(x => new MemberResult(x.Name, x, GeneroMemberType.Method, ast));
        }

        public bool HasChildFunctions(GeneroAst ast)
        {
            return true;
        }

        public string Typename
        {
            get { return null; }
        }
    }

    public class GeneroPackageClassMethod : IFunctionResult
    {
        private readonly string _name;
        private readonly string _parentClass;
        private readonly bool _isStatic;
        public bool IsStatic { get { return _isStatic; } }
        private readonly string _desc;
        private readonly List<ParameterResult> _parameters;
        private readonly List<string> _returns;

        public bool IsPublic { get { return true; } }

        public bool CanGetValueFromDebugger
        {
            get { return false; }
        }

        public GeneroPackageClassMethod(string name, string parentClass, bool isStatic, string description, IEnumerable<ParameterResult> parameters, IEnumerable<string> returns)
        {
            _name = name;
            _parentClass = parentClass;
            _isStatic = isStatic;
            _desc = description;
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
            get { return _desc; }
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

        private const string _scope = "package class method";
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

                if(_returns.Count == 0)
                {
                    sb.Append("void ");
                }
                else if(_returns.Count == 1)
                {
                    sb.AppendFormat("{0} ", _returns[0]);
                }


                sb.AppendFormat("{0}.{1}", _parentClass, Name);
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

                if (_returns.Count > 1)
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

        public LocationInfo Location { get { return null; } }

        public IAnalysisResult GetMember(string name, GeneroAst ast, out IGeneroProject definingProject, out IProjectEntry projEntry, bool function)
        {
            definingProject = null;
            projEntry = null;
            if (_returns != null && _returns.Count == 1)
            {
                var typeRef = new TypeReference(_returns[0]);
                return typeRef.GetMember(name, ast, out definingProject, out projEntry, function);
            }
            return null;
        }

        public IEnumerable<MemberResult> GetMembers(GeneroAst ast, MemberType memberType, bool function)
        {
            if(_returns != null && _returns.Count == 1)
            {
                var typeRef = new TypeReference(_returns[0]);
                return typeRef.GetMembers(ast, memberType, function);
            }
            return new MemberResult[0];
        }

        public bool HasChildFunctions(GeneroAst ast)
        {
            return false;
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

        public string CompletionParentName
        {
            get { return null; }
        }


        public int DecoratorStart
        {
            get
            {
                return StartIndex;
            }
            set
            {
            }
        }

        public string Typename
        {
            get 
            {
                if (_returns != null && _returns.Count == 1)
                {
                    var typeRef = new TypeReference(_returns[0]);
                    return typeRef.ToString();
                }
                return null;
            }
        }


        public string[] Returns
        {
            get { return _returns.ToArray(); }
        }

        private Dictionary<string, List<Tuple<IAnalysisResult, IndexSpan>>> _dummyLimitDict = new Dictionary<string, List<Tuple<IAnalysisResult, IndexSpan>>>();
        public IDictionary<string, List<Tuple<IAnalysisResult, IndexSpan>>> LimitedScopeVariables
        {
            get { return _dummyLimitDict; }
        }
    }
}
