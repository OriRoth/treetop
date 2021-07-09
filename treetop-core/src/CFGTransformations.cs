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
        /// Simplify context-free grammar by removing unreachable symbols and productions and productions of
        /// the form V ::= V.
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
                    if (newSymbols.Contains(production.lhs) && (production.rhs.Length != 1 || production.rhs[0] != production.lhs))
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
        /// Remove epsilon productions.
        /// If the grammar derives epsilon, then only the start variable may derive epsilon, but then it
        /// would not be found on the right-hand side of any production.
        /// </summary>
        /// <param name="grammar">target grammar</param>
        /// <returns>grammar without epsilon productions</returns>
        public static CFG RemoveEpsilonProductions(this CFG grammar)
        {
            bool hasNullableStartSymbolInRHS = grammar.HasNullableStartSymbolInRHS();
            if (!grammar.HasEpsilonProductionsExceptStartVariable() && !hasNullableStartSymbolInRHS)
                return grammar;
            if (hasNullableStartSymbolInRHS)
            {
                OrderedHashSet<Production> newProductions = new OrderedHashSet<Production>();
                foreach (Production production in grammar.productions)
                    newProductions.Add(production);
                string newStartVariable = GenerateAuxiliaryVariable(grammar.Symbols(), grammar.startVariable);
                newProductions.Add(new Production(newStartVariable, grammar.startVariable));
                newProductions.Add(new Production(newStartVariable));
                return RemoveEpsilonProductions(new CFG(newStartVariable, newProductions));
            }
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
                    return verifyConsistency(grammar, new CFG(grammar.startVariable, productions).Simplify());
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
        private static bool HasEpsilonProductionsExceptStartVariable(this CFG grammar)
        {
            foreach (Production production in grammar.productions)
                if (production.lhs != grammar.startVariable && production.IsEpsilonProduction())
                    return true;
            return false;
        }
        private static bool HasNullableStartSymbolInRHS(this CFG grammar)
        {
            bool startVariableNullable = false;
            foreach (Production production in grammar.productions)
                if (production.IsEpsilonProduction())
                    if (production.lhs == grammar.startVariable)
                    {
                        startVariableNullable = true;
                        break;
                    }
            if (!startVariableNullable)
                return false;
            foreach (Production production in grammar.productions)
                foreach (string symbol in production.rhs)
                    if (symbol == grammar.startVariable)
                        return true;
            return false;
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
            grammar = grammar.Simplify().RemoveEpsilonProductions();
            if (!grammar.HasLeftRecursion())
                return grammar;
            bool epsilonIsDerived = grammar.productions.Contains(new Production(grammar.startVariable));
            OrderedHashSet<Production> productions = new OrderedHashSet<Production>();
            foreach (Production production in grammar.productions)
                productions.Add(production);
            if (epsilonIsDerived)
                productions.Remove(new Production(grammar.startVariable));
            List<string> variables = new List<string>(grammar.Variables());
            OrderedHashSet<string> usedSymbols = new OrderedHashSet<string>();
            foreach (string symbol in grammar.Symbols())
                usedSymbols.Add(symbol);
            for (int i = 0; i < variables.Count; ++i)
            {
                string vi = variables[i];
                for (int j = 0; j < i; ++j)
                {
                    string vj = variables[j];
                    OrderedHashSet<Production> toRemove = new OrderedHashSet<Production>();
                    OrderedHashSet<Production> toAdd = new OrderedHashSet<Production>();
                    foreach (Production production in productions)
                        if (production.lhs == vi && production.rhs.Length > 0 && production.rhs[0] == vj)
                        {
                            toRemove.Add(production);
                            foreach (Production otherProduction in productions)
                                if (otherProduction.lhs == vj)
                                {
                                    List<string> newRHS = new List<string>();
                                    newRHS.AddRange(otherProduction.rhs);
                                    newRHS.AddRange(production.rhs.Skip(1));
                                    toAdd.Add(new Production(vi, newRHS.ToArray()));
                                }
                            break;
                        }
                    foreach (Production addedProduction in toAdd)
                        productions.Add(addedProduction);
                    foreach (Production removedProduction in toRemove)
                        productions.Remove(removedProduction);
                }
                OrderedHashSet<Production> productionsWithoutViDirectLeftRecursion = new OrderedHashSet<Production>();
                OrderedHashSet<Production> newViProductions = new OrderedHashSet<Production>();
                foreach (Production production in productions)
                {
                    if (production.lhs != vi)
                        productionsWithoutViDirectLeftRecursion.Add(production);
                    else
                        newViProductions.Add(production);
                }
                foreach (Production newViProduction in RemoveDirectLeftRecursion(usedSymbols, newViProductions))
                    productionsWithoutViDirectLeftRecursion.Add(newViProduction);
                productions = productionsWithoutViDirectLeftRecursion;
            }
            if (epsilonIsDerived)
                productions.Add(new Production(grammar.startVariable));
            return verifyConsistency(grammar, new CFG(grammar.startVariable, productions));
        }
        /// <summary>
        /// Checks whether the grammar has a left-recursive production, i.e.,
        /// of the form `v ::= v s1 s2...` (can also be indirect).
        /// </summary>
        /// <param name="grammar">context-free grammar</param>
        /// <returns>whether grammar has left-recursion</returns>
        public static bool HasLeftRecursion(this CFG grammar)
        {
            foreach (string variable in grammar.Variables())
            {
                OrderedHashSet<string> reachableVariables = new OrderedHashSet<string>();
                reachableVariables.Add(variable);
                int n = 0;
                while (reachableVariables.Count() > n)
                {
                    n = reachableVariables.Count();
                    OrderedHashSet<string> newReachableVariables = new OrderedHashSet<string>();
                    foreach (string reachableVariable in reachableVariables)
                        foreach (Production production in grammar.productions)
                            if (production.lhs == reachableVariable && production.rhs.Length > 0)
                            {
                                string first = production.rhs[0];
                                if (first == variable)
                                    return true;
                                else if (grammar.Variables().Contains(first))
                                    newReachableVariables.Add(first);
                            }
                    foreach (string newVariable in newReachableVariables)
                        reachableVariables.Add(newVariable);
                }
            }
            return false;
        }
        private static OrderedHashSet<Production> RemoveDirectLeftRecursion(OrderedHashSet<string> usedSymbols, OrderedHashSet<Production> productions)
        {
            if (!HasDirectLeftRecursion(productions))
                return productions;
            OrderedHashSet<Production> newProductions = new OrderedHashSet<Production>();
            foreach (Production production in productions)
            {
                Debug.Assert(production.rhs.Length > 0);
                if (production.rhs.Length != 1 || production.lhs != production.rhs[0])
                    newProductions.Add(production);
            }
            if (newProductions.Count == 0)
                return newProductions;
            string lhs = newProductions[0].lhs;
            foreach (Production production in newProductions)
                Debug.Assert(production.lhs == lhs);
            OrderedHashSet<Production> leftRecursiveProductions = new OrderedHashSet<Production>(),
                nonLeftRecursiveProductions = new OrderedHashSet<Production>();
            foreach (Production production in newProductions)
                if (production.rhs[0] != lhs)
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
                List<string> newRHS = new List<string>();
                newRHS.AddRange(nonLeftRecursiveProduction.rhs);
                newProductions.Add(new Production(lhs, newRHS.ToArray()));
                newRHS.Add(auxiliaryVariable);
                newProductions.Add(new Production(lhs, newRHS.ToArray()));
            }
            foreach (Production leftRecursiveProduction in leftRecursiveProductions)
            {
                List<string> newRHS = new List<string>();
                newRHS.AddRange(leftRecursiveProduction.rhs.Skip(1));
                Debug.Assert(newRHS.Count > 0);
                newProductions.Add(new Production(auxiliaryVariable, newRHS.ToArray()));
                newRHS.Add(auxiliaryVariable);
                newProductions.Add(new Production(auxiliaryVariable, newRHS.ToArray()));
            }
            return newProductions;
        }
        private static bool HasDirectLeftRecursion(OrderedHashSet<Production> productions)
        {
            if (productions.Count == 0)
                return false;
            string lhs = productions[0].lhs;
            foreach (Production production in productions)
            {
                Debug.Assert(lhs == production.lhs);
                if (production.rhs.Count() > 0 && production.rhs[0] == lhs)
                    return true;
            }
            return false;
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
