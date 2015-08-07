﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LRGenerator.Terminal;
using static LRGenerator.Nonterminal;

namespace LRGenerator
{
    class Program
    {
        public static void Main()
        {
            // Demo(new MyGrammar(), "var t = x + 3; if (y) { t += 1; }");

            Demo(new MyTestGrammar(), "if (5) { g<a, b>(c); } List<int> numbers = 3;");

            Console.Write("Press any key to continue...");
            Console.ReadKey(true);
        }

        private static void Demo(LRkGrammar grammar, string toParse)
        {
            Console.WriteLine($"{grammar.GetType().Name}:");
            Console.WriteLine();

            var t = grammar.States;

            var hasConflicts = false;

            var slr = grammar as SLRGrammar;
            Tuple<LRItem, Symbol>[] srConflicts = null;
            LRItemSet[] rrConflicts = null;

            if (slr != null)
            {
                srConflicts = slr.ShiftReduceConflicts();
                rrConflicts = slr.ReduceReduceConflicts();
                hasConflicts = srConflicts.Length > 0 || rrConflicts.Length > 0;
            }

            if (!hasConflicts)
            {
                while (!string.IsNullOrEmpty(toParse))
                {
                    var p = new LRkParser(grammar, toParse);
                    var ast = p.ParseAst();

                    if (p.Errors.Count > 0)
                    {
                        foreach (var error in p.Errors)
                            Console.WriteLine(error);
                    }
                    else
                    {
                        var sw = new StringWriter();
                        ast.Print(sw);
                        Console.WriteLine(sw);
                    }

                    Console.Write(" > ");
                    toParse = Console.ReadLine();
                }

                Console.WriteLine();
            }
            else
            {
                if (srConflicts?.Length > 0)
                {
                    Console.WriteLine("Shift/Reduce conflicts (would favor shift):");

                    foreach (var conflict in srConflicts)
                    {
                        Console.WriteLine($"  {conflict}");
                    }
                    Console.WriteLine();
                }
                if (rrConflicts?.Length > 0)
                {
                    Console.WriteLine("Reduce/Reduce conflicts (critical):");

                    for (var i = 0; i < rrConflicts.Length; i++)
                    {
                        Console.WriteLine($"=> {i}");

                        var conflictSet = rrConflicts[i].OrderBy(c => c.Marker).ToArray();

                        Console.WriteLine("  Nonkernel:");
                        foreach (var item in conflictSet.Where(it => !it.IsKernel))
                        {
                            Console.WriteLine($"    {item}");
                        }

                        Console.WriteLine("  Kernel:");
                        foreach (var item in conflictSet.Where(it => it.IsKernel))
                        {
                            Console.WriteLine($"    {item}");
                        }
                    }
                    Console.WriteLine();
                }

                Console.WriteLine();
            }
        }
    }

    public class MyGrammar : SLRGrammar
    {
        public MyGrammar()
            : base()
        {
            var start = DefineProduction(Start);
            var stmtList = DefineProduction(StatementList);
            var stmt = DefineProduction(Statement);
            var simpleStmt = DefineProduction(SimpleStatement);
            var expression = DefineProduction(Expression);
            var assignment = DefineProduction(Assignment);
            var assignExpr = DefineProduction(AssignmentExpression);
            var compare = DefineProduction(Compare);
            var term = DefineProduction(Term);
            var factor = DefineProduction(Factor);
            var assignType = DefineProduction(AssignmentType);

            ProductionRule t;
            
            t = start
                    % StatementList;

            // 
            // Statements
            t = stmtList
                    % StatementList / Statement;
            t = stmtList
                    % Statement;
            t = simpleStmt
                    % Assignment;
            //
            // Assignments
            t = assignType % Var / Ident;
            t = assignType % Ident;
            t = assignment
                    % AssignmentType / Terminal.Equals / AssignmentExpression;
            t = assignment
                    % AssignmentType / PlusEquals / AssignmentExpression;
            t = stmt
                    % SimpleStatement / Semicolon;
            t = stmt
                    % If / LeftParen / AssignmentExpression / RightParen / Statement;
            t = stmt
                    % LeftBrace / StatementList / RightBrace;
            t = assignExpr
                    % Ident / Terminal.Equals / AssignmentExpression;
            t = assignExpr
                    % Ident / PlusEquals / AssignmentExpression;
            t = assignExpr
                    % Compare;
            t = compare
                    % Compare / LeftAngle / Expression;
            t = compare
                    % Compare / RightAngle / Expression;
            t = compare
                    % Expression;
            t = expression
                    % Expression / Plus / Term;
            t = expression
                    % Expression / Minus / Term;
            t = expression
                    % Term;
            t = term
                    % Term / Star / Factor;
            t = term
                    % Term / Slash / Factor;
            t = term
                    % Factor;
            t = factor
                    % LeftParen / Expression / RightParen;
            t = factor
                    % Ident;
            t = factor
                    % Number;
        }
    }

