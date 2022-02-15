using System;
using System.IO;
using System.Linq;
using Tanka.GraphQL.Language;

namespace Tanka.GraphQL.Dev.Allocations;

public class Program
{
    private static void Main(string[] args)
    {
        var parser = Parser.Create(File.ReadAllBytes("RealWorldSchemas/github.graphql"));
        var typeSystem = parser.ParseTypeSystemDocument();

        if (typeSystem.TypeDefinitions == null || !typeSystem.TypeDefinitions.Any())
            throw new Exception("It has types");

        Console.WriteLine("Parsed");
    }
}