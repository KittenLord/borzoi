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
            [ "build", .. ] => BorzoiBuild(argsList, false),
            [ "run", .. ] => BorzoiBuild(argsList, true),
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

    private static (
        List<string> FilePaths,
        List<string> LibSearchPaths,
        ArgValue<string> OutputPath,
        ArgValue<string> OutputObjPath,
        ArgValue<string> OutputNasmPath,
        ArgValue<bool?> PlatformWindows,
        ArgValue<bool> DisplayParser)
    BuildArguments(ArgumentParser parser)
    {
        return (
            parser.TailArgument(0, new(), str => str[0]),
            parser.ListArgument(["--lib-search-path", "-ls"], new(), str => str[0]),
            parser.SingleArgument(["--output", "-o"], new(), str => str[0]),
            parser.SingleArgument(["--output-obj", "-oo"], new(), str => str[0]),
            parser.SingleArgument(["--output-nasm", "-onasm"], new(), str => str[0]),
            parser.SingleArgument<bool?>(["--platform", "-p"], new(), str => str[0] switch 
                { "win" or "win64" => true, "linux" or "lin" or "linux64" or "lin64" => false, _ => null }),
            parser.FlagArgument(["--display-parser", "-parser"], new())
        );
    }
    
    private static Result BorzoiBuild(Stack<string> args, bool run)
    {
        args.Pop();

        var argParser = new ArgumentParser();
        var build = BuildArguments(argParser);

        var filePaths = build.FilePaths;
        var libSearchPaths = build.LibSearchPaths;

        argParser.Parse(args);

        bool platformWindows = build.PlatformWindows.Get(IsWindows)!.Value;
        if(filePaths.Count <= 0) 
        {
            string[] backupFilePaths = [ "main.bz" ];
            var backupFilePath = backupFilePaths.FirstOrDefault(File.Exists);

            if(backupFilePath is null)
            {
                return new("Couldn't find any files");
            }

            filePaths.Add(backupFilePath);
        }

        List<string> defaultLibSearchPaths = [ "lib", "libs", "module", "modules" ];
        defaultLibSearchPaths.ForEach(path => {
            if(libSearchPaths.Contains(path)) return;
            if(!Directory.Exists(path)) return;
            libSearchPaths.Add(path);
        });

        string outputPath = build.OutputPath.Get(Path.ChangeExtension(filePaths.First(), platformWindows ? "exe" : ""));
        string outputObjPath = build.OutputObjPath.Get(Path.ChangeExtension(outputPath, "o"));
        string outputNasmPath = build.OutputNasmPath.Get(Path.ChangeExtension(outputPath, "S"));

        var files = new List<string>();
        foreach(var filePath in filePaths)
        {
            if(!File.Exists(filePath))
            { return new($"File {filePath} does not exist"); }

            var file = File.ReadAllText(filePath);
            files.Add(file);
        }

        var compilerResult = GenerateNasm(files, platformWindows, build.DisplayParser.Get(false));
        if(compilerResult.result.Error) return compilerResult.result;
        File.WriteAllText(outputNasmPath, compilerResult.data!.Nasm);

        var buildResult = BuildExecutable(
                outputNasmPath, outputObjPath, outputPath, 
                platformWindows, 
                compilerResult.data!.Links, libSearchPaths);
        if(buildResult.Error) return buildResult;

        if(run)
        {
            var process = new Process();
            process.StartInfo = new(outputPath) 
            { UseShellExecute = false };
            process.Start();
        }

        return new(0, "Build was successful!");
    }

    private class CompileData { public string Nasm = ""; public List<string> Links = new(); }
    private static (Result result, CompileData? data) GenerateNasm(List<string> files, bool platformWindows, bool displayParser)
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

        if(displayParser) Console.WriteLine(parser.AST);

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
