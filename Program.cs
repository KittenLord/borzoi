using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Borzoi.Lexing;
using Borzoi.Parsing;
using Borzoi.Analysis;
using Borzoi.ASTn;
using Borzoi.Generation;

namespace Borzoi.CLI;

public class ArgInfo 
{
    public string Name;
    public string Description;

    public ArgInfo(string name, string description)
    {
        Name = name;
        Description = description;
    }
}

public class Result
{
    public int Code;
    public string Message;

    public bool Success => Code == 0;
    public bool Error => !Success;

    public Result() { Code = 0; Message = ""; }
    public Result(string message) { Code = 1; Message = message; }
    public Result(int code, string message) { Code = code; Message = message; }
} 

public class CompileData
{
    public string Nasm = "";
    public List<string> Links = new();
}

public class RefValue<T>
{
    public T Value = default;
    public RefValue(T value = default) { Value = value; }

    public override string? ToString() { return Value?.ToString(); }
}


public class ArgumentParser
{
    public class Argument
    {
        public List<string> Aliases = new();
        public int Position = 0;

        public object Reference;
        public Func<string, object?> Parser = default;

        public Func<Stack<string>, Argument, bool> Handler = default;
    }

    private List<Argument> ArgumentDefinitions = new();

    private int Position;
    private List<Argument> PositionalArgumentDefinitions = new();

    private bool HandleSingleArgument<T>(Stack<string> args, Argument argument)
    {
        var reference = (RefValue<T>)argument.Reference;

        if(!args.TryPop(out var arg)) return false;
        var value = (T?)argument.Parser(arg);
        if(value is null) return false;
        reference.Value = value;
        return true;
    }

    private bool HandleListArgument<T>(Stack<string> args, Argument argument)
    {
        var reference = (List<T>)argument.Reference;

        if(!args.TryPop(out var arg)) return false;
        var value = (T?)argument.Parser(arg);
        if(value is null) return false;
        reference.Add(value);
        return true;
    }


    public RefValue<T> CreateSingleArgument<T>(List<string> aliases, Func<string, T?> parser)
    {
        var reference = new RefValue<T>();
        SingleArgument(aliases, reference, parser);
        return reference;
    }

    public void SingleArgument<T>(List<string> aliases, RefValue<T> reference, Func<string, T?> parser)
    {
        if(ArgumentDefinitions.Any(arg => arg.Aliases.Any(alias => aliases.Contains(alias))))
        { throw new System.Exception("Bad"); }

        ArgumentDefinitions.Add(new Argument{ 
            Aliases = aliases,
            Reference = reference,
            Handler = HandleSingleArgument<T>,
            Parser = (str) => parser(str),
        });
    }



    public List<T> CreateTailArgument<T>(int position, Func<string, T?> parser)
    {
        var reference = new List<T>();
        TailArgument(position, reference, parser);
        return reference;
    }
    public void TailArgument<T>(int position, List<T> reference, Func<string, T?> parser)
    {
        if(PositionalArgumentDefinitions.Any(arg => arg.Position >= position))
        { throw new System.Exception("Bad"); }

        PositionalArgumentDefinitions.Add(new Argument{ 
            Position = position,
            Reference = reference,
            Handler = HandleListArgument<T>,
            Parser = (str) => parser(str),
        });
    }

    public RefValue<T> CreatePositionalArgument<T>(int position, Func<string, T?> parser)
    {
        var reference = new RefValue<T>();
        PositionalArgument(position, reference, parser);
        return reference;
    }
    public void PositionalArgument<T>(int position, RefValue<T> reference, Func<string, T?> parser)
    {
        if(PositionalArgumentDefinitions.Any(arg => arg.Position == position))
        { throw new System.Exception("Bad"); }

        PositionalArgumentDefinitions.Add(new Argument{ 
            Position = position,
            Reference = reference,
            Handler = HandleSingleArgument<T>,
            Parser = (str) => parser(str),
        });
    }


