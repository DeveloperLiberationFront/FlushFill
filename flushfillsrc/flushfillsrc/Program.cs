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
                        var traceSet = new Intersector().Unify(traceSet1, traceSet2);

                        Loop loop = new Loop(traceSet);
                        if (loop.Apply(input).Equals(output.Substring(i, k- i)))
                        {
                            Edge targetEdge = new Edge(i, k);
                            //mapping[targetEdge] = mapping[targetEdge] U loop;
                        }
                    }
                }
            }

            return mapping;
        }

        private void GenerateSubstring(string input, string output)
        {
            throw new NotImplementedException();

            //result = null;
            //foreach (i, k) such that s is substring of sigma(vi) at position k
            //  Y1 = GeneratePosition(sigma(vi), k);
            //  Y2 = GeneratePosition(sigma(vi), k + Length(s));
            //  result = result U PSubStr(v1, Y1, Y2)};
            //return result;

            HashSet<string> result = new HashSet<string>();
        }

        private void GeneratePosition(string s, int k)
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

            HashSet<CPos> result = new HashSet<CPos>();
            result.Add(new CPos(k));
            result.Add(new CPos(-(s.Length - k)));
        }

        private void GenerateRegex(TokenSeq r, string s)
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
        private class Edge : IEquatable<Edge>
        {
            public int Start { get; private set; }
            public int End { get; private set; }

            public Edge(int start, int end)
            {
                Start = start;
                End = end;
            }

            public bool Equals(Edge other)
            {
                return Start == other.Start && End == other.End;
            }

            public override int GetHashCode()
            {
                return Start.GetHashCode() ^ End.GetHashCode();
            }
        }

        /// <summary>
        /// Necessary for whatever the edges in the DAG represent...
        /// </summary>
        private class Operation
        {
            //Nothing...yet.
        }

        public interface IApplicable<T>
        {
            T Apply(string s);
        }

        public interface IExpandable<T>
        {
            HashSet<T> Expand();
        }

        /// <summary>
        /// The CONCATENATE expression!
        /// </summary>
        private class Trace : IApplicable<string>
        {
            private List<Atomic<?, ?>> Actions { get; private set; }

            public string Apply(string s)
            {
                throw new NotImplementedException();
            }
        }

        ///FIGURE 1///

        /// <summary>
        /// Switches: conditions and results.
        /// </summary>
        private class Switch : IApplicable<string>
        {
            public string Apply(string s)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// When all of the trace expressions are sets (e with ~) in Figure 3
        /// </summary>
        private class SwitchSet : IExpandable<Switch>
        {
            public HashSet<Switch> Expand()
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// A collection of conjunctions OR'd together.
        /// </summary>
        private class Bool : IApplicable<bool>
        {
            public HashSet<Conjunct> Conjuncts { get; private set; }

            public Bool(params Conjunct[] conjuncts)
            {
                foreach (Conjunct c in conjuncts)
                    Conjuncts.Add(c);
            }

            public bool Apply(string s)
            {
                foreach (Conjunct conjuct in Conjuncts)
                    if (conjuct.Apply(s))
                        return true;
                return false;
            }
        }

        /// <summary>
        /// A bunch of predicates AND'd together.
        /// </summary>
        private class Conjunct : IApplicable<bool>
        {
            public HashSet<Predicate> Predicates { get; private set; }

            public Conjunct(params Predicate[] predicates)
            {
                foreach (Predicate pred in predicates)
                    Predicates.Add(pred);
            }

            public bool Apply(string s)
            {
                foreach (Predicate pred in Predicates)
                    if (!pred.Apply(s))
                        return false;
                return true;
            }
        }

        /// <summary>
        /// Either Matching a regular expression or not matching.
        /// TODO: Probably redundant, since I included the Matching field in Match.
        /// </summary>
        private class Predicate : IApplicable<bool>
        {
            public Match ThisMatch { get; private set; }

            public Predicate(Match match)
            {
                ThisMatch = match;
            }

            public bool Apply(string s)
            {
                return ThisMatch.Apply(s);
            }
        }

        /// <summary>
        /// (v, r, k) -> The string v has at least k instance of the token sequence r.
        /// </summary>
        private class Match : IApplicable<bool>
        {
            public bool Matching { get; private set; }
            public string V { get; private set; }
            public TokenSeq R { get; private set; }
            public int K { get; private set; }

            /// <summary>
            /// "matching" refers to whether this intends to find a match or the string that does not match.
            /// </summary>
            public Match(bool matching, string v, TokenSeq r, int k)
            {
                Matching = matching;
                V = v;
                R = r;
                K = k;
            }

            public bool Apply(string s)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// The directed acyclic graph which takes you from the start node to the end...whatever that means.
        /// </summary>
        private class DAG : IExpandable<Trace>
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

            public HashSet<Trace> Expand()
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// The base class for SubStr, ConstStr, and Loop.
        /// </summary>
        private abstract class Atomic : IEquatable<Atomic>, IApplicable<string>
        {
            public abstract string Apply(string s);
            public abstract bool Equals(Atomic other);
            public abstract override int GetHashCode();
        }

        /// <summary>
        /// SubString object.
        /// </summary>
        private class SubStr : Atomic 
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

            public override bool Equals(Atomic other)
            {
                if (other is SubStr)
                {
                    SubStr otherS = (SubStr)other;
                    return String.Equals(otherS.String) && Pos1 == otherS.Pos1 && Pos2 == otherS.Pos2;
                }
                else
                    return false;
            }

            public override int GetHashCode()
            {
                return String.GetHashCode() ^ Pos1.GetHashCode() ^ Pos2.GetHashCode();
            }

            public override string Apply(string s)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Constant string object.
        /// </summary>
        private class ConstStr : Atomic
        {
            public string String { get; private set; }

            public ConstStr(string s)
            {
                String = s;
            }

            public override bool Equals(Atomic other)
            {
                if (other is ConstStr)
                {
                    return String.Equals(((ConstStr)other).String);
                }
                else
                    return false;
            }

            public override int GetHashCode()
            {
                throw new NotImplementedException();
            }

            public override string Apply(string s)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Loop object.
        /// </summary>
        private class Loop : Atomic 
        {
            public DAG Dag { get; private set; }

            public Loop(DAG dag)
            {
                Dag = dag;
            }

            public override bool Equals(Atomic other)
            {
                if (other is Loop)
                    return Dag.Equals(((Loop)other).Dag);
                else
                    return false;
            }

            public override int GetHashCode()
            {
                return Dag.GetHashCode();
            }

            public override string Apply(string s)
            {
                throw new NotImplementedException();
            }
        }

        private abstract class Position : IEquatable<Position>
        {
            public abstract bool Equals(Position other);
            public abstract override int GetHashCode();
        }

        /// <summary>
        /// CPos object.
        /// </summary>
        private class CPos : Position
        {
            public int K { get; private set; }

            public CPos(int k)
            {
                K = k;
            }

            public override bool Equals(Position other)
            {
                if (other is CPos)
                    return K == ((CPos)other).K;
                else
                    return false;
            }

            public override int GetHashCode()
            {
                return K.GetHashCode();
            }
        }

        /// <summary>
        /// The Position function.
        /// </summary>
        private class Pos : Position
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

            public override bool Equals(Position other)
            {
                if (other is Pos)
                {
                    Pos otherP = (Pos)other;
                    return R1.Equals(otherP.R1) && R2.Equals(otherP.R2) && C == otherP.C;
                }
                else
                    return false;
            }

            public override int GetHashCode()
            {
                return R1.GetHashCode() ^ R2.GetHashCode() ^ C.GetHashCode();
            }
        }

        private enum TokenClass { Digit, Alphabet, Lowercase, Uppercase, Alphanumeric, Whitespace, Start, End, Other }; //TODO: Expand on "Other"

        /// <summary>
        /// Getting the character classes: [0-9], [a-zA-Z], [a-z], [A-Z], [0-9a-zA-Z], whitespace.
        /// </summary>
        private class Token : IEquatable<Token>
        {
            public TokenClass TClass { get; private set; }

            public Token(TokenClass clazz)
            {
                TClass = clazz;
            }

            public bool Equals(Token other)
            {
                TokenClass otherClass = other.TClass;
                switch (TClass)
                {
                    case TokenClass.Digit:
                        return otherClass == TokenClass.Digit;
                    case TokenClass.Alphabet:
                        return otherClass == TokenClass.Alphabet || otherClass == TokenClass.Lowercase || otherClass == TokenClass.Uppercase;
                    case TokenClass.Lowercase:
                        return otherClass == TokenClass.Lowercase;
                    case TokenClass.Uppercase:
                        return otherClass == TokenClass.Uppercase;
                    case TokenClass.Alphanumeric:
                        return otherClass == TokenClass.Digit || otherClass == TokenClass.Alphabet
                                          || otherClass == TokenClass.Lowercase || otherClass == TokenClass.Uppercase;
                    case TokenClass.Whitespace:
                        return otherClass == TokenClass.Whitespace;
                    case TokenClass.Start:
                        return otherClass == TokenClass.Start;
                    case TokenClass.End:
                        return otherClass == TokenClass.End;
                    case TokenClass.Other:
                        return otherClass == TokenClass.Other;
                    default:
                        return true; //Just assume the class to be any kind of character.
                }
            }

            public override int GetHashCode()
            {
                return TClass.GetHashCode();
            }
        }

        /// <summary>
        /// Token sequence object. Also the regular expressions.
        /// </summary>
        private class TokenSeq : IEquatable<TokenSeq>
        {
            public List<Token> Tokens { get; private set; }

            public TokenSeq(params Token[] tokens)
            {
                for (int i = 0; i < tokens.Length; ++i)
                    Tokens.Add(tokens[i]);
            }

            public bool Equals(TokenSeq other)
            {
                List<Token> otherTokens = other.Tokens;
                if (Tokens.Count != otherTokens.Count)
                    return false;
                else
                {
                    for (int i = 0; i < Tokens.Count; ++i)
                        if (!Tokens[i].Equals(otherTokens[i]))
                            return false;

                    return true;
                }
            }

            public override int GetHashCode()
            {
                int hash = 0;
                foreach (Token token in Tokens)
                    hash ^= token.GetHashCode();
                return hash;
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

            /////////UNIFY//////////

            public DAG Unify(DAG dag1, DAG dag2)
            {
                throw new NotImplementedException();

                //DAG(nodeSet1 x nodeSet2, (nodeStart1, nodeStart2), (nodeEnd1, nodeEnd2), edgeSet12, mapping12),
                //  where edgeSet12 = {<(node1, node2), (nodePrime1, nodePrime2)> | <node1, nodePrime1> in edgeSet1, <node2, nodePrime2> in edgeSet2},
                //  and mapping12(<(node1, node2), (nodePrime1, nodePrime2)>) = { Intersect( funcSet, funcPrimeSet) | funcSet in W1(<node1, nodePrime1>) funcPrimeSet in W2(<node2, nodePrime2>)}
            }

            public SubStr Unify(SubStr sub1, SubStr sub2)
            {
                throw new NotImplementedException();

                //{IntersectPos(posSetk, posSetM)}k,m
            }

            public ConstStr Unify(ConstStr con1, ConstStr con2)
            {
                if (con1.String.Equals(con2.String))
                    return con1;
                else
                    return null;
            }

            public Loop Unify(Loop loop1, Loop loop2)
            {
                return new Loop(Unify(loop1.Dag, loop2.Dag));
            }

            public CPos Unify(CPos cpos1, CPos cpos2)
            {
                if (cpos1.K == cpos2.K)
                    return cpos1;
                else
                    return null;
            }

            public Pos Unify(Pos pos1, Pos pos2)
            {
                return new Pos(UnifyRegex(pos1.R1, pos2.R1), UnifyRegex(pos1.R2, pos2.R2), pos1.C * pos2.C); //wrong, but whatever for now.
            }

            public TokenSeq UnifyRegex(TokenSeq seq1, TokenSeq seq2)
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
