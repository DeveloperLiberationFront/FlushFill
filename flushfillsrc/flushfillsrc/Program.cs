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

            IOPair first = io.First();
            io.Remove(first);

            /*Type outputType; bool flag; int num;
            if (bool.TryParse(pair.Output, out flag)) outputType = Type.LOGICAL;
            else if (int.TryParse(pair.Output, out num)) outputType = Type.NUMBER;
            else outputType = Type.TEXT;*/
            Type outputType = Type.TEXT;

            List<ExcelFunction> funcs = functions.GetFunctionsOfType(outputType);
            List<string> possibilities = new List<string>();
            foreach (ExcelFunction func in funcs)
            {
                //Console.WriteLine("Start recurse...");
                func.Arguments = Recurse(func, first, 2, functions);
                Dictionary<object, string> all = Expand(func);

                foreach (object result in all.Keys)
                {
                    //Console.WriteLine(result);
                    if (first.Output.Equals(result))
                        possibilities.Add(all[result].Replace(first.Input, "{0}"));
                }
            }

            foreach (string s in possibilities)
            {
                Console.WriteLine(s);
            }

            foreach (IOPair pair in io) {
                Console.WriteLine(pair.Input);
                foreach (string s in possibilities)
                {
                    Console.WriteLine(string.Format(s, pair.Input));
                }
            }

            return "";
        }

        int i = 1;
        private List<Args[]> Recurse(ExcelFunction func, IOPair ex, int depth, ExcelFunctionFactory functions)
        {
            //Console.WriteLine("Recurse #" + i++);
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
                            if (func.Name == Function.SEARCH)
                                if (i == 1) argument = Args.SingleArgument(ex.Input);
                                else argument = Args.TextArguments(ex.Output);
                            else if (func.Name == Function.REPLACE)
                                if (i == 0) argument = Args.SingleArgument(ex.Input);
                                else argument = Args.TextArguments(ex.Output);
                            else argument = Args.SingleArgument(ex.Input);
                        }
                        else if (type == Type.NUMBER)
                            argument = Args.NumberArguments(ex.Input.Length); //Number should never go above example length.
                        else /*if bool*/
                            argument = Args.BoolArguments();
                    } else
                    {
                        argument = Args.FuncArguments(type, functions);
                        foreach (object o in argument.Arguments)
                        {
                            ExcelFunction f = (ExcelFunction)o;
                            f.Arguments = Recurse(f, ex, depth - 1, functions);
                        }
                    }

                    args[i] = argument;
                }

                argCombinations.Add(args);
            }

            return argCombinations;
        }

        /// <summary>
        /// For the list of numbers from 0 to depth, this returns a list of every combination of numbers.
        /// Ex: (3, 3) => ((0,0,0),(0,0,1),(0,0,2),(0,1,0),(0,1,1),(0,1,2),(0,2,0)...(2,1,2),(2,2,0),(2,2,1),(2,2,2))
        /// </summary>
        /// <param name="depth">Maximum value of number + 1.</param>
        /// <param name="args">Number of values in each combination.</param>
        /// <returns></returns>
        private List<List<int>> GetPermutations(int depth, int args)
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
        /// result, function
        /// </summary>
        /// <param name="func"></param>
        /// <param name="argCombs"></param>
        /// <returns></returns>
        private Dictionary<object, string> Expand(ExcelFunction func)
        {
            List<Args[]> argCombs = func.Arguments;
            Dictionary<object, string> result_function = new Dictionary<object, string>();
            List<Dictionary<object, string>> arguments = GetAllArguments(argCombs);

            List<object[]> combinations = new List<object[]>();
            foreach (Dictionary<object, string> dict in arguments)
                if (combinations.Count == 0)
                    foreach (object o in dict.Keys)
                        combinations.Add(new object[] { o }); //combinations.Add(dict.Keys.ToArray());
                else combinations = Combine(combinations, dict.Keys.ToArray());

            foreach (object[] args in combinations)
            {
                try
                {
                    StringBuilder formulae = new StringBuilder();
                    formulae.Append(func.ToString() + "(");
                    foreach (object a in args) formulae.Append(a + ",");
                    formulae.Remove(formulae.Length - 1, 1);    //Remove final comma.
                    formulae.Append(")");
                    string form = formulae.ToString();
                    object result = func.Execute(args);

                    DictAdd(result_function, result, formulae.ToString());
                    
                    //Console.WriteLine(func.ToString() + ": " + func.Execute(args));
                } catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }

            return result_function;
        }

        private List<Dictionary<object, string>> GetAllArguments(List<Args[]> argCombs)
        {
            int numArgs = argCombs.First().Length;
            List<Dictionary<object, string>> arguments = new List<Dictionary<object, string>>(); //Should have at least one, all same size
            for (int i = 0; i < numArgs; ++i) arguments.Add(new Dictionary<object, string>());

            foreach (Args[] argComb in argCombs)
            {
                for (int i = 0; i < argComb.Length; ++i)
                {
                    Dictionary<object, string> thisArg_result_function = arguments[i];
                    object[] args = argComb[i].Arguments;
                    if (args.Length > 0 && args[0] is ExcelFunction)
                    {
                        foreach (ExcelFunction f in args.Select(o => (ExcelFunction)o))
                        {
                            Dictionary<object, string> below = Expand(f);
                            foreach (object o in below.Keys) DictAdd(thisArg_result_function, o, below[o]);
                        }
                    }
                    else
                    {
                        foreach (object arg in args) DictAdd(thisArg_result_function, arg, arg + "");
                    }
                }
            }

            return arguments;
        }

        private void DictAdd(Dictionary<object, string> dict, object key, string val)
        {
            if (!dict.ContainsKey(key))
            {
                dict[key] = val;
            } else
            {
                //Nothing for now.
            }
        }

        /// <summary>
        /// Similar to GetPermutations, but produces every combination of the two arguments.
        /// </summary>
        /// <param name="combinations"></param>
        /// <param name="domain"></param>
        /// <returns></returns>
        private List<object[]> Combine(List<object[]> combinations, object[] domain)
        {
            List < object[] > newCombinations = new List<object[]>();

            foreach (object[] oarr in combinations)
            {
                foreach (object o in domain)
                {
                    object[] newC = new object[oarr.Length + 1];
                    Array.Copy(oarr, newC, oarr.Length);
                    newC[oarr.Length] = o;  //oarr.Length should be the final element in the new array.
                    newCombinations.Add(newC);
                }
            }

            return newCombinations;
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

        /// <summary>
        /// The implementation of an excel function in this C# environment. Uses Excel interop library when it can.
        /// </summary>
        /// <param name="o">The arguments that a function needs.</param>
        /// <returns>Returns whatever this particular functions would return in Excel.</returns>
        public delegate object ExcelFunctionDelegate(params object[] o);

        /// <summary>
        /// Model class for every Excel function wrapper I'll make from now on out.
        /// </summary>
        private class ExcelFunction
        {
            public Function Name { get; private set; }
            public List<Args[]> Arguments { get; set; }
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

        /// <summary>
        /// The source of Excel function objects. Rather than making an individual class for each function,
        /// this just makes variations within the ExcelFunction class.
        /// </summary>
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
                return FUNCS.Values.Where(f => f.OutputType == type).Select(f => f.MemberwiseClone()).ToList(); //TODO: Nums can be used as text.
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
                        int start = (int)args[1], len = (int)args[2];
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
                        int start = (int)args[2];

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