    public List<T> CreateListArgument<T>(List<string> aliases, Func<string, T?> parser)
    {
        var reference = new List<T>();
        ListArgument(aliases, reference, parser);
        return reference;
    }

    public void ListArgument<T>(List<string> aliases, List<T> reference, Func<string, T?> parser)
    {
        if(ArgumentDefinitions.Any(arg => arg.Aliases.Any(alias => aliases.Contains(alias))))
        { throw new System.Exception("Bad"); }

        ArgumentDefinitions.Add(new Argument{ 
            Aliases = aliases,
            Reference = reference,
            Handler = HandleListArgument<T>,
            Parser = (str) => parser(str),
        });
    }

    public bool Parse(IEnumerable<string> argsArr)
    {
        var args = new Stack<string>(argsArr.Reverse());
        Position = 0;

        while(args.TryPop(out var arg))
        {
            var isPositional = !arg.StartsWith("-");

            if(isPositional)
            {
                continue;
            }

            var argdef = ArgumentDefinitions.Find(a => a.Aliases.Contains(arg));
            if(argdef is null)
            {
                return false;
            }

            var result = argdef!.Handler(args, argdef);
        }

        return true;
    }
}

public class Program
{
    // bzoi
    // bzoi build
    // bzoi run
    // bzoi check
    // bzoi view (lexing/parsing/analyzing/generation)
    
    private static bool IsWindows;
    private static bool IsLinux;

    public static int Main(string[] args)
    {
        IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        IsLinux = !IsWindows; // MacOS and BSD in shambles

        var argsList = new Stack<string>(args.Reverse());
        var result = args switch 
        {
            [ "build", .. ] => BorzoiBuild(argsList),
            [ .. ] => BorzoiInfo(argsList),
        };

        if(result.Message != "")
        {
            Console.WriteLine(result.Message);
        }

        return result.Code;
    }

    private static Result BorzoiInfo(Stack<string> args)
    {
        var consoleWidth = Console.WindowWidth;
        var images = Images.Collection
            .Where(img => img.Width <= consoleWidth)
            .ToList();
        var image = new Image{};
        if(images.Count > 0)
            image = images[Random.Shared.Next(0, images.Count)];

        Console.WriteLine(image.Value);

        return new();
    }
    
    private static Result BorzoiBuild(Stack<string> args)
    {
        args.Pop();

        var argParser = new ArgumentParser();

        ArgInfo files_arg = new ( "files", "File(-s) to be compiled" );
        List<string> filePaths = new();
        List<string> libSearchPaths = new();

        string outputPath = "";
        string outputObjPath = "";
        string outputNasmPath = "";
        bool platformWindows = IsWindows;

        while(args.TryPeek(out _)) 
        {
            var arg = args.Pop();
            if(arg.StartsWith("-"))
            {
                if(arg == "--lib-search-path") 
                {
                    if(!args.TryPop(out var path))
                    {
                        // TODO: Error
                        return new("");
                    }

                    libSearchPaths.Add(path);
                }
            }
            else
            {
                filePaths.Add(arg);
            }
        }

        if(filePaths.Count <= 0) filePaths.Add("main.bz");
        var files = new List<string>();

        if(outputPath == "")
        {
            var reference = filePaths.First();
            outputPath = reference;
            if(reference.Contains("."))
            {
                var refSplit = reference.Split(".").ToList();
                refSplit = refSplit.Take(refSplit.Count - 1).ToList();
                outputPath = string.Join(".", refSplit);
            }

            if(platformWindows) outputPath += ".exe";
        }

        if(outputObjPath == "")
        {
            var split = outputPath.Split(".");
            split = split.Take(split.Length - 1).ToArray();
            outputObjPath = string.Join(".", split) + ".o";
        }

        if(outputNasmPath == "")
        {
            var split = outputPath.Split(".");
            split = split.Take(split.Length - 1).ToArray();
            outputNasmPath = string.Join(".", split) + ".S";
        }

        foreach(var filePath in filePaths)
        {
            if(!File.Exists(filePath))
            { return new($"File {filePath} does not exist"); }

            files.Add(File.ReadAllText(filePath));
        }

        var compilerResult = GenerateNasm(files, platformWindows);
        if(compilerResult.result.Error) return compilerResult.result;

        File.WriteAllText(outputNasmPath, compilerResult.data!.Nasm);

        var buildResult = BuildExecutable(
                outputNasmPath, outputObjPath, outputPath, 
                platformWindows, 
                compilerResult.data!.Links, libSearchPaths);
        if(buildResult.Error) return buildResult;

        return new();
    }

