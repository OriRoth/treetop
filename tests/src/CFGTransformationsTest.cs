using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using treetop;

namespace tests
{
    [TestClass]
    public class CFGTransformationsTest
    {
        readonly CFG grammar1 = CFGBuilder.Start("a")
                .Derive("a").To("b", "c")
                .Derive("a").To("d", "e", "f")
                .Derive("b").To("a")
                .Derive("c").ToEpsilon()
                .Build();
        readonly CFG grammar2 = CFGBuilder.Start("a")
                .Derive("a").ToEpsilon()
                .Derive("a").To("b", "c", "d")
                .Derive("b").ToEpsilon()
                .Derive("c").To("d")
                .Derive("c").ToEpsilon()
                .Build();
        readonly CFG grammar3 = CFGBuilder.Start("S")
                .Derive("S").To("S", "S")
                .Derive("S").To("(", "S", ")")
                .Derive("S").ToEpsilon()
                .Build();
        readonly CFG grammar4 = CFGBuilder.Start("S")
                .Derive("S").To("S", "A")
                .Derive("A").ToEpsilon()
                .Derive("B").ToEpsilon()
                .Build();
        [TestMethod]
        public void TestSimplify()
        {
            Assert.AreEqual(grammar1, grammar1.Simplify());
            Assert.AreEqual(grammar2, grammar2.Simplify());
            Assert.AreEqual(grammar3, grammar3.Simplify());
            CFG expectedResult = CFGBuilder.Start("S")
                .Derive("S").To("S", "A")
                .Derive("A").ToEpsilon()
                .Build();
            Assert.AreEqual(expectedResult, grammar4.Simplify());
        }
        [TestMethod]
        public void TestReversed()
        {
            CFG reversed = CFGBuilder.Start("a")
                    .Derive("a").To("c", "b")
                    .Derive("a").To("f", "e", "d")
                    .Derive("b").To("a")
                    .Derive("c").ToEpsilon()
                    .Build();
            Assert.AreEqual(reversed, grammar1.Reversed());
        }
        [TestMethod]
        public void TestRemoveInitialVariableInRHS()
        {
            CFG grammar1Transformed = grammar1.RemoveInitialVariableFromRHS();
            Assert.AreNotEqual(grammar1.startVariable, grammar1Transformed.startVariable);
            Assert.AreEqual(grammar1.productions.Count + 1, grammar1Transformed.productions.Count);
            Assert.IsTrue(grammar1Transformed.productions.Contains(new Production(grammar1Transformed.startVariable, grammar1.startVariable)));
            CFG grammar1TransformedAgain = grammar1Transformed.RemoveInitialVariableFromRHS();
            Assert.AreEqual(grammar1Transformed.startVariable, grammar1TransformedAgain.startVariable);
            Assert.AreEqual(grammar1Transformed.productions.Count, grammar1TransformedAgain.productions.Count);
        }
        [TestMethod]
        public void TestRemoveEpsilonProductions()
        {
            CFG grammar2Transformed = grammar2.RemoveEpsilonProductions();
            CFG expectedresult = CFGBuilder.Start("a")
                .Derive("a").ToEpsilon()
                .Derive("a").To("c", "d")
                .Derive("a").To("d")
                .Derive("c").To("d")
                .Build();
            Assert.AreEqual(expectedresult, grammar2Transformed);
        }
        [TestMethod]
        public void TestRemoveLeftRecursion()
        {
            Assert.IsFalse(grammar1.HasLeftRecursion());
            Assert.AreEqual(grammar1, grammar1.RemoveLeftRecursion());
            Assert.IsFalse(grammar2.HasLeftRecursion());
            Assert.AreEqual(grammar2, grammar2.RemoveLeftRecursion());
            Assert.IsTrue(grammar3.HasLeftRecursion());
            Assert.IsFalse(grammar3.RemoveLeftRecursion().HasLeftRecursion());
        }
        [TestMethod]
        public void TestGreibachNormalForm()
        {
            Assert.IsFalse(grammar1.InGreibachNormalForm());
            Assert.IsFalse(grammar2.InGreibachNormalForm());
            Assert.IsFalse(grammar3.InGreibachNormalForm());
            Assert.IsTrue(grammar1.ToGreibachNormalForm().InGreibachNormalForm());
            Assert.IsTrue(grammar2.ToGreibachNormalForm().InGreibachNormalForm());
            CFG grammar3Greibach = grammar3.ToGreibachNormalForm();
            Assert.IsTrue(grammar3Greibach.InGreibachNormalForm());
            Assert.IsTrue(derives(grammar3Greibach, ""));
            Assert.IsTrue(derives(grammar3Greibach, "()"));
            Assert.IsTrue(derives(grammar3Greibach, "()()"));
            Assert.IsTrue(derives(grammar3Greibach, "(())"));
            Assert.IsTrue(derives(grammar3Greibach, "(()())"));
            Assert.IsTrue(derives(grammar3Greibach, "()(()(()))(())"));
            Assert.IsFalse(derives(grammar3Greibach, "("));
            Assert.IsFalse(derives(grammar3Greibach, ")"));
            Assert.IsFalse(derives(grammar3Greibach, "())"));
            Assert.IsFalse(derives(grammar3Greibach, "(()()"));
            Assert.IsFalse(derives(grammar3Greibach, "((())))"));
            Assert.IsFalse(derives(grammar3Greibach, "(()(())"));
            Assert.IsFalse(derives(grammar3Greibach, "()(()((()))(())))"));
        }
        private static bool derives(CFG grammar, string word)
        {
            if (!grammar.InGreibachNormalForm())
                throw new Exception("only grammars in Greibach normal form are supported");
            List<string> wordList = new List<string>();
            wordList.AddRange(word.Select(c => c.ToString()));
            List<string> initialSententialForm = new List<string>();
            initialSententialForm.Add(grammar.startVariable);
            return derives(grammar, initialSententialForm, wordList);
        }
        private static bool derives(CFG grammar, List<string> sententialForm, List<string> restOfWord)
        {
            if (sententialForm.Count == 0)
                return restOfWord.Count == 0;
            if (restOfWord.Count == 0)
            {
                foreach (string symbol in sententialForm)
                    if (!grammar.productions.Contains(new Production(symbol)))
                        return false;
                return true;
            }
            string topSymbol = sententialForm[0];
            if (grammar.Terminals().Contains(topSymbol))
                return restOfWord[0] == topSymbol && derives(grammar, sententialForm.Skip(1).ToList(), restOfWord.Skip(1).ToList());
            foreach (Production production in grammar.productions)
                if (production.lhs == topSymbol)
                {
                    List<string> newSententialForm = new List<string>();
                    newSententialForm.AddRange(production.rhs);
                    newSententialForm.AddRange(sententialForm.Skip(1));
                    if (derives(grammar, newSententialForm, restOfWord))
                        return true;
                }
            return false;
        }
    }
}
