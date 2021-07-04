using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.IO;
using System.Text;

namespace treetop
{
    /// <summary>
    /// Generates subtyping-based APIs from given context-free grammars.
    /// For each file `G.cfg` contaning a context-free grammar `G`,
    /// a corresponding class `G` will be generated;
    /// The interfaces included in `G` implement a subtyping-based API for
    /// the language of `G`, `L(G)`.
    /// </summary>
    [Generator]
    public class CFProtocolGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            // Relax.
        }
        public void Execute(GeneratorExecutionContext context)
        {
            foreach (AdditionalText file in context.AdditionalFiles)
            {
                if (Path.GetExtension(file.Path).Equals(".cfg", StringComparison.OrdinalIgnoreCase))
                {
                    Execute(context, file);
                }
            }
        }

        private void Execute(GeneratorExecutionContext context, AdditionalText file)
        {
            CFG grammar = CFGParser.Parse(file.GetText().ToString());
            grammar = grammar.ApplyOptions(GetOptions(context, file)).ToGreibachNormalForm();
            CFGSubtypingAPIGenerator generator = new CFGSubtypingAPIGenerator(grammar, Path.GetFileNameWithoutExtension(file.Path),
                GetBoolConfiguration("FluentAPI", context, file));
            context.AddSource(generator.NamespaceName(), SourceText.From(generator.PrintAPI(), Encoding.UTF8));
        }

        private OrderedHashSet<GrammarOption> GetOptions(GeneratorExecutionContext context, AdditionalText file)
        {
            OrderedHashSet<GrammarOption> options = new OrderedHashSet<GrammarOption>();
            bool reversed = GetBoolConfiguration("Reversed", context, file);
            if (reversed)
            {
                options.Add(GrammarOption.Reversed);
            }
            return options;
        }
        private static bool GetBoolConfiguration(string key, GeneratorExecutionContext context, AdditionalText file)
        {
            context.AnalyzerConfigOptions.GetOptions(file)
                .TryGetValue("build_metadata.additionalfiles." + key, out var valueString);
            bool.TryParse(valueString, out bool value);
            return value;
        }
    }
}
