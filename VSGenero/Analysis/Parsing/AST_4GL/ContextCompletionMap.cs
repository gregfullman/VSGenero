using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Net;

namespace VSGenero.Analysis.Parsing.AST_4GL
{
    public class ContextCompletionMap
    {
        private object _initializeLock = new object();
        private const string _githubFile = @"https://gitcdn.xyz/repo/gregfullman/VSGenero/master/VSGenero/CompletionContexts.xml";
        private readonly Dictionary<object, IEnumerable<ContextPossibilities>> _contextMap;

        private ContextCompletionMap()
        {
            _contextMap = new Dictionary<object, IEnumerable<ContextPossibilities>>();
        }

        private static ContextCompletionMap _instance;
        public static ContextCompletionMap Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ContextCompletionMap();
                return _instance;
            }
        }

        public void Initialize(object state)
        {
            lock(_initializeLock)
            {
                bool getAssemblyFile = false;
                if(File.Exists(Filename))
                {
                    // Make sure it's the correct file
                    var firstLine = File.ReadLines(Filename).First();
                    if (!firstLine.Contains(string.Format("Version=\"{0}", VSGeneroPackage.Instance.ProductVersion.ToString())))
                        getAssemblyFile = true;
                }

                // First, check to see if the file exists.
                if(getAssemblyFile || !File.Exists(Filename))
                {
                    if (!Directory.Exists(VSGeneroPackage.Instance.SettingsDirectory))
                        Directory.CreateDirectory(VSGeneroPackage.Instance.SettingsDirectory);
                    // Copy the embedded resource file into the correct location
                    using (var embeddedFile = typeof(ContextCompletionMap).Assembly.GetManifestResourceStream("VSGenero.CompletionContexts.xml"))
                    using (var file = new FileStream(Filename, FileMode.Create, FileAccess.Write))
                    {
                        embeddedFile.CopyTo(file);
                    }
                }

                LoadFromXML();
            }
        }

        internal void Add(object key, IEnumerable<ContextPossibilities> contextPossibilities)
        {
            _contextMap.Add(key, contextPossibilities);
        }

        internal bool TryGetValue(object key, out IEnumerable<ContextPossibilities> value)
        {
            return _contextMap.TryGetValue(key, out value);
        }

        private string _filename;
        internal string Filename
        {
            get
            {
                if (_filename == null)
                    _filename = Path.Combine(VSGeneroPackage.Instance.SettingsDirectory, "CompletionContexts.xml");
                return _filename;
            }
        }

        internal async Task<bool> DownloadLatestFile()
        {
            WebClient client = new WebClient();
            try
            {
                await client.DownloadFileTaskAsync(new Uri(_githubFile), Filename);
                await Task.Factory.StartNew(() => Initialize(null));
                return await Task.Factory.StartNew<bool>(() => LoadFromXML());
            }
            catch (Exception e)
            {
                return false;
            }
        }

        internal bool LoadFromXML()
        {
            if(File.Exists(Filename))
            {
                _contextMap.Clear();
                XmlDocument contextXml = new XmlDocument();
                contextXml.Load(Filename);
                foreach(XmlNode contextEntry in contextXml.LastChild.ChildNodes)
                {
                    if (contextEntry.Name == "ContextEntry")
                    {
                        // Get the entry key
                        object key = null;
                        if(contextEntry.Attributes != null && contextEntry.Attributes["Type"].Value == "keyword")
                        {
                            var token = Tokens.GetToken(contextEntry.Attributes["Entry"].Value);
                            if(token != null)
                            {
                                key = token.Kind;
                            }
                            else
                            {
                                key = Enum.Parse(typeof(TokenKind), contextEntry.Attributes["Entry"].Value);
                            }
                        }
                        else if(contextEntry.Attributes != null && contextEntry.Attributes["Type"].Value == "category")
                        {
                            key = Enum.Parse(typeof(TokenCategory), contextEntry.Attributes["Entry"].Value);
                        }
                        else
                        {
                            return false;
                        }

                        List<ContextPossibilities> possibilities = new List<ContextPossibilities>();
                        foreach(XmlNode contextPossibility in contextEntry.ChildNodes)
                        {
                            var singleTokens = new List<TokenKind>();
                            var setProviders = new List<ContextSetProviderContainer>();
                            var backwardItems = new List<BackwardTokenSearchItem>();

                            foreach(XmlNode possibility in contextPossibility.ChildNodes)
                            {
                                switch(possibility.Name)
                                {
                                    case "SingleTokens":
                                        {
                                            foreach(XmlNode tokenNode in possibility.ChildNodes)
                                            {
                                                TokenKind tokenKind;
                                                var token = Tokens.GetToken(tokenNode.InnerText);
                                                if (token != null)
                                                {
                                                    tokenKind = token.Kind;
                                                }
                                                else
                                                {
                                                    tokenKind = (TokenKind)Enum.Parse(typeof(TokenKind), tokenNode.InnerText);
                                                }
                                                singleTokens.Add(tokenKind);
                                            }
                                        }
                                        break;
                                    case "ContextSetProviders":
                                        {
                                            foreach(XmlNode setProviderNode in possibility.ChildNodes)
                                            {
                                                MethodInfo provider = typeof(Genero4glAst).GetMethod(setProviderNode.InnerText, BindingFlags.NonPublic | BindingFlags.Static);
                                                if(provider != null)
                                                {
                                                    MemberType mt = MemberType.All;
                                                    var mta = provider.GetCustomAttributes(true).FirstOrDefault(x => x is MemberTypeAttribute);
                                                    if(mta != null)
                                                    {
                                                        mt = (mta as MemberTypeAttribute).MemberType;
                                                    }

                                                    var del = Delegate.CreateDelegate(typeof(ContextSetProvider), provider);
                                                    if(del != null)
                                                    {
                                                        setProviders.Add(new ContextSetProviderContainer
                                                        {
                                                            Provider = del as ContextSetProvider,
                                                            ReturningTypes = mt
                                                        });
                                                    }
                                                }
                                            }
                                        }
                                        break;
                                    case "BackwardSearchItems":
                                        {
                                            foreach(XmlNode backwardItemNode in possibility.ChildNodes)
                                            {
                                                if(backwardItemNode.Name == "BackwardTokenSearchItem" &&
                                                   backwardItemNode.ChildNodes.Count == 1)
                                                {
                                                    bool match = true;
                                                    if (backwardItemNode.Attributes != null && backwardItemNode.Attributes["Match"] != null)
                                                        match = bool.Parse(backwardItemNode.Attributes["Match"].Value);
                                                    XmlNode backwardItem = backwardItemNode.ChildNodes[0];
                                                    if(backwardItem.Name == "Token")
                                                    {
                                                        TokenKind tokenKind;
                                                        var token = Tokens.GetToken(backwardItem.InnerText);
                                                        if (token != null)
                                                        {
                                                            tokenKind = token.Kind;
                                                        }
                                                        else
                                                        {
                                                            tokenKind = (TokenKind)Enum.Parse(typeof(TokenKind), backwardItem.InnerText);
                                                        }
                                                        backwardItems.Add(new BackwardTokenSearchItem(tokenKind, match));
                                                    }
                                                    else if(backwardItem.Name == "OrderedTokenSet")
                                                    {
                                                        List<object> tokenSet = new List<object>();
                                                        foreach(XmlNode tokenSetItem in backwardItem.ChildNodes)
                                                        {
                                                            object tokenItem = null;
                                                            if (tokenSetItem.Attributes != null && tokenSetItem.Attributes["Type"].Value == "keyword")
                                                            {
                                                                var token = Tokens.GetToken(tokenSetItem.Attributes["Value"].Value);
                                                                if (token != null)
                                                                {
                                                                    tokenItem = token.Kind;
                                                                }
                                                                else
                                                                {
                                                                    tokenItem = Enum.Parse(typeof(TokenKind), tokenSetItem.Attributes["Value"].Value);
                                                                }
                                                            }
                                                            else if (tokenSetItem.Attributes != null && tokenSetItem.Attributes["Type"].Value == "category")
                                                            {
                                                                tokenItem = Enum.Parse(typeof(TokenCategory), tokenSetItem.Attributes["Value"].Value);
                                                            }
                                                            else
                                                            {
                                                                return false;
                                                            }

                                                            if(tokenItem != null)
                                                            {
                                                                tokenSet.Add(tokenItem);
                                                            }
                                                        }
                                                        backwardItems.Add(new BackwardTokenSearchItem(new OrderedTokenSet(tokenSet), match));
                                                    }
                                                }
                                            }
                                        }
                                        break;
                                }
                            }
                            possibilities.Add(new ContextPossibilities(singleTokens.ToArray(), setProviders.ToArray(), backwardItems.ToArray()));
                        }
                        // add to the dictionary
                        _contextMap.Add(key, possibilities);
                    }
                }
            }
            else
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// This function is primarily here for testing purposes and exporting the initial
        /// hardcoded map to xml.
        /// </summary>
        /// <param name="outputFile"></param>
        internal void OutputMapToXML(string outputFile)
        {
            XmlDocument contextXml = new XmlDocument();
            XmlElement rootElement;
            rootElement = contextXml.DocumentElement;
            if(rootElement == null)
            {
                rootElement = contextXml.CreateElement("ContextMap");
                contextXml.AppendChild(rootElement);
            }

            XmlElement contextEntry;
            foreach(var entry in _contextMap)
            {
                contextEntry = contextXml.CreateElement("ContextEntry");
                string name = null;
                string type = null;
                if(entry.Key is TokenKind)
                {
                    if(!Tokens.TokenKinds.TryGetValue((TokenKind)entry.Key, out name))
                    {
                        name = Enum.GetName(typeof(TokenKind), (TokenKind)entry.Key);
                    }
                    type = "keyword";
                }
                else
                {
                    name = Enum.GetName(typeof(TokenCategory), (TokenCategory)entry.Key);
                    type = "category";
                }
                contextEntry.Attributes.Append(CreateAttribute(contextXml, "Entry", name));
                contextEntry.Attributes.Append(CreateAttribute(contextXml, "Type", type));

                XmlElement contextPossibility;
                foreach(var possibility in entry.Value)
                {
                    contextPossibility = contextXml.CreateElement("ContextPossibility");

                    if(possibility.SingleTokens.Length > 0)
                    {
                        XmlElement singleTokens = contextXml.CreateElement("SingleTokens");
                        XmlElement singleToken;
                        foreach(var token in possibility.SingleTokens)
                        {
                            if (Tokens.TokenKinds.TryGetValue(token, out name))
                            {
                                singleToken = contextXml.CreateElement("Token");
                                singleToken.InnerText = name;
                                singleTokens.AppendChild(singleToken);
                            }
                        }
                        contextPossibility.AppendChild(singleTokens);
                    }

                    if(possibility.SetProviders.Length > 0)
                    {
                        XmlElement setProviders = contextXml.CreateElement("ContextSetProviders");
                        XmlElement setProvider;
                        foreach(var provider in possibility.SetProviders)
                        {
                            setProvider = contextXml.CreateElement("Provider");
                            setProvider.InnerText = provider.Provider.Method.Name;
                            setProviders.AppendChild(setProvider);
                        }
                        contextPossibility.AppendChild(setProviders);
                    }

                    if(possibility.BackwardSearchItems.Length > 0)
                    {
                        XmlElement backwardSearchItems = contextXml.CreateElement("BackwardSearchItems");
                        XmlElement backwardSearchItem;
                        foreach(var searchItem in possibility.BackwardSearchItems)
                        {
                            backwardSearchItem = contextXml.CreateElement("BackwardTokenSearchItem");
                            if(searchItem.SingleToken != null &&
                               searchItem.SingleToken is TokenKind &&
                               ((TokenKind)searchItem.SingleToken) != TokenKind.EndOfFile)
                            {
                                XmlElement searchItemToken = contextXml.CreateElement("Token");
                                if (Tokens.TokenKinds.TryGetValue((TokenKind)searchItem.SingleToken, out name))
                                {
                                    searchItemToken.InnerText = name;
                                }
                                else
                                {
                                    searchItemToken.InnerText = Enum.GetName(typeof(TokenKind), (TokenKind)searchItem.SingleToken);
                                }
                                backwardSearchItem.AppendChild(searchItemToken);
                            }
                            else if(searchItem.TokenSet != null)
                            {
                                XmlElement searchItemSet = contextXml.CreateElement("OrderedTokenSet");
                                XmlElement itemEle;
                                foreach(var item in searchItem.TokenSet.Set)
                                {
                                    itemEle = contextXml.CreateElement("Item");
                                    if (item is TokenKind)
                                    {
                                        if (!Tokens.TokenKinds.TryGetValue((TokenKind)item, out name))
                                        {
                                            name = Enum.GetName(typeof(TokenKind), (TokenKind)item);
                                        }
                                        type = "keyword";
                                    }
                                    else
                                    {
                                        name = Enum.GetName(typeof(TokenCategory), (TokenCategory)item);
                                        type = "category";
                                    }
                                    itemEle.Attributes.Append(CreateAttribute(contextXml, "Value", name));
                                    itemEle.Attributes.Append(CreateAttribute(contextXml, "Type", type));
                                    searchItemSet.AppendChild(itemEle);
                                }
                                backwardSearchItem.AppendChild(searchItemSet);
                            }
                            else
                            {
                                // BAD!!
                            }

                            backwardSearchItems.AppendChild(backwardSearchItem);
                        }
                        contextPossibility.AppendChild(backwardSearchItems);
                    }

                    contextEntry.AppendChild(contextPossibility);
                }
                rootElement.AppendChild(contextEntry);
            }
            contextXml.Save(outputFile);
        }

        private static XmlAttribute CreateAttribute(XmlDocument document, string name, string value)
        {
            var attrib = document.CreateAttribute(name);
            attrib.Value = value ?? "";
            return attrib;
        }
    }
}