    private static (Result result, CompileData? data) GenerateNasm(List<string> files, bool platformWindows)
    {
        var parser = new Parser(null);

        foreach(var file in files)
        {
            var lexer = new Lexer(file);
            parser.Parse(lexer);

            foreach(var err in lexer.Errors) Console.WriteLine($"\n\n{err}");
            foreach(var err in parser.Errors) Console.WriteLine($"\n\n{err}");

            if(!lexer.Success || !parser.Success) return (new("A big fucky wucky happened while parsing"), null);
        }

        var analyzer = new Analyzer(parser.AST);
        analyzer.Analyze();

        foreach(var err in analyzer.Errors) Console.WriteLine($"\n\n{err}");
        if(!analyzer.Success) return (new("A big fucky wucky happened while analyzing"), null);

        var generator = new Generator(analyzer.AST, platformWindows, true);
        var nasm = generator.Generate();

        return (new(), new CompileData{ Links = parser.AST.Links, Nasm = nasm });
    }

    private static Result BuildExecutable(string outputNasmPath, string outputObjPath, string outputPath, bool platformWindows, List<string> links, List<string> libSearchPaths)
    {
        var nasmProcess = new Process();
        nasmProcess.StartInfo = new("nasm" + (IsWindows ? ".exe" : "")) 
        { UseShellExecute = false };

        void AddNasmArgument(string s) =>
            nasmProcess.StartInfo.Arguments += " " + s + " ";

        AddNasmArgument($"-o {outputObjPath}");
        AddNasmArgument($"-f {(platformWindows ? "win64" : "elf64")}");
        AddNasmArgument(outputNasmPath);

        nasmProcess.Start();
        nasmProcess.WaitForExit();

        if(nasmProcess.ExitCode != 0)
        { return new(nasmProcess.ExitCode, "NASM has encountered a fucky wucky"); }

        // TODO: Alias links?
        if(platformWindows && !links.Contains("kernel32"))
            links.Add("kernel32");
        var linksArg = string.Join(" ", links.Select(l => $"-l{l}"));

        List<string> defaultLibSearchPaths = [ "lib", "libs", "module", "modules" ];
        defaultLibSearchPaths.ForEach(path => {
            if(libSearchPaths.Contains(path)) return;
            if(!Directory.Exists(path)) return;
            libSearchPaths.Add(path);
        });
        var libSearchPathsArg = string.Join(" ", libSearchPaths.Select(l => $"-L{l}"));

        var gccProcess = new Process();
        gccProcess.StartInfo = new("gcc" + (IsWindows ? ".exe" : ""))
        { UseShellExecute = false };

        // NOTE: Learn more about linking/etc, I know too little
        
        void AddGccArgument(string s) =>
            gccProcess.StartInfo.Arguments += " " + s + " ";

        AddGccArgument(outputObjPath);
        AddGccArgument(libSearchPathsArg);
        AddGccArgument(linksArg);
        AddGccArgument($"-o {outputPath}");

        // string entry = platformWindows ? "main" : "_start";
        // gccProcess.StartInfo.Arguments += " " + "--entry=_start";
        // gccProcess.StartInfo.Arguments += " " + "-fPIC";

        // System.Console.WriteLine($"{gccProcess.StartInfo.Arguments}");

        gccProcess.Start();
        gccProcess.WaitForExit();

        if(gccProcess.ExitCode != 0)
        {
            return new(nasmProcess.ExitCode, "GCC has encountered a fucky wucky");
        }

        return new();
    }
}
