﻿using CommandLine;
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
            String result = new CFGSubtypingAPIGenerator(CFGParser.Parse(File.ReadAllText(options.Input)),
                options.Name, options.Fluent).PrintAPI();
            if (options.Output == null)
            {
                Console.WriteLine(result);
            }
            else
            {
                File.WriteAllText(options.Output, result);
            }
        }
    }
}
