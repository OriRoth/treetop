# Treetop: A Context-Free Fluent API Generator for C\#

Treetop is a *proof-of-concept* C# fluent API generator for context-free protocols.
Treetop accepts a context-free grammar, specifying an API protocol or a domain-specific
language, and [embeds](https://martinfowler.com/bliki/DomainSpecificLanguage.html)
it in C# as a fluent API.
The resulting API enforces the grammar at compile-time: A fluent API chain may
compile if and only if it encodes a word derived from the grammar.

For example, consider the following context-free grammar deriving
[palindromes](https://en.wikipedia.org/wiki/Palindrome):
```
S ::= a S a
S ::= b S b
S ::= a
S ::= b
S ::=
```
(Note that the line `S ::=` specifies an `S` epsilon-production.)
Treetop converts this grammar into a C# fluent API that accepts only
palindromes at compile time:
```csharp
// Compiles, "abbabba" is a palindrome
Start.a().b().b().a().b().b().a().Done<Palindrome>();
// Does not compile, "abbbaba" is not a palindrome
Start.a().b().b().b().a().b().a().Done<Palindrome>();
```
## Project Layout

* `treetop-core`: The core logic of Treetop. This project also contains
          the Treetop source generator, `CFProtocolGenerator.cs`:
          Visual Studio does not support source generator projects
          with dependencies, so the generator had to be moved to the
          core project (see the following
          [discussion](https://stackoverflow.com/questions/67071355/source-generators-dependencies-not-loaded-in-visual-studio)).
          A designated generator project is planned to be added in the future.
* `treetop-cli`: The Treetop command line application. Depends on `treetop-core`.
* `tests`: Tests for `treetop-core`.
* `sandbox`: A sandbox project for testing the source generator within
       Visual Studio.
