﻿using ParserGenerator.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParserGenerator
{
    public abstract partial class GrammarBase<Terminal_T, Nonterminal_T>
    {
        public class AstNode
        {
            public AstNode(Symbol s)
            {
                Symbol = s;
                Children = new AstNode[0];
            }

            public AstNode(Symbol s, AstNode[] children)
                : this(s)
            {
                Children = children;
            }

            public Symbol Symbol { get; }
            public AstNode[] Children { get; set; }

            public override string ToString()
            {
                return Children == null || Children.Length == 0
                    ? Symbol.ToString()
                    : string.Format("{0} => {1}", Symbol, string.Join(" ", Children.Select(c => c.Symbol)));
            }

            public void Print(TextWriter w)
            {
                if (Children == null || Children.Length == 0)
                {
                    w.Write(Symbol.Token?.ToString()??Symbol.ToString());
                }
                else
                {
                    for (var i = 0; i < Children.Length; i++)
                    {
                        var c = Children[i];
                        if (i != 0)
                            w.Write(" ");
                        c.Print(w);
                    }
                }
            }

            private IEnumerable<Token> FlattenToEnumerable()
            {
                if (Symbol != null && Symbol.IsTerminal)
                    return Symbol.Token.Yield();
                else
                    return Children.SelectMany(c => c.FlattenToEnumerable());
            }

            public IPeekable<Token> Flatten()
            {
                return FlattenToEnumerable().AsPeekable();
            }

            public AstNode Search(int start, params Symbol[] types)
            {
                if (types.Contains(Symbol))
                    return this;

                foreach (var c in Children.Skip(start))
                {
                    var found = c.Search(0, types);
                    if (found != null)
                        return found;
                }
                return null;
            }

            public AstNode Search(Symbol type, int start = 0)
            {
                if (type.Equals(Symbol))
                    return this;

                foreach (var c in Children.Skip(start))
                {
                    var found = c.Search(type, start);
                    if (found != null)
                        return found;
                }
                return null;
            }

            public IEnumerable<AstNode> SearchAll(Symbol type, int start = 0)
            {
                if (type.Equals(Symbol))
                {
                    yield return this;
                    yield break;
                }

                foreach (var c in Children.Skip(start))
                {
                    foreach (var found in c.SearchAll(type))
                    {
                        yield return found;
                    }
                }
            }

            public AstNode Search(params Symbol[] types)
            {
                return Search(0, types);
            }
        }

    }
}
