using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSGenero.Analysis;

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
                VSGenero.Analysis.Parser p = VSGenero.Analysis.Parser.CreateParser(tr, po);
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
                VSGenero.Analysis.Parser p = VSGenero.Analysis.Parser.CreateParser(tr, po);
                var node = p.ParseFile();
                Assert.IsNotNull(node);
                Assert.IsTrue(_errorSink.Errors.Count == 0);
                Assert.IsTrue(node.Children.Count == 0);
            }
        }

        [TestMethod]
        public void TestFile1()
        {
            string path = @"..\..\ParserTests\TestFile1.4gl";
            using (TextReader tr = new StreamReader(path))
            {
                VSGenero.Analysis.Parser p = VSGenero.Analysis.Parser.CreateParser(tr, po);
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
                VSGenero.Analysis.Parser p = VSGenero.Analysis.Parser.CreateParser(tr, po);
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
