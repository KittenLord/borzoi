using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using EdComp.Lexing;
using EdComp.Parsing;
using EdComp.Analysis;
using EdComp.ASTn;

namespace EdComp;

public class Program
{
    public static void Main(string[] args)
    {
        var filePath = "Resources/test.txt";
        var text = File.ReadAllText(filePath);
        TestLexer(text);
        System.Console.WriteLine($"\n\n\n");

        var lexer = new Lexer(text);
        var parser = new Parser(lexer);

        parser.Parse();
        var ast = parser.AST;

        System.Console.WriteLine($"{string.Join("\n\n", parser.Errors)}");
        System.Console.WriteLine($"{parser.AST}");

        var analyzer = new Analyzer(ast);
        analyzer.Analyze();
        System.Console.WriteLine($"{string.Join("\n\n", analyzer.Errors)}");
        var fn = analyzer.AST.Fndefs.First();
        // System.Console.WriteLine($"{string.Join("\n", analyzer.Identifiers.Select(id => id.WorkingName))}");
        System.Console.WriteLine($"{string.Join("\n", fn.ArgsInternal)}");
        System.Console.WriteLine($"--------------------");
        System.Console.WriteLine($"{string.Join("\n", fn.VarsInternal)}");

    }

    private static void TestLexer(string text)
    {
        var lexer = new Lexer(text);
        Token token = lexer.Pop();

        do 
        {
            Console.WriteLine(token);
        }
        while((token = lexer.Pop()).Type != TokenType.EOF);
    }
}
