using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace flushfillsrc
{
    /// <summary>
    /// The driving class.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            GetFiles();
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
            } catch (DirectoryNotFoundException)
            {
                Console.WriteLine("ERROR: Cannot find directory \"{0}\".", exDir);
                return;
            }

            foreach (string file in files)
            {
                new FlushFill().Complete(file);
            }
        }
    }

    /// <summary>
    /// The class that synthesizes programs.
    /// </summary>
    class FlushFill
    {
        /// <summary>
        /// The driving function for program synthesis. Just pass in the example file (currently expects txt with tab
        /// separation, though should probably just change to tsv...)
        /// </summary>
        /// <param name="file">File name.</param>
        /// <returns></returns>
        internal bool Complete(string file)
        {
            if (file.EndsWith(".txt"))
            {
                List<List<string>> input;
                List<string> output;
                ExtractColumns(file, out input, out output);

                output.ForEach(i => Console.Write(i + " "));
                Console.WriteLine();

                return false;
            } else if (!file.EndsWith(".csv"))
                throw new NotImplementedException();

            return false;
        }

        /// <summary>
        /// Reads in the tsv/txt/whatever file and gets a list of input columns and the output column.
        /// </summary>
        /// <param name="file">File to read.</param>
        /// <param name="input">Returning input columns in list of lists.</param>
        /// <param name="output">Returning output column as list.</param>
        /// <returns>Lists of the input and the output.</returns>
        private void ExtractColumns(string file, out List<List<string>> input, out List<string> output)
        {
            StreamReader reader = new StreamReader(File.OpenRead(@file));
            List<List<string>> columns = new List<List<string>>();

            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                string[] values = line.Split('\t');

                for (int i = 0; i < values.Length; ++i)
                {
                    if (columns.Count <= i)
                        columns.Add(new List<string>());

                    columns[i].Add(values[i]);
                }
            }

            if (columns.Count < 2)
                throw new InvalidOperationException();

            output = columns.Last();
            columns.RemoveAt(columns.Count - 1);
            input = columns;
        }
    }
}
