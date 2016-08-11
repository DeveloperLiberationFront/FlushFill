using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Linq;
using System.Text;

namespace flushfillsrc
{
    class Program
    {
        static void Main(string[] args)
        {
            GetFiles();
            //new FlushFill().Synthesize("../../examples/ex0-2.txt");
            //new FunctionScraper().Scrape();
            Console.ReadLine();
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
            }
        }
    }
}
