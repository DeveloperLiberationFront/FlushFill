using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Excel = Microsoft.Office.Interop.Excel;

namespace flushfillsrc
{
    /**
     * 23May - Had to download MICROSOFT OFFICE 2010 PRIMARY INTEROP ASSEMBLIES
     */
    public class FlushFill
    {
        /// <summary>
        /// Open and read a file for its input/output pairs.
        /// </summary>
        /// <param name="file">The file with the examples.</param>
        /// <returns>The function that transforms the inputs into the outputs.</returns>
        public string Synthesize(string file)
        {
            if (!file.EndsWith(".txt") || !File.Exists(file))
                throw new NotSupportedException("Synthesize: Invalid file for examples.");

            List<IOPair> iopairs = GetIOFromFile(file);
            return Synthesize(iopairs);
        }

        /// <summary>
        /// Opens a file and gets all the pairs of input and their respective output.
        /// </summary>
        /// <param name="file">The file to open and parse.</param>
        /// <returns>A list of IOPair objects which represents the problem.</returns>
        private static List<IOPair> GetIOFromFile(string file)
        {
            StreamReader reader = new StreamReader(File.OpenRead(@file));       //Note to self: @ is verbatim string, regex ignored
            List<IOPair> iopairs = new List<IOPair>();
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                string[] splitline = line.Split('\t');
                if (splitline.Length != 2)
                {
                    Console.Error.WriteLine("Synthesize: Unexpected number of columns; for right now, only 2. Skipping...");
                    Console.Error.WriteLine("\t" + line);
                    continue;
                }

                iopairs.Add(new IOPair(splitline[0], splitline[1]));
                break;
            }

            reader.Close();
            return iopairs;
        }

        private string Synthesize(List<IOPair> io)
        {
            return "";
        }

        /// <summary>
        /// Wrapper for an input value and its expected transformation.
        /// </summary>
        private class IOPair
        {
            public string Input { get; private set; }
            public string Output { get; private set; }

            public IOPair(string i, string o)
            {
                Input = i; Output = o;
            }
        }

        /// <summary>
        /// Holds all possible arguments for a given argument.
        /// </summary>
        private class Args
        {
            public object[] Arguments { get; private set; }

            private Args(object[] arg)
            {
                Arguments = arg;
            }

            public static Args SingleArgument(object arg)
            {
                return new Args(new object[] { arg });
            }

            public static Args NumberArguments(int limit)
            {
                return new Args(Enumerable.Range(1, limit).Select(number => (object) number).ToArray());
            }

            public static Args TextArguments(string output) 
            {
                HashSet<string> substrings = new HashSet<string>();
                for (int i = 0; i < output.Length; ++i)
                    for (int j = 0; j < output.Length - i + 1; ++j)
                        substrings.Add(output.Substring(i, j));
                return new Args(substrings.Select(s => (object)s).ToArray());
            }

            public static Args BoolArguments()
            {
                return new Args(new object[] { true, false });
            }

            /*public static Args FuncArguments(Type type, ExcelFunctionFactory funcs)
            {
                return new Args(funcs.GetFunctionsOfType(type).Select(f => (object)f).ToArray());
            }*/

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder("[ ");
                foreach (object o in Arguments) sb.Append(o.ToString() + " ");
                sb.Append(" ]");
                return sb.ToString().Trim();
            }
        }

        public enum Function
        {
            CONCATENATE, EXACT, LEFT, LEN, LOWER, MID,
            PROPER, REPLACE, RIGHT, SEARCH, UPPER
        }

        public void TestOpenWorkbook()
        {
            Excel.Application app = new Excel.Application();
            Excel.Workbook workbook = app.Workbooks.Add(Excel.XlWBATemplate.xlWBATWorksheet);
            Excel.Worksheet worksheet = workbook.Worksheets[1];
            dynamic v = app.Evaluate("=SUM(1,2,)");
            Console.WriteLine(v is Int32);  //http://stackoverflow.com/a/2425170
        }
    }
}
