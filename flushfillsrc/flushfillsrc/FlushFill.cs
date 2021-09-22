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

            IOSet iopairs = GetIOFromFile(file);
            Synthesize(iopairs);
            return "Done.";
        }

        /// <summary>
        /// Opens a file and gets all the pairs of input and their respective output.
        /// </summary>
        /// <param name="file">The file to open and parse.</param>
        /// <returns>A list of IOPair objects which represents the problem.</returns>
        private static IOSet GetIOFromFile(string file)
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
            }

            reader.Close();
            return new IOSet(iopairs);
        }

        private void Synthesize(IOSet io)
        {
            List<ExcelFunction> functions = new FunctionScraper().Scrape();
            for (int max_depth = 1; max_depth < 5; ++max_depth)
            {
                foreach (string s in Recurse(functions, io, max_depth))
                {
                    string fullFormula = "=" + s;
                    if (CheckAllCases(io, fullFormula))
                    {
                        string formula = string.Format(s, "<input>");
                        Console.WriteLine(formula);
                    }
                }
            }

            //TODO: Ranking
        }

        private IEnumerable<string> Recurse(List<ExcelFunction> functions, List<IOPair> io, int max_depth)
        {
            foreach (ExcelFunction function in functions)
            {

                Console.WriteLine(function.Name);

                int numberOfArguments = function.NumberOfTotalArguments;
                if (numberOfArguments > 4) continue; //temporary time saver
                foreach (string s in GetArgumentCombinations(numberOfArguments, max_depth, functions, io))
                {
                    yield return function.Name + "(" + s + ")";
                }

            }
        }

        private IEnumerable<string> GetArgumentCombinations(int number, int max_depth, List<ExcelFunction> functions, List<IOPair> io)
        {
            string text = io.First().Input;
            IEnumerator[] arguments = new IEnumerator[number];
            for (int i = 0; i < number; ++i)
            {
                arguments[i] = GetAllArguments(text, max_depth, functions, io);
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

                    argument = GetAllArguments(text, max_depth, functions, io);
                    argument.MoveNext();
                    arguments[index] = argument;    //since it seems copies are made, not references
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

        private IEnumerator<string> GetAllArguments(string text, int depth, List<ExcelFunction> functions, List<IOPair> io)
        {
            yield return "\"{0}\"";
            yield return "true";
            yield return "false";

            int limit = text.Length;
            for (int i = 0; i <= limit; ++i)
                yield return i + "";

            if (depth > 1)
            {
                foreach (string s in Recurse(functions, io, depth - 1))
                    yield return s;
            }
        }
    }

    /// <summary>
    /// Wrapper for an input value and its expected transformation.
    /// </summary>
    public class IOPair
    {
        public string Input { get; private set; }
        public string Output { get; private set; }

        public IOPair(string i, string o)
        {
            Input = i; Output = o;
        }
    }

    public class IOSet
    {
        public List<IOPair> Examples { get; private set; }
        private Excel.Application evaluator = new Excel.Application();

        public IOSet(List<IOPair> examples)
        {
            Examples = examples;
        }

        public bool CheckAllCases(string s)
        {
            foreach (IOPair pair in Examples)
            {
                string formula = string.Format(s, pair.Input);
                if (!CheckResults(pair, formula))
                {
                    return false;
                }
            }

            return true;
        }

        public bool CheckResults(IOPair io, string fullFormula)
        {
            dynamic eval = evaluate(fullFormula);
            if (!(eval is Int32))  //http://stackoverflow.com/a/2425170
            {
                if (eval.ToString().Equals(io.Output))
                {
                    return true;
                }
            }
            
            return false;
        }
        
        public dynamic evaluate(string formula)
        {
            dynamic result;
            try
            {
                result = evaluator.Evaluate(formula);
            }
            catch (IOException)
            {
                result = -2146826273; // #Value! 
            }
            return result;
        }
    }
}
