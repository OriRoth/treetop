using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using treetop;

namespace tests
{
    [TestClass]
    public class CFGTest
    {
        readonly CFG grammar = CFGBuilder.Start("a")
                .Derive("a").To("b", "c")
                .Derive("a").To("d", "e", "f")
                .Derive("b").To("a")
                .Derive("c").ToEpsilon()
                .Build();
        [TestMethod]
        public void SanityChecks()
        {
            Assert.AreEqual("a", grammar.startVariable);
            Assert.AreEqual(4, grammar.productions.Count);
            ISet<Production> productions = new HashSet<Production>
            {
                new Production("a", "b", "c"),
                new Production("a", "d", "e", "f"),
                new Production("b", "a"),
                new Production("c")
            };
            Assert.IsTrue(productions.SetEquals(grammar.productions));
        }
        [TestMethod]
        public void TestGrammarSymbols()
        {
            ISet<string> symbols = new HashSet<string>
            {
                "a", "b", "c", "d", "e", "f"
            };
            ISet<string> variables = new HashSet<string>
            {
                "a", "b", "c"
            };
            ISet<string> terminals = new HashSet<string>
            {
                "d", "e", "f"
            };
            Assert.IsTrue(symbols.SetEquals(grammar.Symbols()));
            Assert.IsTrue(variables.SetEquals(grammar.Variables()));
            Assert.IsTrue(terminals.SetEquals(grammar.Terminals()));
        }
    }
}
