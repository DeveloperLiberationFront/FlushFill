using System;
using HtmlAgilityPack;
using ScrapySharp.Extensions;
using ScrapySharp.Network;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace flushfillsrc
{
    /// <summary>
    /// Summary description for Class1
    /// </summary>
    public class FunctionScraper
    {
        private const string URL_BASE = "https://support.office.com/en-us/article/";
        private const string HOME = "Excel-functions-alphabetical-b3944572-255d-4efb-bb96-c6d90033e188";
        private const string SERIALIZE_DESTINATION = "./ ScrapedFiles.bin";

        public List<ExcelFunction> Scrape()
        {
            List<ExcelFunction> functions;
            if (File.Exists(SERIALIZE_DESTINATION))
            {
                functions = DeserializeFunctions();
                if (functions != null)
                    return functions;
            }

            //http://stackoverflow.com/questions/19870116/using-htmlagilitypack-for-parsing-a-web-page-information-in-c-sharp
            HtmlWeb web = new HtmlWeb();

            HtmlDocument homeDoc = web.Load(URL(HOME));
            Dictionary<string, Tuple<string, string>> links = ExtractFunctionLinks(homeDoc);
            functions = new List<ExcelFunction>(); 
            foreach (string func in links.Keys)
            {
                if (func.StartsWith("FORECAST")) continue;  //Irregular link destination.

                string functionType = links[func].Item1;
                string urlTail = links[func].Item2;
                string url = URL(urlTail);
                HtmlDocument funcDoc = web.Load(url);
                string functionString = ExtractFunctionString(func, funcDoc);
                functions.Add(new ExcelFunction(functionString, functionType));
            }

            SerializeFunctions(functions);
            return functions;
        }

        //http://www.dotnetperls.com/serialize-list
        private void SerializeFunctions(List<ExcelFunction> functions)
        {
            try
            {
                using (Stream stream = File.Open(SERIALIZE_DESTINATION, FileMode.Create))
                {
                    BinaryFormatter binary = new BinaryFormatter();
                    binary.Serialize(stream, functions);
                }
            }
            catch (IOException)
            {
                Console.WriteLine("Warning: Cannot serialize list of functions into binary file.");
                if (File.Exists(SERIALIZE_DESTINATION))
                    File.Delete(SERIALIZE_DESTINATION);
            }
        }

        private List<ExcelFunction> DeserializeFunctions()
        {
            List<ExcelFunction> functions = null;

            try
            {
                using (Stream stream = File.Open(SERIALIZE_DESTINATION, FileMode.Open))
                {
                    BinaryFormatter binary = new BinaryFormatter();
                    functions = (List<ExcelFunction>) binary.Deserialize(stream);
                }
            }
            catch (IOException)
            {
                Console.WriteLine("Warning: Cannot deserialize list of functions from binary file.");
            }

            return functions;
        }

        /// <summary>
        /// Gets full link for a given file on the API website. If it already has the directories, then do nothing.
        /// </summary>
        /// <param name="file">The file name on the web server</param>
        /// <returns>string for the file name with protocol and directory</returns>
        private string URL(string file)
        {
            if (file.Contains(".com"))
                return file;
            else 
                return URL_BASE + file;
        }

        /// <summary>
        /// Gets all the links from the home URL which point to pages with function definitions.
        /// </summary>
        /// <param name="doc">The HTML document for the API homepage</param>
        /// <returns>A dictionary where function names are associated with a function type
        /// (e.g. statistical, logical, financial) and the URL for the function description.</returns>
        private static Dictionary<string, Tuple<string, string>> ExtractFunctionLinks(HtmlDocument doc)
        {
            Dictionary<string, Tuple<string, string>> links = new Dictionary<string, Tuple<string, string>>();
            foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//a"))
            {
                string title = node.InnerHtml;
                HtmlAttribute hrefNode = node.Attributes["href"];

                if (hrefNode == null) continue;

                string href = hrefNode.Value;

                string[] words = title.Split(' ');
                if (words.Length < 2) continue;

                string first = words[0], last = words[words.Length - 1];
                if (!first.ToUpper().Equals(first)) continue;                   //If the first word is not cap, it's not a function page.
                else if (last.Equals("function") || last.Equals("functions"))
                {
                    HtmlNode row = node.CssSelectAncestors("tr").Single();
                    HtmlNode funcTypeNode = row.CssSelect("b").Last();
                    string funcType = funcTypeNode.InnerText.TrimEnd(':');

                    Tuple<string, string> typeAndLink = new Tuple<string, string>(funcType, href);

                    for (int i = 0; i < words.Length - 1; ++i) //Otherwise, add all the functions covered by a page.
                    {
                        string funcName = words[i].TrimEnd(',');
                        if (!links.ContainsKey(funcName))
                            links.Add(funcName, typeAndLink);
                    }
                }
                //else nothing
            }

            return links;
        }

        /// <summary>
        /// Finds function syntax in a webpage and makes the function in C#.
        /// </summary>
        /// <param name="doc"></param>
        private string ExtractFunctionString(string func, HtmlDocument doc)
        {
            Console.WriteLine(func);

            //TODO: Is there a more elegant way of regexing even the typos?
            switch (func)
            {
                case "ISEVEN":
                case "ISODD":
                    return func + "(value)";
                case "BASE":
                    return "BASE(Number, Radix, [Min_length])";   //Because the official page has a typo.
                case "SKEW.P":
                    return "SKEW.P(number1, [number2],…)";      //Spaces make things complicated
                case "IF":
                    return "IF(logical_test, value_if_true, [value_if_false])";
                case "IFS":
                    return "IFS(logical_test1, value_if_true1, [logical_test2, value_if_true2], "
                        + "[logical_test3, value_if_true3],…)";
                case "SWITCH":
                    return "SWITCH(expression, value1, result1, [default or value2, result2]," +
                        "…[default or value3, result3])";
                case "MODE.MULT":
                    return "MODE.MULT(number1,[number2],...)";   //Typo on official page.
                default:
                    break; //just fall out
            }

            string argRegex = "[\\s\\.…]*"          //An argument is preceded by any number of spaces, periods, or ellipses
                            + "(\\[?[^,\\)\"]*\\]?)"//and is either any series of non-comma, paren, space, or quote characters
                            + "\\s*",               //and is followed by any amount of whitespace
                   argsRegex = string.Format("({0},)*{0}", argRegex);
            Regex regex = new Regex("^" + func + "\\s*\\(" + argsRegex + "\\)$"),      //"\\s*\\([^\"\\)]*\\)$")
                  tags = new Regex("<[^>]+>");

            foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//p"))
            {
                string text = node.InnerHtml.CleanInnerText();

                if (!text.Contains(func)) continue;
                text = tags.Replace(text, "");
                Match match = regex.Match(text);
                if (match.Success) return text;
                else
                {
                    HtmlNodeCollection boldC = node.SelectNodes("//b");
                    if (boldC == null || boldC.Count == 0) continue;
                    foreach (HtmlNode bold in node.SelectNodes("//b"))
                    {
                        text = bold.InnerHtml.CleanInnerText();
                        text = tags.Replace(text, "");
                        match = regex.Match(text);
                        if (match.Success)
                            return text;
                        else if (text.Equals("Syntax"))
                        {
                            text = bold.ParentNode.InnerHtml.CleanInnerText();
                            text = tags.Replace(text, "");
                            if (text.Contains(":")) text = text.Substring(text.IndexOf(':') + 1).Trim(); //Overflow?
                            match = regex.Match(text);
                            if (match.Success)
                                return text;
                        }
                    }
                }
            }

            throw new NotSupportedException("CANT FIND IT");
        }
    }
}
