using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Text.RegularExpressions;
using System.Reflection;
using System.IO;

namespace VSGeneroUnitTesting.Utilities
{
    [TestClass]
    public class BuiltinsGenerator
    {
        private readonly XmlReader _reader;
        private readonly XDocument _document;
        private readonly XmlNamespaceManager _nsManager;

        public BuiltinsGenerator()
        {
            FileStream fs = new FileStream("..\\..\\..\\VSGenero\\Genero4GL.xml", FileMode.Open);
            _reader = XmlReader.Create(fs);
            _document = XDocument.Load(_reader);
            _nsManager = new XmlNamespaceManager(_reader.NameTable);
            _nsManager.AddNamespace("gns", "GeneroXML");
        }

        private IEnumerable<XElement> GetElementsAtPath(string path)
        {
            return _document.XPathSelectElements(path, _nsManager);
        }

        [TestMethod]
        public void GenerateFunctions()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var element in GetElementsAtPath("//gns:Genero4GL/gns:Parsing/gns:Functions/gns:Context"))
            {
                string context = (string)element.Attribute("name");
                switch(context)
                {
                    case "system":
                        {
                            GenerateSystemFunctions(element, sb);
                        }
                        break;
                    case "array":
                        {
                            GenerateArrayFunctions(element, sb);
                        }
                        break;
                    case "string":
                        {
                            GenerateStringFunctions(element, sb);
                        }
                        break;
                    default:
                        break;
                }

            }

