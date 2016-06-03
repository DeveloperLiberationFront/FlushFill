﻿using System;
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
                break;
            }

            return Synthesize(iopairs);
        }

        private string Synthesize(List<IOPair> io)
        {
            ExcelFunctionFactory functions = new ExcelFunctionFactory();
            foreach (IOPair pair in io)
            {
                Type outputType; bool flag; int num;
                if (bool.TryParse(pair.Output, out flag)) outputType = Type.LOGICAL;
                else if (int.TryParse(pair.Output, out num)) outputType = Type.NUMBER;
                else outputType = Type.TEXT;

                List<ExcelFunction> funcs = functions.GetFunctionsOfType(outputType);
                foreach (ExcelFunction func in funcs)
                {
                    Console.WriteLine("Start recurse...");
                    List<Args[]> l = Recurse(func, pair, 2, functions);
                    foreach (Args[] a in l)
                    {
                        Console.WriteLine(func);
                        foreach (Args aa in a) Console.Write(aa + " -- ");
                        Console.WriteLine();
                        Console.WriteLine();
                    }
                    Console.ReadLine();

                }
            }

            return "";
        }

        int i = 1;
        private List<Args[]> Recurse(ExcelFunction func, IOPair ex, int depth, ExcelFunctionFactory functions)
        {
            Console.WriteLine("Recurse #" + i++);
            List<List<int>> depths = GetPermutations(depth, func.NumArguments);
            Type[] types = func.InputTypes;

            List<Args[]> argCombinations = new List<Args[]>();
            foreach (List<int> list in depths)
            {
                Args[] args = new Args[list.Count];
                for (int i = 0; i < list.Count; ++i)
                {
                    int d = list[i];
                    Type type = types[i];

                    Args argument;
                    if (d == 0)
                    {
                        if (type == Type.TEXT)
                        {
                            if (func.Name == Function.SEARCH) if (i == 1) argument = Args.SingleArgument(ex.Input); else argument = Args.TextArguments(ex.Output);
                            else if (func.Name == Function.REPLACE) if (i == 0) argument = Args.SingleArgument(ex.Input); else argument = Args.TextArguments(ex.Output);
                            else argument = Args.SingleArgument(ex.Input);
                        }
                        else if (type == Type.NUMBER) argument = Args.NumberArguments(ex.Input.Length); //Number should never go above example length.
                        else /*if bool*/ argument = Args.BoolArguments();
                    } else
                    {
                        argument = Args.FuncArguments(type, functions);
                        foreach (object o in argument.Arguments)
                        {
                            ExcelFunction f = (ExcelFunction)o;
                            List<Args[]> l = Recurse(f, ex, depth - 1, functions);
                        }
                    }

                    args[i] = argument;
                }

                argCombinations.Add(args);
            }

            return argCombinations;
        }

        private static List<List<int>> GetPermutations(int depth, int args)
        {
            List<List<int>> depths = (from num in Enumerable.Range(0, depth).ToList() select new List<int> { num }).ToList();
            foreach (int _ in Enumerable.Range(0, args - 1))
            {
                List<List<int>> newDepths = new List<List<int>>();

                foreach (List<int> oldDepth in depths)
                    foreach (int j in Enumerable.Range(0, depth))
                    {
                        List<int> newList = new List<int>(oldDepth);
                        newList.Add(j);
                        newDepths.Add(newList);
                    }

                depths = newDepths;
            }

            return depths;
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
                return new Args(Enumerable.Range(0, limit).Select(number => (object) number).ToArray());
            }

            public static Args TextArguments(string output) 
            {
                HashSet<string> substrings = new HashSet<string>();
                for (int i = 0; i < output.Length; ++i)
                    for (int j = 0; j < output.Length - i; ++j)
                        substrings.Add(output.Substring(i, j));
                return new Args(substrings.Select(s => (object)s).ToArray());
            }

            public static Args BoolArguments()
            {
                return new Args(new object[] { true, false });
            }

            public static Args FuncArguments(Type type, ExcelFunctionFactory funcs)
            {
                return new Args(funcs.GetFunctionsOfType(type).Select(f => (object)f).ToArray());
            }

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder("[ ");
                foreach (object o in Arguments) sb.Append(o.ToString() + " ");
                sb.Append(" ]");
                return sb.ToString().Trim();
            }
        }

        public delegate object ExcelFunctionDelegate(params object[] o);

        /// <summary>
        /// Model class for every Excel function wrapper I'll make from now on out.
        /// </summary>
        private class ExcelFunction
        {
            public Function Name { get; private set; }
            public object[] Arguments { get; private set; }
            public int NumArguments { get { return InputTypes.Length; } }
            public int MinimumArguments { get; private set; }
            public Type[] InputTypes { get; private set; }
            public Type OutputType { get; private set; }
            public bool Variadic { get; private set; }

            private ExcelFunctionDelegate FuncExecution;

            public ExcelFunction(Function name, Type[] input, Type output, ExcelFunctionDelegate func, bool variadic = false)
            {
                Name = name;
                InputTypes = input;
                OutputType = output;
                FuncExecution = func;
                Variadic = variadic;
            }

            public object Execute(params object[] o)
            {
                if (!TypeCheck(o))
                    throw new NotSupportedException("Bad types passed in.");

                return FuncExecution(o);
            }

            //Formerly ExcelFunctionDictionary.
            public bool TypeCheck(object[] args)
            {
                for (int i = 0; i < args.Length; ++i) //TODO: args and types numbers might not perfectly correspond.
                {
                    Type currentType = (i > InputTypes.Length && Variadic) ? InputTypes.Last() : InputTypes[i]; //What if first condition true but !Variadic?
                    object arg = args[i];

                    if (arg is ExcelFunction)
                    {
                        Type outputType = ((ExcelFunction)arg).OutputType;
                        switch (currentType)
                        {
                            case Type.LOGICAL:
                                if (outputType != Type.LOGICAL) return false; break;
                            case Type.NUMBER:
                                if (outputType != Type.NUMBER) return false; break;
                            case Type.TEXT:
                                break; 
                            default:
                                return false;
                        }
                    }
                    else //if just a primitive type
                    {
                        switch (currentType)
                        {
                            case Type.LOGICAL:
                                if (!(arg is bool)) return false; break;
                            case Type.NUMBER:
                                if (!(arg is int || arg is float)) return false; break;
                            case Type.TEXT:
                                break;
                            default:
                                return false;
                        }
                    }
                }

                return true;
            }

            public new ExcelFunction MemberwiseClone()
            {
                return new ExcelFunction(Name, InputTypes, OutputType, Execute, Variadic);
            }

            public override string ToString()
            {
                return Name.ToString("g");
            }
        }

        public enum Type { NUMBER, TEXT, LOGICAL };
        public enum Function
        {
            CONCATENATE, EXACT, LEFT, LEN, LOWER, MID,
            PROPER, REPLACE, RIGHT, SEARCH, UPPER
        }
        private class ExcelFunctionFactory
        {
            private Excel.WorksheetFunction funcEval = new Excel.Application().WorksheetFunction;
            public Dictionary<Function, ExcelFunction> FUNCS = new Dictionary<Function, ExcelFunction>();

            public ExcelFunction Create(Function func)
            {
                if (FUNCS.ContainsKey(func))
                    return FUNCS[func];
                else
                    throw new NotSupportedException("Unimplemented function: " + func);
            }

            public List<ExcelFunction> GetFunctionsOfType(Type type)
            {
                return FUNCS.Values.Where(f => f.OutputType == type).Select(f => f.MemberwiseClone()).ToList();
            }

            public ExcelFunctionFactory() {
                //https://support.office.com/en-us/article/CONCATENATE-function-8f8ae884-2ca8-4f7a-b093-75d702bea31d
                /*FUNCS[Function.CONCATENATE] = new ExcelFunction(Function.CONCATENATE.ToString("g"),
                    new Type[] { Type.TEXT, Type.TEXT }, Type.TEXT, delegate (object[] args)    //Can have one argument but not much point.
                    {
                        StringBuilder concat = new StringBuilder();
                        foreach (object o in args) concat.Append((string)o);
                        return concat.ToString();
                    }, true);*/

                //https://support.office.com/en-us/article/EXACT-function-d3087698-fc15-4a15-9631-12575cf29926
                FUNCS[Function.EXACT] = new ExcelFunction(Function.EXACT,
                    new Type[] { Type.TEXT, Type.TEXT }, Type.LOGICAL, delegate (object[] args)
                    {
                        return ((string)args[0]).Equals((string)args[1]);
                    });

                //https://support.office.com/en-us/article/LEFT-LEFTB-functions-9203d2d2-7960-479b-84c6-1ea52b99640c
                FUNCS[Function.LEFT] = new ExcelFunction(Function.LEFT,
                    new Type[] { Type.TEXT, Type.NUMBER }, Type.TEXT, delegate (object[] args)
                    {
                        string text = (string)args[0];
                        int len = (int)args[1];

                        if (len < 0) throw new NotSupportedException("No negative numbers allowed.");
                        else if (len >= text.Length) return text;
                        else return text.Substring(0, len);
                    });

                //https://support.office.com/en-us/article/LEN-LENB-functions-29236f94-cedc-429d-affd-b5e33d2c67cb
                FUNCS[Function.LEN] = new ExcelFunction(Function.LEN,
                    new Type[] { Type.TEXT }, Type.NUMBER, delegate (object[] args)
                    {
                        return ((string)args[0]).Length;
                    });

                //https://support.office.com/en-us/article/LOWER-function-3f21df02-a80c-44b2-afaf-81358f9fdeb4
                FUNCS[Function.LOWER] = new ExcelFunction(Function.LOWER,
                    new Type[] { Type.TEXT }, Type.TEXT, delegate (object[] args)
                    {
                        return ((string)args[0]).ToLower();
                    });

                //https://support.office.com/en-us/article/MID-MIDB-functions-d5f9e25c-d7d6-472e-b568-4ecb12433028
                FUNCS[Function.MID] = new ExcelFunction(Function.MID,
                    new Type[] { Type.TEXT, Type.NUMBER, Type.NUMBER }, Type.TEXT, delegate (object[] args)
                    {
                        string text = (string)args[0];
                        int start = (int)args[1] - 1; //Subtract one for offset.
                        int len = (int)args[2];

                        if (start < 0 || len < 0) throw new NotSupportedException("No negative numbers allowed.");
                        else if (start >= text.Length) return "";
                        else if (start + len >= text.Length) return text.Substring(start);
                        return text.Substring(start, len);
                    });

                //https://support.office.com/en-us/article/PROPER-function-52a5a283-e8b2-49be-8506-b2887b889f94
                FUNCS[Function.PROPER] = new ExcelFunction(Function.PROPER,
                    new Type[] { Type.TEXT }, Type.TEXT, delegate (object[] args)
                    {
                        return CultureInfo.CurrentCulture.TextInfo.ToTitleCase((string)args[0]);
                    });

                //https://support.office.com/en-us/article/REPLACE-REPLACEB-functions-8d799074-2425-4a8a-84bc-82472868878a
                FUNCS[Function.REPLACE] = new ExcelFunction(Function.REPLACE,
                    new Type[] { Type.TEXT, Type.NUMBER, Type.NUMBER, Type.TEXT }, Type.TEXT, delegate (object[] args)
                    {
                        string original = (string)args[0];
                        string replace = (string)args[3];
                        int start = (int)args[1] - 1, len = (int)args[2];
                        StringBuilder result = new StringBuilder();

                        return funcEval.Replace(original, start, len, replace);
                    });

                //https://support.office.com/en-us/article/RIGHT-RIGHTB-functions-240267ee-9afa-4639-a02b-f19e1786cf2f
                FUNCS[Function.RIGHT] = new ExcelFunction(Function.RIGHT,
                    new Type[] { Type.TEXT, Type.NUMBER }, Type.TEXT, delegate (object[] args)
                    {
                        string text = (string)args[0];
                        int len = (int)args[1];

                        if (len < 0) throw new NotSupportedException("No negative numbers allowed.");
                        else if (len >= text.Length) return text;
                        else return text.Substring(text.Length - len);
                    });

                //https://support.office.com/en-us/article/SEARCH-SEARCHB-functions-9ab04538-0e55-4719-a72e-b6f54513b495
                FUNCS[Function.SEARCH] = new ExcelFunction(Function.SEARCH,
                    new Type[] { Type.TEXT, Type.TEXT, Type.NUMBER }, Type.NUMBER, delegate (object[] args)
                    {
                        string find_text = (string)args[0], within_text = (string)args[1];
                        int start = (int)args[2] - 1;

                        return funcEval.Search(find_text, within_text, start);
                    });

                //https://support.office.com/en-us/article/UPPER-function-c11f29b3-d1a3-4537-8df6-04d0049963d6
                FUNCS[Function.UPPER] = new ExcelFunction(Function.UPPER,
                    new Type[] { Type.TEXT }, Type.TEXT, delegate (object[] args)
                    {
                        return ((string)args[0]).ToUpper();
                    });
            }
        }        
    }
}
