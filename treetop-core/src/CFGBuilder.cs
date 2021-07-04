namespace treetop
{
    /// <summary>
    /// Fluent CFG builder.
    /// </summary>
    public class CFGBuilder
    {
        /// <summary>
        /// Grammar start variable.
        /// </summary>
        private readonly string startVariable;
        /// <summary>
        /// Grammar productions.
        /// </summary>
        private readonly OrderedHashSet<Production> productions;
        private CFGBuilder(string startVariable, OrderedHashSet<Production> productions)
        {
            this.startVariable = startVariable;
            this.productions = productions;
        }
        /// <summary>
        /// Initiate fluent CFG builder.
        /// </summary>
        /// <param name="startVariable">Grammar start variable `v0`</param>
        /// <returns>fluent CFG builder</returns>
        public static CFGBuilder Start(string startVariable) => new CFGBuilder(startVariable, new OrderedHashSet<Production>());
        /// <summary>
        /// Fluently derive grammar variable.
        /// </summary>
        /// <param name="lhs">Grammar variable to be derived</param>
        /// <returns>fluent CFG builder</returns>
        public CFGBuilderDerive Derive(string lhs) => new CFGBuilderDerive(this, lhs);
        /// <summary>
        /// Build CFG from specified start variable and productions.
        /// </summary>
        /// <returns>CFG</returns>
        public CFG Build() => new CFG(startVariable, productions);
        /// <summary>
        /// Fluent CFG builder part.
        /// </summary>
        public class CFGBuilderDerive
        {
            /// <summary>
            /// Parent fluent builder.
            /// </summary>
            private readonly CFGBuilder parent;
            /// <summary>
            /// Grammar variable to be derived.
            /// </summary>
            private readonly string lhs;
            public CFGBuilderDerive(CFGBuilder parent, string lhs)
            {
                this.parent = parent;
                this.lhs = lhs;
            }
            /// <summary>
            /// Derive `lhs` to specified sentential form.
            /// </summary>
            /// <param name="rhs">derived sentential form</param>
            /// <returns>fluent CFG buidler</returns>
            public CFGBuilder To(params string[] rhs)
            {
                parent.productions.Add(new Production(lhs, rhs));
                return parent;
            }
            /// <summary>
            /// Derive `lhs` to epsilon.
            /// </summary>
            /// <returns>fluent CFG buidler</returns>
            public CFGBuilder ToEpsilon()
            {
                parent.productions.Add(new Production(lhs, new string[0]));
                return parent;
            }
        }
    }
}
