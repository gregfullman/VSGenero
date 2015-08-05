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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Data.Schema.ScriptDom.Sql;
using Microsoft.Data.Schema.ScriptDom;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Text;
using VSGenero.SqlSupport;
using VSGenero.Analysis;
using VSGenero.Analysis.Parsing;

namespace VSGeneroUnitTesting
{
    [TestClass]
    public class SqlExtraction
    {
        [TestMethod]
        public void SimpleStaticTest()
        {
            int count = 0;
            string text = "select * from testtable where txre_info = 3";
            foreach (var fragment in SqlStatementExtractor.ExtractStatements(text))
            {
                Assert.IsTrue(fragment != null);
                Assert.IsTrue(fragment.FragmentLength > 0);
                string txt = fragment.GetText();
                int i = 0;
                count++;
            }
            Assert.IsTrue(count == 1);
        }

        [TestMethod]
        public void SimpleDynamicTest()
        {
            int count = 0;
            string text = "\"select * from testtable where txre_info = 3\"";
            foreach (var fragment in SqlStatementExtractor.ExtractStatements(text))
            {
                Assert.IsTrue(fragment != null);
                Assert.IsTrue(fragment.FragmentLength > 0);
                string txt = fragment.GetText();
                int i = 0;
                count++;
            }
            Assert.IsTrue(count == 1);
        }

        [TestMethod]
        public void SimpleDynamicTestWithPlaceholder()
        {
            int count = 0;
            string text = "\"select * from testtable where txre_info = ?\"";
            foreach (var fragment in SqlStatementExtractor.ExtractStatements(text))
            {
                Assert.IsTrue(fragment != null);
                Assert.IsTrue(fragment.FragmentLength > 0);
                string txt = fragment.GetText();
                int i = 0;
                count++;
            }
            Assert.IsTrue(count == 1);
        }

        [TestMethod]
        public void SimpleIncompleteDynamicTest()
        {
            int count = 0;
            string text = "\"select * from testtable where txre_info = 3";
            foreach (var fragment in SqlStatementExtractor.ExtractStatements(text))
            {
                Assert.IsTrue(fragment != null);
                Assert.IsTrue(fragment.FragmentLength > 0);
                string txt = fragment.GetText();
                int i = 0;
                count++;
            }
            Assert.IsTrue(count == 1);
        }

        [TestMethod]
        public void EmbeddedStaticTest()
        {
            int count = 0;
            string text = "if(n == 1) then\nlet n = 2\nend if\nselect * from testtable where txre_info = 3\ndisplay record";
            foreach (var fragment in SqlStatementExtractor.ExtractStatements(text))
            {
                Assert.IsTrue(fragment != null);
                Assert.IsTrue(fragment.FragmentLength > 0);
                string txt = fragment.GetText();
                int i = 0;
                count++;
            }
            Assert.IsTrue(count == 1);
        }

        [TestMethod]
        public void EmbeddedMultilineStaticTest()
        {
            int count = 0;
            string text = "if(n == 1) then\nlet n = 2\nend if\nselect * from testtable where txre_info = 3\n and txre_mult = 4\ndisplay record";
            foreach (var fragment in SqlStatementExtractor.ExtractStatements(text))
            {
                Assert.IsTrue(fragment != null);
                Assert.IsTrue(fragment.FragmentLength > 0);
                string txt = fragment.GetText();
                int i = 0;
                count++;
            }
            Assert.IsTrue(count == 1);
        }

        [TestMethod]
        public void EmbeddedMultilineStaticTest2()
        {
            int count = 0;
            string text = "if(n == 1) then\nlet n = 2\nend if\nselect * from testtable inner join othertable on testtable.txre_id = othertable.arbq_id where txre_info = 3\n and txre_mult = 4\ndisplay record\nupdate othertable set arbq_id = 1\nlet x = 0";
            foreach (var fragment in SqlStatementExtractor.ExtractStatements(text))
            {
                Assert.IsTrue(fragment != null);
                Assert.IsTrue(fragment.FragmentLength > 0);
                string txt = fragment.GetText();
                int i = 0;
                count++;
            }
            Assert.IsTrue(count == 2);
        }

        [TestMethod]
        public void EmbeddedMultilineStaticTest3()
        {
            int count = 0;
            string text = "if(n == 1) then\nlet n = 2\nend if\nselect arbh_bill_dt1, arbh_bill_dt2, arbh_bill_dt3, arbh_bill_dt4\n into gr_arbilhdr.arbh_bill_dt1, gr_arbilhdr.arbh_bill_dt2,\n gr_arbilhdr.arbh_bill_dt3, gr_arbilhdr.arbh_bill_dt4\n from arbilhdr\n where arbh_year = gr_tmpfnd.tp_year and\n arbh_ar_cat = gr_tmpfnd.ar_cat and\n arbh_bill = gr_tmpfnd.bill\n   \n if (gr_arbilhdr.arbh_bill_dt1 IS NOT NULL) and";
            foreach (var fragment in SqlStatementExtractor.ExtractStatements(text))
            {
                Assert.IsTrue(fragment != null);
                Assert.IsTrue(fragment.FragmentLength > 0);
                string txt = fragment.GetText();
                int i = 0;
                count++;
            }
            Assert.IsTrue(count == 1);
        }

        [TestMethod]
        public void EmbeddedMultilineMultiresultDynamicTest()
        {
            int count = 0;
            string text = "if(n == 1) then\nlet n = 2\nend if\nlet prepare = \"select * from testtable inner join othertable\",\n    \" on testtable.txre_id = othertable.arbq_id\",\n   \" where txre_info = ? and txre_mult = ?\"\ndisplay record\nupdate othertable set arbq_id = 1\nlet x = 0";
            foreach (var fragment in SqlStatementExtractor.ExtractStatements(text))
            {
                Assert.IsTrue(fragment != null);
                Assert.IsTrue(fragment.FragmentLength > 0);
                string txt = fragment.GetText();
                int i = 0;
                count++;
            }
            Assert.IsTrue(count == 2);
        }

        [TestMethod]
        public void TokenizerTest1()
        {
            Tokenizer t = new Tokenizer(null, options: TokenizerOptions.Verbatim | TokenizerOptions.VerbatimCommentsAndLineJoins);

            using (TextReader tr = new StringReader("let x = 3 # this is a comment"))
            {
                t.Initialize(tr);
                var tokens = t.ReadTokens(int.MaxValue);
                Assert.IsTrue(tokens.Count == 5);
            }
        }

        // let tmp_err_msg = "Export file \"", tmp_exp_command clipped,"\" ",
        [TestMethod]
        public void TokenizerTest2()
        {
            Tokenizer t = new Tokenizer(null, options: TokenizerOptions.Verbatim | TokenizerOptions.VerbatimCommentsAndLineJoins);
            var path = @"..\..\TokenizerTests\TestFile1.4gl";
            using (TextReader tr = new StreamReader(path))
            {
                t.Initialize(tr);
                var tokens = t.ReadTokens(int.MaxValue);
                int i = 0;
            }
        }
    }
}
