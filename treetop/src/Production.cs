using System;
using System.Linq;

namespace treetop
{
    /// <summary>
    /// Context-free production, of the form `v ::= s`
    /// where `v` is a variable and `s` is the derived sentential form.
    /// </summary>
    public class Production
    {
        /// <summary>
        /// Production left-hand side, i.e., the derived variable.
        /// </summary>
        public readonly string lhs;
        /// <summary>
        /// Production right-hand side, i.e., the derived sentential form.
        /// </summary>
        public readonly string[] rhs;
        public Production(string lhs, params string[] rhs)
        {
            this.lhs = lhs;
            this.rhs = rhs;
        }
        /// <summary>
        /// Returns true if and only if this is an epsilon production, i.e.,
        /// the right-hand side is empty.
        /// </summary>
        /// <returns>whether epsilon is produced</returns>
        public bool IsEpsilonProduction() => rhs.Length == 0;
        public override bool Equals(object obj)
        {
            return obj is Production production &&
                   lhs == production.lhs &&
                   rhs.SequenceEqual(production.rhs);
        }
        public override int GetHashCode()
        {
            int hashCode = -491437398;
            hashCode = hashCode * -1521134295 + lhs.GetHashCode();
            foreach (string item in rhs)
                hashCode = hashCode * -1521134295 + item.GetHashCode();
            return hashCode;
        }
        public override string ToString() => $"{lhs} ::= {string.Join(" ", rhs)}";
    }
}
