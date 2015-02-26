using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSGenero.Analysis;
using System.IO;

namespace VSGeneroTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TokenizerTest1()
        {
            Tokenizer t = new Tokenizer();

            using(TextReader tr = new StringReader("\"This is a multiline \n string\""))
            {
                t.Initialize(tr);
                var tokens = t.ReadTokens(int.MaxValue);
                Assert.IsTrue(tokens.Count == 4);
            }
        }
    }
}
