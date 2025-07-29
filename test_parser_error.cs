using System;
using Tanka.GraphQL.Language;

var source = "{ field";
var parser = new Parser(new Lexer(source));

try
{
    parser.ParseExecutableDocument();
    Console.WriteLine("No error thrown");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}