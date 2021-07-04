using System;
using System.IO;

namespace treetop
{
    /// <summary>
    /// Parses context-free grammars from textual description.
    /// Description is a sequence of lines of the form `v ::= s`,
    /// where `v` is the derived variable and `s` is the derived sentential form.
    /// </summary>
    public class CFGParser
    {
        /// <param name="grammar">textual context-free grammar description</param>
        /// <returns>described grammar</returns>
        public static CFG Parse(string grammar)
        {
            grammar = grammar.Trim();
            var reader = new StringReader(grammar);
            string startVariable;
            OrderedHashSet<Production> productions = ParseProductions(reader, out startVariable);
            return new CFG(startVariable, productions);
        }
        private static OrderedHashSet<Production> ParseProductions(StringReader reader, out string firstVariable)
        {
            firstVariable = null;
            OrderedHashSet<Production> productions = new OrderedHashSet<Production>();
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (line.Trim() == "")
                {
                    continue;
                }
                var lineParts = line.Split(new string[] { "::=" }, 2, StringSplitOptions.None);
                productions.Add(new Production(lineParts[0].Trim(), lineParts[1].Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)));
                if (firstVariable == null)
                {
                    firstVariable = lineParts[0].Trim();
                }
            }
            return productions;
        }
    }
}