            // write the string to a file
            using(var file = new StreamWriter("builtinFunctions.txt"))
            {
                file.Write(sb.ToString());
            }
        }

        private void GenerateSystemFunctions(XElement element, StringBuilder sb)
        {
            foreach (var contextMethod in element.XPathSelectElements("gns:Function", _nsManager))
            {
                sb.AppendFormat("SystemFunctions.Add(\"{0}\", new BuiltinFunction(\"{0}\", new List<ParameterResult>\n{{", (string)contextMethod.Attribute("name"));
                foreach (var paramElement in contextMethod.XPathSelectElement("gns:Parameters", _nsManager)
                                                          .XPathSelectElements("gns:Parameter", _nsManager))
                {
                    sb.AppendFormat("new ParameterResult(\"{0}\", \"\", \"{1}\"),\n", (string)paramElement.Attribute("name"), (string)paramElement.Attribute("type"));
                }
                sb.AppendFormat("}}, new List<string> {{");
                foreach (var returnElement in contextMethod.XPathSelectElement("gns:Returns", _nsManager)
                                                           .XPathSelectElements("gns:Return", _nsManager))
                {
                    sb.AppendFormat("\"{0}\", ", (string)returnElement.Attribute("type"));
                }
                sb.AppendFormat("}},\n\"{0}\"));\n", (string)contextMethod.Attribute("description"));
            }
        }

        private void GenerateArrayFunctions(XElement element, StringBuilder sb)
        {
            foreach (var contextMethod in element.XPathSelectElements("gns:Function", _nsManager))
            {
                sb.AppendFormat("ArrayFunctions.Add(\"{0}\", new BuiltinFunction(\"{0}\", new List<ParameterResult>\n{{", (string)contextMethod.Attribute("name"));
                foreach (var paramElement in contextMethod.XPathSelectElement("gns:Parameters", _nsManager)
                                                          .XPathSelectElements("gns:Parameter", _nsManager))
                {
                    sb.AppendFormat("new ParameterResult(\"{0}\", \"\", \"{1}\"),\n", (string)paramElement.Attribute("name"), (string)paramElement.Attribute("type"));
                }
                sb.AppendFormat("}}, new List<string> {{");
                foreach (var returnElement in contextMethod.XPathSelectElement("gns:Returns", _nsManager)
                                                           .XPathSelectElements("gns:Return", _nsManager))
                {
                    sb.AppendFormat("\"{0}\", ", (string)returnElement.Attribute("type"));
                }
                sb.AppendFormat("}},\n\"{0}\"));\n", (string)contextMethod.Attribute("description"));
            }
        }

        private void GenerateStringFunctions(XElement element, StringBuilder sb)
        {
            foreach (var contextMethod in element.XPathSelectElements("gns:Function", _nsManager))
            {
                sb.AppendFormat("StringFunctions.Add(\"{0}\", new BuiltinFunction(\"{0}\", new List<ParameterResult>\n{{", (string)contextMethod.Attribute("name"));
                foreach (var paramElement in contextMethod.XPathSelectElement("gns:Parameters", _nsManager)
                                                          .XPathSelectElements("gns:Parameter", _nsManager))
                {
                    sb.AppendFormat("new ParameterResult(\"{0}\", \"\", \"{1}\"),\n", (string)paramElement.Attribute("name"), (string)paramElement.Attribute("type"));
                }
                sb.AppendFormat("}}, new List<string> {{");
                foreach (var returnElement in contextMethod.XPathSelectElement("gns:Returns", _nsManager)
                                                           .XPathSelectElements("gns:Return", _nsManager))
                {
                    sb.AppendFormat("\"{0}\", ", (string)returnElement.Attribute("type"));
                }
                sb.AppendFormat("}},\n\"{0}\"));\n", (string)contextMethod.Attribute("description"));
            }
        }

        [TestMethod]
        public void GeneratePackages()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("#region Generated Package Init Code");
            foreach (var element in GetElementsAtPath("//gns:Genero4GL/gns:Parsing/gns:Packages/gns:Package"))
            {
                GeneratePackage(element, sb);
            }
            sb.Append("#endregion");

            // write the string to a file
            using (var file = new StreamWriter("packages.txt"))
            {
                file.Write(sb.ToString());
            }
        }

        private void GeneratePackage(XElement element, StringBuilder sb)
        {
            sb.AppendFormat("Packages.Add(\"{0}\", new GeneroPackage(\"{0}\", new List<GeneroPackageClass>\n{{\n", (string)element.Attribute("name"));
            foreach (var classElement in element.XPathSelectElement("gns:Classes", _nsManager)
                                                .XPathSelectElements("gns:Class", _nsManager))
            {
                sb.AppendFormat("\tnew GeneroPackageClass(\"{0}\", {1}, new List<GeneroPackageClassMethod>\n\t{{\n", (string)classElement.Attribute("name"), ((bool)classElement.Attribute("isStatic") ? "true" : "false"));
                GeneratePackageClass(classElement, sb);
                sb.Append("\t}),\n");
            }
            sb.Remove(sb.Length - 2, 1);    // take out the comma
            sb.Append("}));\n");
        }

        private void GeneratePackageClass(XElement element, StringBuilder sb)
        {
            foreach (var methodElement in element.XPathSelectElement("gns:Methods", _nsManager)
                                                 .XPathSelectElements("gns:Method", _nsManager))
            {
                sb.AppendFormat("\t\tnew GeneroPackageClassMethod(\"{0}\", {1}, \"{2}\", new List<ParameterResult>\n\t\t{{\n",
                    (string)methodElement.Attribute("name"),
                    ((string)methodElement.Attribute("scope") == "static") ? "true" : "false",
                    ((string)methodElement.Attribute("desc")).Replace("\"", "\\\""));
                foreach (var paramElement in methodElement.XPathSelectElement("gns:Parameters", _nsManager)
                                                          .XPathSelectElements("gns:Parameter", _nsManager))
                {
                    sb.AppendFormat("\t\t\tnew ParameterResult(\"{0}\", \"\", \"{1}\"),\n", (string)paramElement.Attribute("name"), (string)paramElement.Attribute("type"));
                }
                if(sb[sb.Length - 2] == ',')
                    sb.Remove(sb.Length - 2, 1);    // take out the comma
                sb.AppendFormat("\t\t}}, new List<string> {{");
                foreach (var returnElement in methodElement.XPathSelectElement("gns:Returns", _nsManager)
                                                           .XPathSelectElements("gns:Return", _nsManager))
                {
                    sb.AppendFormat("\"{0}\", ", (string)returnElement.Attribute("type"));
                }
                if (sb[sb.Length - 2] == ',')
                    sb.Remove(sb.Length - 2, 2);    // take out the comma
                sb.Append("}),\n");
            }
            sb.Remove(sb.Length - 2, 1);    // take out the comma
        }
    }
}
