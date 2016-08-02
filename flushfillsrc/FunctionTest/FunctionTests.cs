using MStringAssert = Microsoft.VisualStudio.TestTools.UnitTesting.StringAssert;
using NUnit.Framework;
using flushfillsrc;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Collections;

namespace flushfillsrc.Tests
{
    //http://nunit.org/index.php?p=testCaseSource&r=2.5
    [TestFixture]
    public class FunctionTests
    {
        [Test, TestCaseSource(typeof(FunctionFactory), "TestCases")]
        public void FunctionTest(Function function)
        {
            Regex regex = new Regex("^[a-zA-Z0-9_]+$");
            foreach (string argument in function.RequiredArguments)
                MStringAssert.Matches(argument, regex);
            foreach (string argument in function.OptionalArguments)
                MStringAssert.Matches(argument, regex);
        }
    }

    public class FunctionFactory
    {
        public static IEnumerable TestCases
        {
            get
            {
                FunctionScraper scrape = new FunctionScraper();
                List<Function> functions = scrape.Scrape();
                foreach (Function function in functions)
                    yield return new TestCaseData(function);
            }
        }
    }
}