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
                //T = T U (iopair.Input, GenerateStr(iopair); Gets trace expressions.
            }
            //T = GeneratePartition(T);
            //sigmaSetPrime = { sigma | (sigma, s) in S };
            //foreach (sigmaSet, eSet) in T
            //  let B[sigmaSet] = GenerateBoolClassifier(sigmaSet, sigmaSetPrime-sigmaSet);
            //Let (sigmaSet1, eSet1),...,(sigmaSetK, eSetK) be the k elements in 
            //  T in increasing order of Size(eSet)
            //return Switch((B[sigmaSet1], eSet1),...,(B[sigmaSetK], eSetK));
        }

        private DAG GenerateStr(IOPair pair)
        {
            List<Node> nodes = null; //= {1,...,Length(pair.Output)};
            Node nodeStart = null, //={1},
                 nodeEnd = null; //={Length(pair.Output)};
            List<Edge> edges = null; //=All edges for i and j where 1 <= i <= j <= Length(pair.Output)
            // Let W be the mapping that maps edge <i,j> to the set 
            //      {ConstStr(s[i:j]} U GenerateSubstring(pair.Input, s[i:j]).
            Dictionary<Edge, Operation> mapping = null;
            mapping = GenerateLoop(pair, edges, mapping);
            return new DAG(nodes, nodeStart, nodeEnd, edges, mapping);
        }

        private void GeneratePartition()
        {
            throw new NotImplementedException();

            //while exists (sigmaSet, eSet), (sigmaSetPrime, eSetPrime) in T such that Comp(eSet, eSetPrime)
            //  Let (sigmaSet1, eSet1), (sigmaSet2, eSet2) in T be such that CS(eSet1, eSet2) is largest.
            //  T = T - {(sigmaSet1, eSet1), (sigmaSet2, eSet2)} U {(sigmaSet1 U sigmaSet2, Intersect(eSet1, eSet2))};
            //return T;
        }

        private void GenerateBoolClassifier()
        {
            throw new NotImplementedException();

            //sigmaSetPrime1 = sigmaSet1;
            //b = false;
            //while (sigmaSetPrime1 is not null)
            //  OldSigmaSetPrime1 = sigmaSetPrime1;
            //  sigmaSetPrime2 = sigmaSet2;
            //  sigmaSetPrimePrime1 = sigmaSetPrime1;
            //  d = true;
            //  while (sigmaSetPrime2 is not null)
            //      OldSigmaSetPrime2 = sigmaSetPrime2;
            //      Preds = {Match(vi, r, c), !Match(vi, r, c) | [[Match(vi, r, c)]]sigma, sigma in sigmaSet1 U sigmaSet2 };
            //      Let pi in Preds be such that CSP(pi, sigmaSetPrimePrime1, sigmaSetPrime2) is largest
            //      d = d AND pi; (or at least the intersection symbol)
            //      sigmaSetPrimePrime1 = sigmaSetPrimePrime1 - {sigma1 | sigma1 in sigmaSetPrimePrime1, ![[pi]]sigma1 };
            //      sigmaSetPrime2 = sigmaSetPrime2 - {sigma2 | sigma2 in sigmaSetPrime2, ![[pi]]sigma2 };
            //      if (OldSigmaSetPrime2 = SigmaSetPrime2) then FAIL;
            //  sigmaSetPrime1 = sigmaSetPrime1 - sigmaSetPrimePrime1;
            //  b = b OR d;
            //  if (OldSigmaSetPrime1 = sigmaSetPrime1) then FAIL;
            //return b;
        }

        private Dictionary<Edge, Operation> GenerateLoop(IOPair pair, List<Edge> edges, Dictionary<Edge, Operation> mapping)
        {
            //foreach 0 <= k1, k2, k3 < Length(s)
            //  eSet1 = GenerateStr(sigma, s[k1 : k2]);
            //  eSet2 = GenerateStr(sigma, s[k2 : k3]);
            //  eSet = Unify(eSet1, eSet2);
            //  if ([[Loop(lambda W : eSet)]]sigma = {s[k1 : k3]})
            //      W(<k1, k3>) = W'(<k1, k3>) U {Loop(lambda W : eSet)};

            string input = pair.Input;
            int len = pair.Output.Length;
            for (int i = 0; i < len; ++i)
            {
                for (int j = i; j < len; ++j)
                {
                    for (int k = j; k < len; ++k)
                    {
                        //traceSet1 = GenerateStr()
                    }
                }
            }

            return mapping;
        }

        private void GenerateSubstring()
        {
            throw new NotImplementedException();

            //result = null;
            //foreach (i, k) such that s is substring of sigma(vi) at position k
            //  Y1 = GeneratePosition(sigma(vi), k);
            //  Y2 = GeneratePosition(sigma(vi), k + Length(s));
            //  result = result U PSubStr(v1, Y1, Y2)};
            //return result;
        }

        private void GeneratePosition()
        {
            throw new NotImplementedException();

            //result = {CPos(k), CPos(-(Length(s)-k)};
            //foreach r1 = TokenSeq(T1,...,Tn) matching s[k1 : k-1] for some k1:
            //  foreach r2 = TokenSeq(T1Prime,...,TmPrime) matching s[k:k2] for some k2:
            //      r12 = TokenSeq(T1,..., T1Prime,...,TmPrime);
            //      Let c be such that s[k1:k2] is the cth match for r12 in s.
            //      Let cPrime be the total number of matches for r12 in s.
            //      rSet1 = generateRegex(r1, s);
            //      rSet2 = generateRegex(r2, s);
            //      result = result U {Pos(rSet1, rSet2, {c, -(cprime - c + 1)})};
            //return result;
        }

        private void GenerateRegex()
        {
            throw new NotImplementedException();

            //let r be of the form TokenSeq(T1,...,Tn)
            //return TokenSeq(IPartss(T1),...,IPartss(Tn));
        }

        class IOPair
        {
            //public string[] Input { get; private set; }
            public string Input { get; private set; }
            public string Output { get; private set; }

            public IOPair(string[] input, string output)
            {
                Input = input[0];   //TODO: Only counts the first column right now.
                Output = output;
            }
        }

        /// <summary>
        /// For DAG in paper. Represented by eta.
        /// </summary>
        class Node
        {
            //Nothing...yet.
        }

        /// <summary>
        /// For DAG in paper. Represented by xi.
        /// </summary>
        class Edge
        {
            //Nothing...yet.
        }

        /// <summary>
        /// Necessary for whatever the edges in the DAG represent...
        /// </summary>
        class Operation
        {

        }

        class DAG
        {
            public List<Node> Nodes { get; private set; }
            public Node Start { get; private set; }
            public Node End { get; private set; }
            public List<Edge> Edges { get; private set; }
            public Dictionary<Edge, Operation> Mapping { get; private set; }

            public DAG(List<Node> nodes, Node start, Node end, List<Edge> edges, Dictionary<Edge, Operation> mapping)
            {
                Nodes = nodes;
                Start = start;
                End = end;
                Edges = edges;
                Mapping = mapping;                
            }
        }
    }
}
