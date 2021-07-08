using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace treetop
{
    /// <summary>
    /// Implementations of several context-free grammar transformations.
    /// </summary>
    public static class CFGTransformations
    {
        /// <summary>
        /// Transforms grammar according to given grammar options.
        /// </summary>
        /// <param name="grammar">target grammar</param>
        /// <param name="options">set of options</param>
        /// <returns>application of options to grammar</returns>
        public static CFG ApplyOptions(this CFG grammar, OrderedHashSet<GrammarOption> options)
        {
            if (options.Contains(GrammarOption.Reversed))
            {
                return Reversed(grammar);
            }
            return grammar;
        }
        /// <summary>
        /// Simplify context-free grammar by removing unreachable symbols and productions.
        /// </summary>
        /// <param name="grammar">target grammar</param>
        /// <returns>simplified grammar</returns>
        public static CFG Simplify(this CFG grammar)
        {
            OrderedHashSet<Production> productions = new OrderedHashSet<Production>();
            OrderedHashSet<string> newSymbols = new OrderedHashSet<string>(), seenSymbols = new OrderedHashSet<string>();
            newSymbols.Add(grammar.startVariable);
            while (newSymbols.Count() > 0)
            {
                foreach (string symbol in newSymbols)
                    seenSymbols.Add(symbol);
                OrderedHashSet<string> nextSymbols = new OrderedHashSet<string>();
                foreach (Production production in grammar.productions)
                    if (newSymbols.Contains(production.lhs))
                    {
                        productions.Add(production);
                        foreach (string symbol in production.rhs)
                            if (!seenSymbols.Contains(symbol))
                                nextSymbols.Add(symbol);
                    }
                newSymbols = nextSymbols;
            }
            return verifyConsistency(grammar, new CFG(grammar.startVariable, productions));
        }
        /// <summary>
        /// Reverses context-free grammar, by reversing each of its productions.
        /// </summary>
        /// <param name="grammar">target grammar</param>
        /// <returns>reversed grammar</returns>
        public static CFG Reversed(this CFG grammar)
        {
            OrderedHashSet<Production> productions = new OrderedHashSet<Production>();
            foreach (Production production in grammar.productions)
            {
                string[] reversed = new string[production.rhs.Length];
                Array.Copy(production.rhs, reversed, reversed.Length);
                Array.Reverse(reversed);
                productions.Add(new Production(production.lhs, reversed));
            }
            return new CFG(grammar.startVariable, productions);
        }
        /// <summary>
        /// If the start variable `S` appears in production left-hand side,
        /// introduce new start variable `S'` with production `S' -> S`.
        /// </summary>
        /// <param name="grammar">target grammar</param>
        /// <returns>grammar with start variable not in any RHS</returns>
        public static CFG RemoveInitialVariableFromRHS(this CFG grammar)
        {
            bool noInitialVariableInRHS = true;
            foreach (Production production in grammar.productions)
                foreach (string symbol in production.rhs)
                    if (grammar.startVariable == symbol)
                        noInitialVariableInRHS = false;
            if (noInitialVariableInRHS)
                return grammar;
            string startVariable = GenerateAuxiliaryVariable(grammar.Symbols(), grammar.startVariable);
            OrderedHashSet<Production> productions = new OrderedHashSet<Production>();
            productions.Add(new Production(startVariable, grammar.startVariable));
            foreach (Production production in grammar.productions)
                productions.Add(production);
            return new CFG(startVariable, productions);
        }
        /// <summary>
        /// Remove all epsilon productions, except for the start variable.
        /// </summary>
        /// <param name="grammar">target grammar</param>
        /// <returns>grammar without epsilon productions</returns>
        public static CFG RemoveEpsilonProductions(this CFG grammar)
        {
            OrderedHashSet<Production> productions = grammar.productions;
            while (true)
            {
                OrderedHashSet<string> nullableVariables = new OrderedHashSet<string>(), nullVariables = new OrderedHashSet<string>();
                foreach (Production production in productions)
                    if (production.IsEpsilonProduction())
                    {
                        nullableVariables.Add(production.lhs);
                        nullVariables.Add(production.lhs);
                    }
                foreach (Production production in productions)
                    if (!production.IsEpsilonProduction())
                        nullVariables.Remove(production.lhs);
                if (nullableVariables.Count == 0 || nullableVariables.Count == 1 && nullableVariables.Contains(grammar.startVariable))
                    return verifyConsistency(grammar, new CFG(grammar.startVariable, productions));
                OrderedHashSet<Production> newProductions = new OrderedHashSet<Production>();
                foreach (Production production in productions)
                {
                    OrderedHashSet<List<string>> rhss = new OrderedHashSet<List<string>>();
                    rhss.Add(new List<string>());
                    foreach (string symbol in production.rhs)
                    {
                        OrderedHashSet<List<string>> newRHSs = new OrderedHashSet<List<string>>();
                        foreach (var rhs in rhss)
                            if (nullVariables.Contains(symbol))
                                newRHSs.AddUnique(rhs);
                            else if (!nullableVariables.Contains(symbol))
                            {
                                List<string> newRHS = new List<string>();
                                newRHS.AddRange(rhs);
                                newRHS.Add(symbol);
                                newRHSs.Add(newRHS);
                            }
                            else
                            {
                                newRHSs.AddUnique(rhs);
                                List<string> newRHS = new List<string>();
                                newRHS.AddRange(rhs);
                                newRHS.Add(symbol);
                                newRHSs.Add(newRHS);
                            }
                        rhss = newRHSs;
                    }
                    foreach (var newRHS in rhss)
                        newProductions.Add(new Production(production.lhs, newRHS.ToArray()));
                }
                foreach (string nullableVariable in nullableVariables)
                    if (nullableVariable != grammar.startVariable)
                        newProductions.Remove(new Production(nullableVariable));
                productions = newProductions;
            }
        }
        private static void AddUnique(this OrderedHashSet<List<string>> set, List<string> item)
        {
            foreach (var existingItem in set)
                if (item.SequenceEqual(existingItem))
                    return;
            set.Add(item);
        }
        /// <summary>
        /// Removes left-recursive productions from the grammar.
        /// Implementation of Paull's algorithm.
        /// </summary>
        /// <param name="grammar">target grammar</param>
        /// <returns>grammar without left recursion</returns>
        public static CFG RemoveLeftRecursion(this CFG grammar)
        {
            if (!grammar.HasLeftRecursion())
                return grammar;
            OrderedHashSet<Production> productions = new OrderedHashSet<Production>();
            List<string> variables = new List<string>(grammar.Variables());
            OrderedHashSet<string> usedSymbols = new OrderedHashSet<string>();
            foreach (string symbol in grammar.Symbols())
                usedSymbols.Add(symbol);
            for (int i = 0; i < variables.Count; ++i)
            {
                string vi = variables[i];
                OrderedHashSet<Production> rawProductions = new OrderedHashSet<Production>();
                foreach (Production production in grammar.productions)
                    if (production.lhs == vi)
                        rawProductions.Add(production);
                for (int j = 0; j < i; ++j)
                {
                    string vj = variables[j];
                    foreach (Production production in grammar.productions)
                        if (production.lhs == vi && production.rhs.Length > 0 && production.rhs[0] == vj)
                        {
                            rawProductions.Remove(production);
                            foreach (Production otherProduction in grammar.productions)
                                if (otherProduction.lhs == vj)
                                {
                                    List<string> newRHS = new List<string>();
                                    newRHS.AddRange(otherProduction.rhs);
                                    newRHS.AddRange(production.rhs.Skip(1));
                                    rawProductions.Add(new Production(vi, newRHS.ToArray()));
                                }
                        }
                }
                foreach (Production production in RemoveDirectLeftRecursion(usedSymbols, rawProductions))
                    productions.Add(production);
            }
            return verifyConsistency(grammar, new CFG(grammar.startVariable, productions));
        }
        /// <summary>
        /// Checks whether the grammar has a left-recursive production, i.e.,
        /// of the form `v ::= v s1 s2...`.
        /// </summary>
        /// <param name="grammar">context-free grammar</param>
        /// <returns>whether grammar has left-recursion</returns>
        public static bool HasLeftRecursion(this CFG grammar)
        {
            foreach (Production production in grammar.productions)
                if (production.rhs.Length > 0 && production.rhs[0] == production.lhs)
                    return true;
            return false;
        }
        private static OrderedHashSet<Production> RemoveDirectLeftRecursion(OrderedHashSet<string> usedSymbols, OrderedHashSet<Production> productions)
        {
            OrderedHashSet<Production> newProductions = new OrderedHashSet<Production>();
            foreach (Production production in productions)
                newProductions.Add(production);
            if (newProductions.Count == 0)
                return newProductions;
            string lhs = newProductions[0].lhs;
            foreach (Production production in newProductions)
                Debug.Assert(production.lhs == lhs);
            OrderedHashSet<Production> leftRecursiveProductions = new OrderedHashSet<Production>(),
                nonLeftRecursiveProductions = new OrderedHashSet<Production>();
            foreach (Production production in newProductions)
                if (production.rhs.Length == 0 || production.rhs[0] != lhs)
                    nonLeftRecursiveProductions.Add(production);
                else
                    leftRecursiveProductions.Add(production);
            if (leftRecursiveProductions.Count == 0)
                return newProductions;
            newProductions.Clear();
            string auxiliaryVariable = GenerateAuxiliaryVariable(usedSymbols, lhs);
            usedSymbols.Add(auxiliaryVariable);
            foreach (Production nonLeftRecursiveProduction in nonLeftRecursiveProductions)
            {
                newProductions.Add(nonLeftRecursiveProduction);
                List<string> newRHS = new List<string>();
                newRHS.AddRange(nonLeftRecursiveProduction.rhs);
                newRHS.Add(auxiliaryVariable);
                newProductions.Add(new Production(lhs, newRHS.ToArray()));
            }
            foreach (Production leftRecursiveProduction in leftRecursiveProductions)
            {
                List<string> newRHS = new List<string>();
                newRHS.AddRange(leftRecursiveProduction.rhs.Skip(1));
                if (newRHS.Count == 0)
                    continue;
                newProductions.Add(new Production(auxiliaryVariable, newRHS.ToArray()));
                newRHS.Add(auxiliaryVariable);
                newProductions.Add(new Production(auxiliaryVariable, newRHS.ToArray()));
            }
            return newProductions;
        }
        /// <summary>
        /// Transforms grammar into Greibach normal form, i.e.,
        /// where productions are of the form `v ::= t s1 s2...`, `v` a variable,
        /// `t` a terminal.
        /// The grammar's start variable may be nullable, `v0 ::= epsilon`.
        /// </summary>
        /// <param name="grammar">target grammar</param>
        /// <returns>grammar in Greibach normal form</returns>
        public static CFG ToGreibachNormalForm(this CFG grammar)
        {
            if (grammar.InGreibachNormalForm())
                return grammar;
            grammar = grammar.RemoveInitialVariableFromRHS();
            while (true)
            {
                grammar = grammar.RemoveEpsilonProductions().RemoveLeftRecursion();
                OrderedHashSet<Production> productions = new OrderedHashSet<Production>();
                foreach (Production production in grammar.productions)
                    if (production.IsEpsilonProduction() || grammar.Terminals().Contains(production.rhs[0]))
                        productions.Add(production);
                    else
                    {
                        string rhsStartVariable = production.rhs[0];
                        foreach (Production otherProduction in grammar.productions)
                            if (otherProduction.lhs == rhsStartVariable)
                            {
                                List<string> newRHS = new List<string>();
                                newRHS.AddRange(otherProduction.rhs);
                                newRHS.AddRange(production.rhs.Skip(1));
                                productions.Add(new Production(production.lhs, newRHS.ToArray()));
                            }
                    }
                grammar = verifyConsistency(grammar, new CFG(grammar.startVariable, productions));
                if (grammar.InGreibachNormalForm())
                    return grammar;
            }
        }
        /// <summary>
        /// Checks whether grammar is in Greibach normal form, i.e.,
        /// where productions are of the form `v ::= t s1 s2...`, `v` a variable,
        /// `t` a terminal.
        /// The grammar's start variable may be nullable, `v0 ::= epsilon`.
        /// </summary>
        /// <param name="grammar">context-free grammar</param>
        /// <returns>whether grammar is in Greibach normal form</returns>
        public static bool InGreibachNormalForm(this CFG grammar)
        {
            bool hasNilStartVariable = false;
            foreach (Production production in grammar.productions)
                if (production.IsEpsilonProduction())
                    if (production.lhs == grammar.startVariable)
                        hasNilStartVariable = true;
                    else
                        return false;
                else if (!grammar.Terminals().Contains(production.rhs[0]))
                    return false;
            if (hasNilStartVariable)
            {
                foreach (Production production in grammar.productions)
                    foreach (string symbol in production.rhs)
                        if (grammar.startVariable == symbol)
                            return false;
            }
            return true;
        }
        private static CFG verifyConsistency(CFG oldGrammar, CFG newGrammar)
        {
            OrderedHashSet<string> oldVariables = oldGrammar.Variables();
            foreach (string terminal in newGrammar.Terminals())
                if (oldVariables.Contains(terminal))
                    throw new Exception($"grammar is malformed: variable {terminal} cannot be derived");
            foreach (string variable in newGrammar.Variables())
            {
                bool variableIsDerived = false;
                foreach (Production production in newGrammar.productions)
                    if (production.lhs == variable)
                    {
                        variableIsDerived = true;
                        break;
                    }
                if (!variableIsDerived)
                    throw new Exception($"grammar is malformed: variable {variable} cannot be derived");
            }
            return newGrammar;
        }
        private static string GenerateAuxiliaryVariable(OrderedHashSet<string> usedSymbols, string sourceVariable)
        {
            if (!usedSymbols.Contains(sourceVariable))
                return sourceVariable;
            for (int i = 0; i < sourceVariable.Length; ++i)
                if (int.TryParse(sourceVariable.Substring(i), out int variableIndex))
                    return GenerateAuxiliaryVariable(usedSymbols, sourceVariable.Substring(0, i), variableIndex);
            return GenerateAuxiliaryVariable(usedSymbols, sourceVariable, 2);
        }
        private static string GenerateAuxiliaryVariable(OrderedHashSet<string> usedSymbols, string variableName, int indexHint)
        {
            int index = indexHint;
            while (index > 0)
            {
                string name = variableName + index;
                if (!usedSymbols.Contains(name))
                    return name;
                ++index;
            }
            throw new Exception("grammar transformation error");
        }
    }
}
