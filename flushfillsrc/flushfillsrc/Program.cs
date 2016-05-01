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
            foreach (IOPair iopair in iopairs)
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

        private DAG GenerateStr(string input, string output)
        {
            List<Node> nodes = null; //= {1,...,Length(pair.Output)};
            Node nodeStart = null, //={1},
                 nodeEnd = null; //={Length(pair.Output)};
            List<Edge> edges = null; //=All edges for i and j where 1 <= i <= j <= Length(pair.Output)
            // Let W be the mapping that maps edge <i,j> to the set 
            //      {ConstStr(s[i:j]} U GenerateSubstring(pair.Input, s[i:j]).
            Dictionary<Edge, Operation> mapping = null;
            mapping = GenerateLoop(input, output, edges, mapping);
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

        private Dictionary<Edge, Operation> GenerateLoop(string input, string output, List<Edge> edges, Dictionary<Edge, Operation> mapping)
        {
            //foreach 0 <= k1, k2, k3 < Length(s)
            //  eSet1 = GenerateStr(sigma, s[k1 : k2]);
            //  eSet2 = GenerateStr(sigma, s[k2 : k3]);
            //  eSet = Unify(eSet1, eSet2);
            //  if ([[Loop(lambda W : eSet)]]sigma = {s[k1 : k3]})
            //      W(<k1, k3>) = W'(<k1, k3>) U {Loop(lambda W : eSet)};

            int len = output.Length;
            for (int i = 0; i < len; ++i)
            {
                for (int j = i; j < len; ++j)
                {
                    for (int k = j; k < len; ++k)
                    {
                        var traceSet1 = GenerateStr(input, output.Substring(i, j - i));
                        var traceSet2 = GenerateStr(input, output.Substring(j, k - j));
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

        private class IOPair
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
        private class Node
        {
            //Nothing...yet.
        }

        /// <summary>
        /// For DAG in paper. Represented by xi.
        /// </summary>
        private class Edge
        {
            //Nothing...yet.
        }

        /// <summary>
        /// Necessary for whatever the edges in the DAG represent...
        /// </summary>
        private class Operation
        {
            //Nothing...yet.
        }

        private class DAG
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

        private class Switch
        {
            ///Nothing...for now.
        }

        /// <summary>
        /// SubString object.
        /// </summary>
        private class SubStr
        {
            public string String { get; private set; }
            public int Pos1 { get; private set; }
            public int Pos2 { get; private set; }
            
            public SubStr(string v1, int pos1, int pos2)
            {
                String = v1;
                Pos1 = pos1;
                Pos2 = pos2;
            }
        }

        /// <summary>
        /// Constant string object.
        /// </summary>
        private class ConstStr
        {
            public string String { get; private set; }

            public ConstStr(string s)
            {
                String = s;
            }
        }

        /// <summary>
        /// Loop object.
        /// </summary>
        private class Loop
        {
            public DAG Dag { get; private set; }

            public Loop(DAG dag)
            {
                Dag = dag;
            }
        }

        /// <summary>
        /// CPos object.
        /// </summary>
        private class CPos
        {
            public int K { get; private set; }

            public CPos(int k)
            {
                K = k;
            }
        }

        /// <summary>
        /// The Position function.
        /// </summary>
        private class Pos
        {
            public TokenSeq R1 { get; private set; }
            public TokenSeq R2 { get; private set; }
            public int C { get; private set; }

            public Pos(TokenSeq r1, TokenSeq r2, int c)
            {
                R1 = r1;
                R2 = r2;
                C = c;
            }
        }

        /// <summary>
        /// Getting the character classes: [0-9], [a-zA-Z], [a-z], [A-Z], [0-9a-zA-Z], whitespace.
        /// </summary>
        private class Token
        {
            //Nothing...yet.
        }

        /// <summary>
        /// Token sequence object. Also the regular expressions.
        /// </summary>
        private class TokenSeq
        {
            public List<Token> Tokens { get; private set; }

            public TokenSeq(params Token[] tokens)
            {
                for (int i = 0; i < tokens.Length; ++i)
                    Tokens.Add(tokens[i]);
            }
        }

        /// <summary>
        /// Holds all the variations of the Intersect algorithm, as described in Figure 4.
        /// </summary>
        private class Intersector
        {
            public DAG Intersect(DAG dag1, DAG dag2)
            {
                throw new NotImplementedException();

                //DAG(nodeSet1 x nodeSet2, (nodeStart1, nodeStart2), (nodeEnd1, nodeEnd2), edgeSet12, mapping12),
                //  where edgeSet12 = {<(node1, node2), (nodePrime1, nodePrime2)> | <node1, nodePrime1> in edgeSet1, <node2, nodePrime2> in edgeSet2},
                //  and mapping12(<(node1, node2), (nodePrime1, nodePrime2)>) = { Intersect( funcSet, funcPrimeSet) | funcSet in W1(<node1, nodePrime1>) funcPrimeSet in W2(<node2, nodePrime2>)}
            }

            public SubStr Intersect(SubStr sub1, SubStr sub2)
            {
                throw new NotImplementedException();

                //{IntersectPos(posSetk, posSetM)}k,m
            }

            public ConstStr Intersect(ConstStr con1, ConstStr con2)
            {
                if (con1.String.Equals(con2.String))
                    return con1;
                else
                    return null;
            }

            public Loop Intersect(Loop loop1, Loop loop2)
            {
                return new Loop(Intersect(loop1.Dag, loop2.Dag));
            }

            public CPos IntersectPos(CPos cpos1, CPos cpos2)
            {
                if (cpos1.K == cpos2.K)
                    return cpos1;
                else
                    return null;
            }

            public Pos IntersectPos(Pos pos1, Pos pos2)
            {
                return new Pos(IntersectRegex(pos1.R1, pos2.R1), IntersectRegex(pos1.R2, pos2.R2), pos1.C * pos2.C); //wrong, but whatever for now.
            }

            public TokenSeq IntersectRegex(TokenSeq seq1, TokenSeq seq2)
            {
                throw new NotImplementedException();

                //each of the sequences supposed to have set of sequences???
            }
        }

        /// <summary>
        /// Holds all the size methods, as described in Figure 5.
        /// </summary>
        private class Sizer
        {
            public int Size(Switch s)
            {
                throw new NotImplementedException();
            }

            public int Size(DAG dag)
            {
                throw new NotImplementedException();
            }

            public int Size(SubStr s)
            {
                throw new NotImplementedException();
            }

            public int Size(Loop loop)
            {
                throw new NotImplementedException();
            }

            public int Size(ConstStr str)
            {
                throw new NotImplementedException();
            }

            public int Size(CPos cpos)
            {
                throw new NotImplementedException();
            }

            public int Size(Pos pos)
            {
                throw new NotImplementedException();
            }

            public int Size(TokenSeq seq)
            {
                throw new NotImplementedException();
            }
        }
    }
}
