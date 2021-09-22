using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Excel = Microsoft.Office.Interop.Excel;

namespace flushfillsrc
{
    class FunctionSpace
    {
        private List<ExcelFunction> Functions { get; set; }
        private ParameterGenerator Generator { get; set; }
        private Dictionary<object, string> Results { get; set; }

        public FunctionSpace()
        {
            Functions = new FunctionScraper().Scrape();
            Generator = new GeneralGenerator();
            Results = new Dictionary<object, string>();
        }

        private static Excel.Application evaluator = new Excel.Application();
        public IEnumerable<string> Execute(IOSet ios)
        {
            return null;
        }
    }

    public abstract class ParameterGenerator
    {
        public abstract List<_Generator> GetGenerator(ExcelFunction func);
        public abstract void Reset(List<_Generator> args, int idx);
    }

    public abstract class _Generator
    {
        public abstract IEnumerable<object> Next();
    }

    class GeneralGenerator : ParameterGenerator
    {
        public override List<_Generator> GetGenerator(ExcelFunction func)
        {
            int argsNum = func.NumberOfTotalArguments;
            List<_Generator> args = new List<_Generator>(argsNum);
            for (int i = 0; i < argsNum; ++i)
            {
                args.Add(new _Generic());
            }

            return args;
        }

        public override void Reset(List<_Generator> args, int idx)
        {
            args[idx] = new _Generic();
        }

        private class _Generic : _Generator
        {
            public override IEnumerable<object> Next()
            {
                //input is irrelevant
                yield return "true";
                yield return "false";
                for (int i = 0; i <= 50; ++i)
                    yield return i;
            }
        }

        private class _GenericInput : _Generator
        {
            public override IEnumerable<object> Next()
            {
                yield return "{0}";
            }
        }
    }
}
