﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LRGenerator.Terminal;
using static LRGenerator.Nonterminal;
using static LRGenerator.ActionType;

namespace LRGenerator
{
    public class SLRGrammar : LRkGrammar
    {
        public LRItemSet[] ReduceReduceConflicts()
        {
            return States
                .Where(rs => rs.Count(s => s.Length == s.Marker) > 1)
                .Select(rs => new LRItemSet(rs.Where(s => s.Length == s.Marker || !s.IsKernel)))
                .Distinct()
                .ToArray();
        }

        public Tuple<LRItem, Symbol>[] ShiftReduceConflicts()
        {
            var toReturn = new List<Tuple<LRItem, Symbol>>();
            foreach (var s in States)
            {
                foreach (var i in s)
                {
                    if (i.Length == i.Marker
                        && !i.Rule.Production.Rules.Any(r => r.IsAccepting))
                    {
                        foreach (var sym in Follow[i.Rule.Production.Lhs])
                        {
                            var transitionKey = new Tuple<int, Symbol>(s.Index, sym);
                            if (GotoSymbol.ContainsKey(transitionKey))
                            {
                                toReturn.Add(new Tuple<LRItem, Symbol>(i, sym));
                            }
                        }
                    }
                }
            }
            return toReturn.Distinct().ToArray();

        }

        protected override LRItemSetCollection ComputeItemSetCollection()
        {
            return ComputeLR0ItemSetCollection();
        }

        protected override LRkParseTable ComputeParseTable()
        {
            var table = new LRkParseTable();
            table.StartState = States.StartState.Index;

            foreach (var state in States)
            {
                foreach (var sym in Symbols)
                {
                    if (sym.IsTerminal)
                    {
                        var key = new Tuple<int, Terminal>(state.Index, sym.Terminal);

                        foreach (var item in state)
                        {
                            // Preferring shift over reduce here (though undefined for SLR)
                            if (item.Marker < item.Length && item.Rule.Symbols[item.Marker].Equals(sym))
                            {
                                int @goto;
                                if (GotoSymbol.TryGetValue(new Tuple<int, Symbol>(state.Index, sym), out @goto))
                                {
                                    table.Action[key] = new Action(Shift, @goto);
                                }
                            }
                            else if (item.Length == item.Marker)
                            {
                                if (item.Rule.IsAccepting)
                                {
                                    if (sym.Terminal == Eof)
                                        table.Action[key] = new Action(ActionType.Accept);
                                }
                                else if (Follow[item.Rule.Production.Lhs].Contains(sym.Terminal))
                                    table.Action[key] = new Action(Reduce, item.Rule.Index);

                                // else don't add, will be error by default
                            }
                        }
                    }
                    else // Nonterminal
                    {
                        int @goto;
                        if (GotoSymbol.TryGetValue(new Tuple<int, Symbol>(state.Index, sym), out @goto))
                        {
                            table.Goto[new Tuple<int, Nonterminal>(state.Index, sym.Nonterminal)] = @goto;
                        }
                    }
                }
            }

            return table;
        }

        protected override Dictionary<Tuple<int, Symbol>, int> ComputeGotoLookup()
        {
            return ComputeLR0GotoLookup(States);
        }
    }
}