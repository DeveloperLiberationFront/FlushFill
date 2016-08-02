using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace flushfillsrc
{
    /// <summary>
    /// Variations in function signatures:
    ///     Each has a name -- before parenthesis
    ///     Each argument is separated by a comma.
    ///     But sometimes, the comma is within paired bracked. 
    ///         func(arg1, arg2, [arg3, arg4])
    ///         This happens when arguments are expected in pairs.
    ///     And sometimes the arguments end with ellipses, if indefinite numbers are accepted.
    ///     If paired arguments are accepted, then ellipses might happen with bracket all in one comma.
    ///         func(arg1, arg2,...[arg3, arg4])
    ///         Ellipses might either be three periods or a single ellipsis character.
    ///     Spaces are meaningless.
    /// </summary>
    public class Function
    {
        public string       Name { get; private set; }              //The words before the parenthesis.
        public List<string> RequiredArguments { get; private set; } //Every comma-separated value not in brackets
        public bool         Variadic { get; private set; }          //Does it have optional arguments?
        public List<string> OptionalArguments { get; private set; } //The optional arguments.
        public bool         Paired { get; private set; }            //Are arguments expected in pairs if variadic?         

        public Function(string func)
        {
            RequiredArguments = new List<string>();
            OptionalArguments = new List<string>();

            string[] nameAndArguments = func.Split('(');
            if (nameAndArguments.Length != 2)
                throw new FormatException("Invalid function signature: " + func);

            Name = nameAndArguments[0].Trim();

            string[] arguments = nameAndArguments[1].TrimEnd(')').Split(',');
            for (int i = 0; i < arguments.Length; ++i)
            {
                string argument = arguments[i].Trim();

                if (argument.StartsWith("[") && !(argument.EndsWith("]")))
                    Paired = true;

                if (argument.Contains(".") || argument.Contains("…"))
                    Variadic = true;

                //Some functions, like IFS, has ellipses and optional args within one argument spot.
                //This corrects for that.
                //argument = argument.Replace('.', ' ').Replace('…', ' ').Trim();
                //if (argument.Equals(""))
                //    continue;   

                if (argument.StartsWith("[") || argument.EndsWith("]"))
                {
                    argument = argument.TrimStart('[').TrimEnd(']').Trim();
                    OptionalArguments.Add(argument);
                } 
                else if (!argument.Equals("...") && !argument.Equals("…"))
                    RequiredArguments.Add(argument);
            }
        }

        public override string ToString()
        {
            string ret = Name + ": " + (Variadic ? "Variadic" : "Non-Variadic") + ", " + (Paired ? "Paired" : "Not Paired");
            foreach (string argument in RequiredArguments)
                ret += "\n\t" + argument;
            foreach (string argument in OptionalArguments)
                ret += "\n\t" + argument;
            return ret;
        }
    }
}
