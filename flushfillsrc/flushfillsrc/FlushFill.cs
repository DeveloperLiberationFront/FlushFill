using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Excel = Microsoft.Office.Interop.Excel;
using System.Collections;

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
            List<string> matches = new List<string>();
            List<ExcelFunction> functions = new FunctionScraper().Scrape();
            Recurse(functions, io);
            return "";
        }

        private Excel.Application evaluator = new Excel.Application();

        private void Recurse(List<ExcelFunction> functions, List<IOPair> io)
        {
            IOPair first = io.First();
            foreach (ExcelFunction function in functions)
            {
                if (!AcceptableFunction(function.Name)) continue;
                Console.WriteLine(function.Name);

                int arguments = function.NumberOfTotalArguments;
                foreach (string s in GetArgumentCombinations(arguments, first.Input))
                {
                    bool winnerWinnerChickenDinner = true;

                    foreach (IOPair pair in io) {
                        string argString = string.Format(s, pair.Input);
                        string formula = string.Format("={0}({1})", function.Name, argString);
                        if (!CheckResults(first, formula))
                        {
                            winnerWinnerChickenDinner = false;
                            break;
                        }
                    }

                    if (winnerWinnerChickenDinner)
                    {
                        string argString = string.Format(s, "<input>");
                        string formula = string.Format("={0}({1})", function.Name, argString);
                        Console.WriteLine(formula);
                    }
                }
            }
        }

        private bool CheckResults(IOPair io, string fullFormula)
        {
            dynamic eval = evaluator.Evaluate(fullFormula);
            if (!(eval is Int32))  //http://stackoverflow.com/a/2425170
            {
                if (eval.ToString().Equals(io.Output))
                {
                    return true;
                }
            }

            return false;
        }

        private IEnumerable<string> GetArgumentCombinations(int number, string text)
        {
            IEnumerator[] arguments = new IEnumerator[number];
            for (int i = 0; i < number; ++i)
            {
                arguments[i] = GetAllArguments(text);
            }

            for (int i = 0; i < number - 1; ++i)    //all but last
            {
                arguments[i].MoveNext();
            }

            int index = arguments.Length - 1;
            while (true)
            {
                IEnumerator argument = arguments[index];
                bool end = !argument.MoveNext();

                if (end)
                {
                    if (index == 0) break;

                    argument = GetAllArguments(text);
                    argument.MoveNext();
                    arguments[index] = argument;
                    index -= 1;
                } else
                {
                    yield return Stringify(arguments);
                    index = arguments.Length - 1;   //reset to final index
                }
            }
        }

        private string Stringify(IEnumerator[] arguments)
        {
            StringBuilder stringBuild = new StringBuilder();

            foreach (IEnumerator argument in arguments)
            {
                stringBuild.Append(argument.Current);
                stringBuild.Append(",");
            }

            stringBuild.Remove(stringBuild.Length - 1, 1);
            return stringBuild.ToString();
        }

        private IEnumerator GetAllArguments(string text)
        {
            yield return "\"{0}\"";
            yield return "true";
            yield return "false";

            int limit = text.Length;
            for (int i = 0; i <= limit; ++i)
                yield return i;
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

        public enum Function
        {
            CONCATENATE, EXACT, LEFT, LEN, LOWER, MID,
            PROPER, REPLACE, RIGHT, SEARCH, UPPER
        }

        public bool AcceptableFunction(string function)
        {
            switch (function)
            {
                case "LEFT":
                case "MID":
                case "REPLACE":
                case "RIGHT":
                case "SEARCH":
                    return true;
                default:
                    return false;
            }
        }
    }
}
