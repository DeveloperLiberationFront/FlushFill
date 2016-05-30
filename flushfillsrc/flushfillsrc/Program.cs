using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Excel = Microsoft.Office.Interop.Excel;

namespace flushfillsrc
{
    /**
     * 23May - Had to download MICROSOFT OFFICE 2010 PRIMARY INTEROP ASSEMBLIES
     */
    class Program
    {
        static void Main(string[] args)
        {
            GetFiles();
            Console.ReadLine();
            Excel.Application excelApp = new Excel.Application();
            Excel.WorksheetFunction funcs = excelApp.WorksheetFunction;
        }

        /// <summary>
        /// Get all the files in the examples folder.
        /// </summary>
        static void GetFiles()
        {
            string[] files;
            string exDir = "../../examples/";

            try
            {
                files = Directory.GetFiles(exDir);
            }
            catch (DirectoryNotFoundException)
            {
                Console.WriteLine("ERROR: Cannot find directory \"{0}\".", exDir);
                return;
            }

            foreach (string file in files)
            {
                Console.WriteLine(new FlushFill().Synthesize(file));
                break;
            }
        }
    }

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
            }

            return Synthesize(iopairs);
        }

        private string Synthesize(List<IOPair> io)
        {
            foreach (IOPair pair in io)
            {
                ExcelFunction mid = new ExcelFunctionFactory().Create(ExcelFunctionFactory.MID);
                for (int i = 1; i <= pair.Input.Length; ++i)
                {
                    for (int j = 1; j <= pair.Input.Length - i + 1; ++j)
                    {
                        string attempt = (string)mid.Execute(pair.Input, i, j);
                        if (attempt.Equals(pair.Output))
                            return String.Format("MID({0}, {1}, {2})", pair.Input, i, j);
                    }
                }
            }

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

        public delegate object ExcelFunctionDelegate(params object[] o);

        /// <summary>
        /// Model class for every Excel function wrapper I'll make from now on out.
        /// </summary>
        private class ExcelFunction
        {
            public ExcelFunctionDelegate Execute;

            public string Name { get; private set; }
            public object[] Arguments { get; private set; }
            public int NumArguments { get { return Arguments.Length; } }

            public ExcelFunction(string name, ExcelFunctionDelegate func)
            {
                Name = name;
                Execute = func;
            }
        }

        public enum Type { NUMBER, TEXT, LOGICAL };
        private class ExcelFunctionFactory
        {
            private Excel.WorksheetFunction funcEval = new Excel.Application().WorksheetFunction;
            public const string MID = "MID";

            public Dictionary<string, ExcelFunction> DICT = new Dictionary<string, ExcelFunction>();

            public ExcelFunction Create(string func)
            {
                ExcelFunction excelFunc = null;
                switch (func)
                {
                    case MID:
                        ExcelFunctionDelegate mid = delegate (object[] o)
                        {
                            if (!TypeCheck(func, o)) throw new NotSupportedException("Bad arguments.");
                            string text = (string)o[0]; int start_num = (int)o[1] - 1; int num_chars = (int)o[2]; //Subtract one for offset.

                            if (start_num < 0 || num_chars < 0) throw new NotSupportedException("No negative numbers allowed.");
                            else if (start_num >= text.Length) return "";
                            else if (start_num + num_chars >= text.Length) return text.Substring(start_num);
                            return text.Substring(start_num, num_chars);
                        };
                        excelFunc = new ExcelFunction(func, mid);
                        break;

                    default:
                        break;
                }

                return excelFunc;
            }

            //Formerly ExcelFunctionDictionary.
            public bool TypeCheck(string func, object[] args)
            {
                switch (func)
                {
                    case MID:
                        return args.Length == 3 && args[0] is string && args[1] is int && args[2] is int;
                    default:
                        return false;
                }
            }
        }

        
    }
}
