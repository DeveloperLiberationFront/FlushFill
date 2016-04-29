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
        internal class IOPair
        {
            private string[] input;
            public string[] Input
            {
                get { return input; }
            }

            private string output;
            public string Output
            {
                get { return output; }
            }

            public IOPair(string[] input, string output)
            {
                this.input = input;
                this.output = output;
            }
        }

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
                List<IOPair> iopairs = ExtractColumns(file);
                iopairs.ForEach(i => Console.Write(i.Output + " "));
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
        private List<IOPair> ExtractColumns(string file)
        {
            StreamReader reader = new StreamReader(File.OpenRead(@file));
            List<IOPair> iopairs = new List<IOPair>();

            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                string[] values = line.Split('\t');

                int len = values.Length;
                if (len < 2)
                    throw new InvalidOperationException();

                iopairs.Add(new IOPair(values.Take(len - 1).ToArray(), values[len - 1]));
            }

            return iopairs;
        }

        private void GenerateStringProgram(List<IOPair> iopairs)
        {
            List<Tuple<List<string>, string>> T = new List<Tuple<List<string>, string>>();
            foreach(IOPair iopair in iopairs)
            {

            }
        }

        private void GeneratePartition()
        {

        }

        private void GenerateBoolClassifier()
        {

        }

        private void GenerateStr()
        {

        }

        private void GenerateLoop()
        {

        }

        private void GenerateSubstring()
        {

        }

        private void GeneratePosition()
        {

        }

        private void GenerateRegex()
        {

        }
    }
}
