using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace treetop
{
    /// <summary>
    /// Prints subtyping-based API for given context-free grammar.
    /// </summary>
    public class CFGSubtypingAPIGenerator
    {
        private static readonly string TYPE_PARAMETER_NAME = "_x";
        private static readonly string BOTTOM_TYPE_NAME = "BOTTOM";
        private readonly CFG grammar;
        private readonly string grammarName;
        private readonly bool generateFluentAPI;
        public CFGSubtypingAPIGenerator(CFG grammar, string grammarName, bool generateFluentAPI)
        {
            if (!grammar.InGreibachNormalForm())
                throw new Exception("can generate API only for grammars in Greibach normal form");
            this.grammar = grammar;
            this.grammarName = grammarName;
            this.generateFluentAPI = generateFluentAPI;
        }
        /// <returns>API C# file as string</returns>
        public string PrintAPI()
        {
            return $"namespace {NamespaceName()}\n{{\n"
                + PrintTerminalInterfaces()
                + PrintVariableInterfaces()
                + PrintBottomType()
                + PrintStartingSententialFormTypeAlias()
                + (!generateFluentAPI ? "" : PrintFluentAPI())
                + "}\n".Replace("\n", Environment.NewLine);
        }
        /// <returns>top namespace name</returns>
        public string NamespaceName()
        {
            return grammarName + "API";
        }
        private string PrintTerminalInterfaces()
        {
            StringBuilder builder = new StringBuilder();
            foreach (string terminal in grammar.Terminals())
                builder.Append(PrintTerminalInterface(terminal));
            return builder.ToString();
        }
        private string PrintTerminalInterface(string terminal)
        {
            return $"\tpublic interface {terminal}<out {TYPE_PARAMETER_NAME}> {{}}\n";
        }
        private string PrintVariableInterfaces()
        {
            StringBuilder builder = new StringBuilder();
            foreach (string variable in grammar.Variables())
                builder.Append(PrintVariableInterface(variable));
            return builder.ToString();
        }
        private string PrintVariableInterface(string variable)
        {
            return $"\tpublic interface {variable}<{TYPE_PARAMETER_NAME}> : {PrintSupertypes(variable)} {{}}\n";
        }
        private string PrintSupertypes(string variable)
        {
            List<string> supertypes = new List<string>();
            foreach (Production production in grammar.productions)
                if (production.rhs.Length > 0 // ignore nullable start variable
                    && production.lhs == variable)
                    supertypes.Add(PrintType(production.rhs));
            return string.Join(", ", supertypes);
        }
        private string PrintType(string[] rhs)
        {
            return rhs.Length == 0 ? TYPE_PARAMETER_NAME : $"{rhs[0]}<{PrintType(rhs.Skip(1).ToArray())}>";
        }
        private string PrintBottomType()
        {
            return $"\tpublic interface {BOTTOM_TYPE_NAME} {{}}\n";
        }
        private string PrintStartingSententialFormTypeAlias()
        {
            return $"\tpublic interface {grammarName} : {grammar.startVariable}<{BOTTOM_TYPE_NAME}> {{}}\n";
        }
        private string PrintFluentAPI()
        {
            return "\tnamespace FluentAPI\n\t{\n"
                + PrintFluentAPIWrapper()
                + PrintFluentAPIEnum()
                + PrintFluentAPIMethods()
                + "\t}\n";
        }
        private string PrintFluentAPIWrapper()
        {
            return @"		public class Wrapper<T>
		{
			public readonly System.Collections.Generic.List<ENUM> values = new System.Collections.Generic.List<ENUM>();
			public Wrapper<T> AddRange<S>(Wrapper<S> other)
			{
				this.values.AddRange(other.values);
				return this;
			}
			public Wrapper<T> Add(ENUM value)
			{
				values.Add(value);
				return this;
			}
			public System.Collections.Generic.List<ENUM> Done<API>() where API : T => values;
		}
".Replace("ENUM", TokenEnumName());
        }
        private string PrintFluentAPIEnum()
        {
            StringBuilder enumValues = new StringBuilder();
            foreach (string terminal in grammar.Terminals())
                enumValues.Append(terminal).Append(", ");
            return $"\t\tpublic enum {TokenEnumName()} {{ {enumValues}}}\n";
        }
        private string PrintFluentAPIMethods()
        {
            List<string> methods = new List<string>();
            foreach (string terminal in grammar.Terminals())
            {
                methods.Add($"public static Wrapper<{terminal}<{BOTTOM_TYPE_NAME}>> {terminal}() => "
                    + $"new Wrapper<{terminal}<{BOTTOM_TYPE_NAME}>>().Add({TokenEnumName()}.{terminal});");
                methods.Add($"public static Wrapper<{terminal}<{TYPE_PARAMETER_NAME}>> {terminal}<{TYPE_PARAMETER_NAME}>"
                    + $"(this Wrapper<{TYPE_PARAMETER_NAME}> _wrapper) => "
                    + $"new Wrapper<{terminal}<{TYPE_PARAMETER_NAME}>>().AddRange(_wrapper).Add({TokenEnumName()}.{terminal});");
            }
            if (grammar.productions.Contains(new Production(grammar.startVariable)))
            {
                methods.Add($"public static System.Collections.Generic.List<{TokenEnumName()}> Done<{grammarName}>() => " +
                    $"new System.Collections.Generic.List<{TokenEnumName()}>();");
            }
            return $"\t\tpublic static class Start\n\t\t{{\n\t\t\t{String.Join("\n\t\t\t", methods)}\n\t\t}}\n";
        }
        private string TokenEnumName()
        {
            return grammarName + "Token";
        }
    }
}