    public class MyTestGrammar : LALRGrammar
    {
        public MyTestGrammar()
            : base()
        {
            var start = DefineProduction(Start);
            var stmtList = DefineProduction(StatementList);
            var stmt = DefineProduction(Statement);
            var simpleStmt = DefineProduction(SimpleStatement);
            var optExpr = DefineProduction(OptionalExpression);
            var optStmt = DefineProduction(OptionalSimpleStatement);
            var expression = DefineProduction(Expression);
            var assignment = DefineProduction(Assignment);
            var assignExpr = DefineProduction(AssignmentExpression);
            var compare = DefineProduction(Compare);
            var term = DefineProduction(Term);
            var factor = DefineProduction(Factor);
            var typeName = DefineProduction(TypeName);
            var simpleName = DefineProduction(SimpleName);
            var typeParams = DefineProduction(TypeParameters);
            var typeParamList = DefineProduction(TypeParameterList);
            var call = DefineProduction(Call);
            var param = DefineProduction(Parameters);
            var paramList = DefineProduction(ParameterList);
            var assignType = DefineProduction(AssignmentType);

            ProductionRule t;
            
            t = start
                    % StatementList;
            // 
            // Statements
            t = stmtList
                    % StatementList / Statement;
            t = stmtList
                    % Statement;
            t = stmtList
                    % null;
            t = optStmt
                    % SimpleStatement;
            t = optStmt
                    % null;
            t = simpleStmt
                    % Assignment;
            t = simpleStmt
                    % Call;
            //
            // TypeName
            t = typeName % Int;
            t = typeName % Float;
            t = typeName % Bool;
            t = typeName % Terminal.String;
            t = typeName % SimpleName;
            t = typeName % SimpleName / TypeParameters;
            t = typeParams % LeftAngle / TypeParameterList / RightAngle;
            t = typeParamList % TypeParameterList / Comma / TypeName;
            t = typeParamList % TypeName;
            //
            // Call
            t = call % TypeName / Parameters;
            t = param % LeftParen / ParameterList / RightParen;
            t = paramList % ParameterList / Comma / AssignmentExpression;
            t = paramList % AssignmentExpression;
            t = paramList % null;
            //
            // Assignments
            t = assignType % Var;
            t = assignType % TypeName;
            t = assignment
                    % AssignmentType / SimpleName / Terminal.Equals / AssignmentExpression;
            t = assignment
                    % AssignmentType / SimpleName / PlusEquals / AssignmentExpression;
            t = stmt
                    % SimpleStatement / Semicolon;
            t = stmt
                    % If / LeftParen / AssignmentExpression / RightParen / Statement;
            t = stmt
                    % For / LeftParen / OptionalSimpleStatement / Semicolon / OptionalExpression / Semicolon / OptionalSimpleStatement / RightParen / Statement;
            t = stmt
                    % LeftBrace / StatementList / RightBrace;
            t = optExpr
                    % AssignmentExpression;
            t = optExpr
                    % null;
            t = assignExpr
                    % SimpleName / Terminal.Equals / AssignmentExpression;
            t = assignExpr
                    % SimpleName / PlusEquals / AssignmentExpression;
            t = assignExpr
                    % Compare;
            t = compare
                    % Compare / LeftAngle / Expression;
            t = compare
                    % Compare / RightAngle / Expression;
            t = compare
                    % Expression;
            t = expression
                    % Expression / Plus / Term;
            t = expression
                    % Expression / Minus / Term;
            t = expression
                    % Term;
            t = term
                    % Term / Star / Factor;
            t = term
                    % Term / Slash / Factor;
            t = term
                    % Factor;
            t = factor
                    % LeftParen / Expression / RightParen;
            t = factor
                    % SimpleName;
            t = simpleName
                    % Ident;
            t = factor
                    % Number;
            //t = factor % Call;
        }
    }
}