using CommandLine;
using System;
using System.IO;

namespace treetop
{
    /// <summary>
    /// Generator command line option declarations.
    /// </summary>
    class SomeOptions
    {
        [Option('n', "name", Required = true, HelpText = "API name")]
        public string Name { get; set; }
        [Option('f', "fluent", Required = false, HelpText = "Whether to generate a fluent API")]
        public bool Fluent { get; set; }
        [Option('i', "input", Required = true, HelpText = "Input grammar file")]
        public string Input { get; set; }
        [Option('o', "output", Required = false, HelpText = "Output C# API file")]
        public string Output { get; set; }
    }

    /// <summary>
    /// Treetop's command line interface application.
    /// Embeds language specified by context-free grammar as C# API.
    /// Resulting API is subtyping-based; complementary fluent API generation is optional.
    /// 
    /// Usage example:
    /// <code>treetop-cli -i Grammar.cfg -n MyGrammar -o Grammar.cs -f</code>
    /// Generates subtyping-based + fluent API for the language specified by the CFG in
    /// file Grammar.cfg, and writes the result to file Grammar.cs.
    /// </summary>
    public class CFProtocolGeneratorCLI
    {
        public static void Main(String[] args)
        {
            var options = new SomeOptions();
            Parser.Default.ParseArguments<SomeOptions>(args).WithParsed(parsed => options = parsed);
            if (options.Input == null || options.Name == null)
            {
                return;
            }
            CFG grammar = null;
            try
            {
                grammar = CFGParser.Parse(File.ReadAllText(options.Input));
            } catch (Exception e)
            {
                Console.WriteLine($"Could not parse file {options.Input}:\n{e.Message}");
                return;
            }
            try
            {
                if (options.Fluent)
                    grammar = grammar.Reversed();
            } catch (Exception e)
            {
                Console.WriteLine($"Error when reversing grammar:\n{e.Message}");
                return;
            }
            try
            {
                grammar = grammar.ToGreibachNormalForm();
            } catch (Exception e)
            {
                Console.WriteLine($"Error when converting grammar to Greibach normal form:\n{e.Message}");
                return;
            }
            string result = null;
            try
            {
                result = new CFGSubtypingAPIGenerator(grammar, options.Name, options.Fluent).PrintAPI();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error when generating API:\n{e.Message}");
                return;
            }
            try
            {
                if (options.Output == null)
                {
                    Console.WriteLine(result);
                }
                else
                {
                    File.WriteAllText(options.Output, result);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error when writing output:\n{e.Message}");
                return;
            }
        }
    }
}
