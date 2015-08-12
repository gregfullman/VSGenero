VSGenero
========

This is a Visual Studio extension for the Genero BDL. It supports Visual Studio 2013 and above.

The goal of this project is to provide an experience similar to the C# (or other .NET) editor in Visual Studio. It provides functionality such as syntax highlighting, function list (via standard editor dropdown), and intellisense features like typing auto-complete and auto-indent, function signature info, quick info (via hover-over), language snippets, and more. 

The VSGenero extension also exposes a number of APIs that allow for another Visual Studio extension to work hand-in-hand with it. These APIs can give access to global function repositories (for those using legacy linking), database information, importable modules and include files, and dynamic function snippet generation. This additional functionality all integrates seamlessly with VSGenero's intellisense engine.
