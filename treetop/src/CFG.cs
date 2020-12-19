namespace treetop
{
    /// <summary>
    /// Context-free grammar.
    /// Grammar productions are of the form `v ::= s`, i.e., where the left-hand side
    /// contains a single variable.
    /// </summary>
    public class CFG
    {
        /// <summary>
        /// Grammar start variable, `v0`.
        /// </summary>
        public readonly string startVariable;
        /// <summary>
        /// Set of grammar productions, `R`.
        /// </summary>
        public readonly OrderedHashSet<Production> productions;
        public CFG(string startVariable, OrderedHashSet<Production> productions)
        {
            this.startVariable = startVariable;
            this.productions = productions;
        }
        /// <summary>
        /// Get all grammar symbols, including terminals and variables.
        /// </summary>
        /// <returns>Grammar symbols</returns>
        public OrderedHashSet<string> Symbols()
        {
            OrderedHashSet<string> symbols = new OrderedHashSet<string>();
            foreach (Production production in productions)
            {
                symbols.Add(production.lhs);
                foreach (string rhsItem in production.rhs)
                {
                    symbols.Add(rhsItem);
                }
            }
            return symbols;
        }
        /// <returns>Grammar variables</returns>
        public OrderedHashSet<string> Variables()
        {
            OrderedHashSet<string> variables = new OrderedHashSet<string>();
            foreach (Production production in productions)
            {
                variables.Add(production.lhs);
            }
            return variables;
        }
        /// <returns>Grammar terminals</returns>
        public OrderedHashSet<string> Terminals()
        {
            OrderedHashSet<string> terminals = Symbols();
            foreach (string variable in Variables())
            {
                terminals.Remove(variable);
            }
            return terminals;
        }
        public override bool Equals(object obj)
        {
            return obj is CFG cfg &&
                   startVariable == cfg.startVariable &&
                   productions.SetEquals(cfg.productions);
        }
        public override int GetHashCode()
        {
            int hashCode = -151382474;
            hashCode = hashCode * -1521134295 + startVariable.GetHashCode();
            foreach (Production production in productions)
                hashCode = hashCode * -1521134295 + production.GetHashCode();
            return hashCode;
        }
        public override string ToString() => string.Join(",", productions);
    }
}
