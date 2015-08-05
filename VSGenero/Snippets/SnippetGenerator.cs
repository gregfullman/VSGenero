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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace VSGenero.Snippets
{
    internal static class SnippetGenerator
    {
        internal static string GenerateSnippetXml(DynamicSnippet dynSnippet)
        {
            if(string.IsNullOrWhiteSpace(dynSnippet.Code) ||
               string.IsNullOrWhiteSpace(dynSnippet.Title) ||
               string.IsNullOrWhiteSpace(dynSnippet.Shortcut))
            {
                return null;
            }

            XmlDocument xmlDoc = new XmlDocument();
            var root = xmlDoc.CreateElement("CodeSnippets");
            xmlDoc.AppendChild(root);
            var codeSnippet = xmlDoc.CreateElement("CodeSnippet");
            var formatAttrib = xmlDoc.CreateAttribute("Format");
            formatAttrib.Value = "1.0.0";
            codeSnippet.Attributes.Append(formatAttrib);
            var header = xmlDoc.CreateElement("Header");
            var title = xmlDoc.CreateElement("Title");
            title.InnerText = dynSnippet.Title;
            var shortcut = xmlDoc.CreateElement("Shortcut");
            shortcut.InnerText = dynSnippet.Shortcut;
            var desc = xmlDoc.CreateElement("Description");
            desc.InnerText = dynSnippet.Description;
            var author = xmlDoc.CreateElement("Author");
            author.InnerText = dynSnippet.Author;

            var snipTypes = xmlDoc.CreateElement("SnippetTypes");
            var expType = xmlDoc.CreateElement("SnippetType");
            expType.InnerText = "Expansion";
            snipTypes.AppendChild(expType);

            header.AppendChild(title);
            header.AppendChild(shortcut);
            header.AppendChild(desc);
            header.AppendChild(author);
            header.AppendChild(snipTypes);
            codeSnippet.AppendChild(header);

            var snippet = xmlDoc.CreateElement("Snippet");
            var declars = xmlDoc.CreateElement("Declarations");
            foreach (var replacement in dynSnippet.Replacements)
            {
                
                var literal = xmlDoc.CreateElement("Literal");
                var id = xmlDoc.CreateElement("ID");
                id.InnerText = replacement.ID;
                var tooltip = xmlDoc.CreateElement("ToolTip");
                tooltip.InnerText = replacement.ToolTip;
                var def = xmlDoc.CreateElement("Default");
                def.InnerText = replacement.Default;
                literal.AppendChild(id);
                literal.AppendChild(tooltip);
                literal.AppendChild(def);
                declars.AppendChild(literal);
            }
            snippet.AppendChild(declars);

            var code = xmlDoc.CreateElement("Code");
            var langAttrib = xmlDoc.CreateAttribute("Language");
            langAttrib.Value = VSGeneroConstants.LanguageName4GL;
            code.Attributes.Append(langAttrib);
            var cdata = xmlDoc.CreateCDataSection(dynSnippet.Code);
            code.AppendChild(cdata);
            snippet.AppendChild(code);
            codeSnippet.AppendChild(snippet);
            root.AppendChild(codeSnippet);

            using (var stringWriter = new StringWriter())
            using (var xmlTextWriter = XmlWriter.Create(stringWriter))
            {
                xmlDoc.WriteTo(xmlTextWriter);
                xmlTextWriter.Flush();
                return stringWriter.GetStringBuilder().ToString();
            }
        }
    }
}
