/* ****************************************************************************
 * 
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
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Text.RegularExpressions;
using System.Reflection;
using System.IO;

namespace VSGenero.EditorExtensions
{
    public abstract class GeneroComponentBase
    {
        public string Name { get; set; }
        public virtual string Description { get; set; }
    }

    public class GeneroClassMethodReturn : GeneroComponentBase
    {
        public string Type { get; set; }
    }

    public class GeneroClassMethodParameter : GeneroComponentBase
    {
        public string Type { get; set; }
    }

    public class GeneroClassMethod : GeneroComponentBase
    {
        public enum GeneroClassScope
        {
            Static,
            Instance
        }

        public GeneroClassScope Scope { get; set; }

        private Dictionary<string, GeneroClassMethodParameter> _params;
        public Dictionary<string, GeneroClassMethodParameter> Parameters
        {
            get
            {
                if (_params == null)
                    _params = new Dictionary<string, GeneroClassMethodParameter>();
                return _params;
            }
        }

        private Dictionary<string, GeneroClassMethodReturn> _returns;
        public Dictionary<string, GeneroClassMethodReturn> Returns
        {
            get
            {
                if (_returns == null)
                    _returns = new Dictionary<string, GeneroClassMethodReturn>();
                return _returns;
            }
        }

        private string _description;
        public override string Description
        {
            get
            {
                // ([returns]) [methodName]([params])
                // [description]
                StringBuilder sb = new StringBuilder();
                int total = Returns.Count;
                int i = 0;
                if (total > 1)
                {
                    sb.Append("(");
                }
                if (total == 0)
                {
                    sb.Append("void");
                }
                else
                {
                    foreach (var ret in Returns)
                    {
                        sb.Append(ret.Value.Type);
                        if (i + 1 < total)
                        {
                            sb.Append(", ");
                        }
                        i++;
                    }
                }
                if (total > 1)
                {
                    sb.Append(")");
                }
                sb.Append(" ");

                sb.Append(Name);
                sb.Append("(");
                total = Parameters.Count;
                i = 0;
                foreach (var param in Parameters)
                {
                    sb.Append(param.Value.Type + " " + param.Value.Name);
                    if (i + 1 < total)
                    {
                        sb.Append(", ");
                    }
                    i++;
                }
                sb.Append(")");
                if (!string.IsNullOrWhiteSpace(_description))
                {
                    sb.Append("\n");
                    sb.Append(_description);
                }
                return sb.ToString();
            }
            set
            {
                _description = value;
            }
        }
    }

    public class GeneroClass : GeneroComponentBase
    {
        public bool IsStatic { get; set; }

        private Dictionary<string, GeneroClassMethod> _methods;
        public Dictionary<string, GeneroClassMethod> Methods
        {
            get
            {
                if (_methods == null)
                    _methods = new Dictionary<string, GeneroClassMethod>();
                return _methods;
            }
        }
    }

    public class GeneroPackage : GeneroComponentBase
    {
        public enum GeneroPackageType
        {
            Builtin,
            Extension
        }

        public GeneroPackageType Type { get; set; }

        private Dictionary<string, GeneroClass> _classes;
        public Dictionary<string, GeneroClass> Classes
        {
            get
            {
                if (_classes == null)
                    _classes = new Dictionary<string, GeneroClass>();
                return _classes;
            }
        }
    }

    public class Genero4GL_XMLSettingsLoader : GeneroXMLSettingsLoader
    {
        private Dictionary<string, string> _symbolMap;
        public Dictionary<string, string> SymbolMap
        {
            get { return _symbolMap; }
        }

        private Dictionary<string, string> _keywordMap;
        public Dictionary<string, string> KeywordMap
        {
            get { return _keywordMap; }
        }

        private Dictionary<string, DataType> _dataTypeMap;
        public Dictionary<string, DataType> DataTypeMap
        {
            get { return _dataTypeMap; }
        }

        private Dictionary<string, GeneroPackage> _packages;
        public Dictionary<string, GeneroPackage> Packages
        {
            get
            {
                return _packages;
            }
        }

        public Genero4GL_XMLSettingsLoader() :
            base(Assembly.GetAssembly(typeof(Genero4GL_XMLSettingsLoader)).GetManifestResourceStream(@"VSGenero.Genero4GL.xml"))
        {
            _symbolMap = new Dictionary<string, string>();
            LoadSymbolMap();
            _keywordMap = new Dictionary<string, string>();
            LoadKeywordMap();
            _dataTypeMap = new Dictionary<string, DataType>();
            LoadDataTypes();
            _packages = new Dictionary<string, GeneroPackage>();
            LoadPackages();
        }

        private void LoadKeywordMap()
        {
            foreach(var element in GetElementsAtPath("//gns:Genero4GL/gns:Lexing/gns:Keywords/gns:Keyword"))
            {
                _keywordMap.Add((string)element.Attribute("name"), (string)element.Attribute("value"));
            }
        }

        private void LoadSymbolMap()
        {
            foreach (var element in GetElementsAtPath("//gns:Genero4GL/gns:Lexing/gns:Symbols/gns:Symbol"))
            {
                _symbolMap.Add((string)element.Attribute("name"), (string)element.Attribute("value"));
            }
        }

        private void LoadDataTypes()
        {
            foreach (var element in GetElementsAtPath("//gns:Genero4GL/gns:Parsing/gns:DataTypes/gns:DataType"))
            {
                List<DataType> tempList = new List<DataType>();
                tempList.Add(new DataType { Name = (string)element.Attribute("name") });

                // get the synonyms
                string synonym_csv = (string)element.Attribute("synonyms");
                if (synonym_csv != null)
                {
                    foreach (var name in synonym_csv.Split(new[] { ',' }))
                    {
                        tempList.Add(new DataType { Name = name });
                    }
                }

                bool dimReq = false;
                bool.TryParse((string)element.Attribute("dim_req"), out dimReq);
                bool rangeReq = false;
                bool.TryParse((string)element.Attribute("range_req"), out rangeReq);
                foreach (var dt in tempList)
                {
                    dt.DimensionRequired = dimReq;
                    dt.RangeRequired = rangeReq;

                    _dataTypeMap.Add(dt.Name, dt);
                }
            }
        }

        private void LoadPackages()
        {
            foreach (var element in GetElementsAtPath("//gns:Genero4GL/gns:Parsing/gns:Packages/gns:Package"))
            {
                // get the package info
                GeneroPackage newPackage = new GeneroPackage();
                newPackage.Name = (string)element.Attribute("name");
                newPackage.Type = ((string)element.Attribute("type") == "builtin") ?
                    GeneroPackage.GeneroPackageType.Builtin : GeneroPackage.GeneroPackageType.Extension;

                // get the classes within the package
                foreach (var classElement in element.XPathSelectElement("gns:Classes", _nsManager)
                                                    .XPathSelectElements("gns:Class", _nsManager))
                {
                    GeneroClass newClass = new GeneroClass();
                    newClass.Name = (string)classElement.Attribute("name");
                    newClass.IsStatic = (bool)classElement.Attribute("isStatic");

                    foreach (var methodElement in classElement.XPathSelectElement("gns:Methods", _nsManager)
                                                              .XPathSelectElements("gns:Method", _nsManager))
                    {
                        GeneroClassMethod newMethod = new GeneroClassMethod();
                        newMethod.Name = (string)methodElement.Attribute("name");
                        newMethod.Description = (string)methodElement.Attribute("desc");
                        newMethod.Scope = ((string)methodElement.Attribute("scope") == "static") ?
                            GeneroClassMethod.GeneroClassScope.Static : GeneroClassMethod.GeneroClassScope.Instance;

                        foreach (var paramElement in methodElement.XPathSelectElement("gns:Parameters", _nsManager)
                                                                  .XPathSelectElements("gns:Parameter", _nsManager))
                        {
                            GeneroClassMethodParameter newParam = new GeneroClassMethodParameter();
                            newParam.Name = (string)paramElement.Attribute("name");
                            newParam.Type = (string)paramElement.Attribute("type");
                            newMethod.Parameters.Add(newParam.Name, newParam);
                        }

                        foreach (var returnElement in methodElement.XPathSelectElement("gns:Returns", _nsManager)
                                                                   .XPathSelectElements("gns:Return", _nsManager))
                        {
                            GeneroClassMethodReturn newReturn = new GeneroClassMethodReturn();
                            newReturn.Name = (string)returnElement.Attribute("name");
                            newReturn.Type = (string)returnElement.Attribute("type");
                            newMethod.Returns.Add(newReturn.Name, newReturn);
                        }

                        newClass.Methods.Add(newMethod.Name, newMethod);
                    }

                    newPackage.Classes.Add(newClass.Name, newClass);
                }

                Packages.Add(newPackage.Name, newPackage);
            }
        }
    }

    public abstract class GeneroXMLSettingsLoader
    {
        private XDocument _document;
        protected XmlNamespaceManager _nsManager;

        public GeneroXMLSettingsLoader(Stream fileContents)
        {
            var xmlReader = XmlReader.Create(fileContents);
            _document = XDocument.Load(xmlReader);
            _nsManager = new XmlNamespaceManager(xmlReader.NameTable);
            _nsManager.AddNamespace("gns", "GeneroXML");
        }

        public IEnumerable<XElement> GetElementsAtPath(string path)
        {
            return _document.XPathSelectElements(path, _nsManager);
        }
    }
}
