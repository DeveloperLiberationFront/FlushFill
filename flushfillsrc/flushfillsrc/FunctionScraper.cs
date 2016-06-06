using System;
using HtmlAgilityPack;
using ScrapySharp.Extensions;
using ScrapySharp.Network;
using System.Collections.Generic;

namespace flushfillsrc
{
    /// <summary>
    /// Summary description for Class1
    /// </summary>
    public class FunctionScraper
    {
        private const string URL_BASE = "https://support.office.com/en-us/article/";
        private const string HOME = "Excel-functions-by-category-5f91f4e9-7b42-46d2-9bd1-63f26a86c0eb";

        public void Scrape()
        {
            //http://stackoverflow.com/questions/19870116/using-htmlagilitypack-for-parsing-a-web-page-information-in-c-sharp
            HtmlWeb web = new HtmlWeb();

            HtmlDocument homeDoc = web.Load(URL(HOME));
            Dictionary<string, string> links = ExtractFunctionLinks(homeDoc); Console.WriteLine(links.Count);
            foreach (string func in links.Keys)
            {
                string url = URL(links[func]);
                HtmlDocument funcDoc = web.Load(url);
                ExtractFunctions(func, funcDoc);
                Console.WriteLine();
            }
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
        /// <returns>A set of every link to a function page.</returns>
        private static Dictionary<string, string> ExtractFunctionLinks(HtmlDocument doc)
        {
            Dictionary<string, string> links = new Dictionary<string, string>();
            foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//a"))
            {
                HtmlAttribute titleNode = node.Attributes["title"],
                              hrefNode = node.Attributes["href"];

                if (titleNode == null || hrefNode == null) continue;

                string title = titleNode.Value,
                       href = hrefNode.Value;

                string[] words = title.Split(' ');
                if (words.Length < 2) continue;

                string first = words[0], last = words[words.Length - 1];
                if (!first.ToUpper().Equals(first)) continue;                   //If the first word is not cap, it's not a function page.
                else if (last.Equals("function") || last.Equals("functions"))
                    for (int i = 0; i < words.Length - 1; ++i) //Otherwise, add all the functions covered by a page.
                    {
                        string funcName = words[i].TrimEnd(',');
                        if (!links.ContainsKey(funcName)) links.Add(funcName, href);
                    }         
                //else nothing
            }

            return links;
        }

        /// <summary>
        /// Finds function syntax in a webpage and makes the function in C#.
        /// </summary>
        /// <param name="doc"></param>
        private void ExtractFunctions(string func, HtmlDocument doc)
        {
            foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//p"))
            {

            }
        }
    }
}
