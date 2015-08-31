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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSGenero.Analysis;
using VSGenero.Analysis.Parsing;
using VSGenero.Analysis.Parsing.AST_4GL;

namespace VSGeneroUnitTesting
{
    [TestClass]
    public class ParserTester
    {
        private ParserOptions po;
        private TestErrorSink _errorSink;

        public ParserTester()
        {
            po = new ParserOptions();
            po.ErrorSink = _errorSink = new TestErrorSink();
        }

        [TestMethod]
        public void HardcodedParserTest()
        {
            //string codeSample = "globals\n\tdefine x, y record\n\tz smallint, a int\nend record\nend globals";
            //string codeSample = "globals\n\tdefine x, y record like tablename.*\nend globals";
            //string codeSample = "globals\n\tdefine x, y record like tablename.*\nend globals\n\n\nmain\n\tdefine x, y smallint\n\nend main";
            string codeSample = "globals\n\tdefine x, y like tablename.*\nend globals\n\n\nmain\n\tdefine x, y smallint\n\nend main";

            using (TextReader tr = new StringReader(codeSample))
            {
                Genero4glParser p = Genero4glParser.CreateParser(tr, po);
                var node = p.ParseFile();
                int i = 0;
            }
        }

        [TestMethod]
        public void EmptyFileTest()
        {
            string path = @"..\..\ParserTests\EmptyFile.4gl";
            using (TextReader tr = new StreamReader(path))
            {
                Genero4glParser p = Genero4glParser.CreateParser(tr, po);
                var node = (Genero4glAst)p.ParseFile();
                Assert.IsNotNull(node);
                Assert.IsTrue(_errorSink.Errors.Count == 0);
                Assert.IsTrue(node.Body.Children.Count == 0);
            }
        }

        [TestMethod]
        public void TestFile1()
        {
            string path = @"..\..\ParserTests\TestFile1.4gl";
            using (TextReader tr = new StreamReader(path))
            {
                Genero4glParser p = Genero4glParser.CreateParser(tr, po);
                var node = p.ParseFile();
                int i = 0;
            }
        }

        [TestMethod]
        public void RealLife1()
        {
            string path = @"..\..\ParserTests\RealLife1.4gl";
            using (TextReader tr = new StreamReader(path))
            {
                Genero4glParser p = Genero4glParser.CreateParser(tr, po);
                var node = p.ParseFile();
                int i = 0;
            }
        }

        [TestMethod]
        public void RealLife2()
        {
            string path = @"..\..\ParserTests\RealLife2.4gl";
            using (TextReader tr = new StreamReader(path))
            {
                Genero4glParser p = Genero4glParser.CreateParser(tr, po);
                var node = p.ParseFile();
                int i = 0;
            }
        }
    }

    public class TestErrorSink : ErrorSink
    {
        private List<TestError> _errors;
        public List<TestError> Errors
        {
            get
            {
                if (_errors == null)
                    _errors = new List<TestError>();
                return _errors;
            }
        }

        public override void Add(string message, int[] lineLocations, int startIndex, int endIndex, int errorCode, Severity severity)
        {
            Errors.Add(new TestError { Message = message, LineLocations = lineLocations, StartIndex = startIndex, EndIndex = endIndex, ErrorCode = errorCode, Severity = severity });
        }
    }

    public struct TestError
    {
        public string Message { get; set; }
        public int[] LineLocations { get; set; }
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
        public int ErrorCode { get; set; }
        public Severity Severity { get; set; }
    }
}
