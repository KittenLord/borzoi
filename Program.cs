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

namespace Borzoi;

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
        return args switch 
        {
            [ "build", .. ] => BorzoiBuild(argsList),
            [ .. ] => BorzoiInfo(argsList),
        };
    }

    private static int BorzoiInfo(Stack<string> args)
    {
        return 1;
    }
    
    private static int BorzoiBuild(Stack<string> args)
    {
        args.Pop();

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
            if(arg == "--lib-search-path") 
            {
                if(!args.TryPop(out var path))
                {
                    // TODO: Error
                    return 1;
                }

                libSearchPaths.Add(path);
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
            {
                Console.WriteLine($"File {filePath} does not exist");
                return 1;
            }

            files.Add(File.ReadAllText(filePath));
        }

        var parser = new Parser(null);

        foreach(var file in files)
        {
            var lexer = new Lexer(file);
            parser.Parse(lexer);

            foreach(var err in lexer.Errors) Console.WriteLine($"\n\n{err}");
            foreach(var err in parser.Errors) Console.WriteLine($"\n\n{err}");

            if(!lexer.Success || !parser.Success) return 1;
        }

        var analyzer = new Analyzer(parser.AST);
        analyzer.Analyze();

        foreach(var err in analyzer.Errors) Console.WriteLine($"\n\n{err}");
        if(!analyzer.Success) return 1;

        var generator = new Generator(analyzer.AST, platformWindows, true);
        var nasm = generator.Generate();

        File.WriteAllText(outputNasmPath, nasm);

        var nasmProcess = new Process();
        nasmProcess.StartInfo = new(
            "nasm.exe", 
            @$"-o {outputObjPath} -f {(platformWindows ? "win64" : "elf64")} {outputNasmPath}")
        {
            UseShellExecute = false
        };
        nasmProcess.Start();
        nasmProcess.WaitForExit();

        if(nasmProcess.ExitCode != 0)
        {
            Console.WriteLine("A fucky wucky has occurred");
            return nasmProcess.ExitCode;
        }

        // TODO: Alias links?
        var links = parser.AST.Links;
        if(platformWindows && !links.Contains("kernel32"))
            links.Add("kernel32");
        var linksArg = string.Join(" ", links.Select(l => $"-l{l}"));

        var libSearchPathsArg = string.Join(" ", libSearchPaths.Select(l => $"-L{l}"));

        var gccProcess = new Process();
        gccProcess.StartInfo = new("gcc.exe")
        {
            UseShellExecute = false
        };


        // NOTE: Learn more about linking/etc, I know too little
        
        void AddArgument(string s) =>
            gccProcess.StartInfo.Arguments += " " + s + " ";

        AddArgument(outputObjPath);
        AddArgument(libSearchPathsArg);
        AddArgument(linksArg);
        AddArgument($"-o {outputPath}");

        // string entry = platformWindows ? "main" : "_start";
        // gccProcess.StartInfo.Arguments += " " + "--entry=_start";
        // gccProcess.StartInfo.Arguments += " " + "-fPIC";

        // System.Console.WriteLine($"{gccProcess.StartInfo.Arguments}");

        gccProcess.Start();
        gccProcess.WaitForExit();

        if(gccProcess.ExitCode != 0)
        {
            Console.WriteLine("A fucky wucky has occurred");
            return gccProcess.ExitCode;
        }

        return 0;
    }
}
