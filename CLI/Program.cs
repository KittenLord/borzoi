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
            [ "version", .. ] or 
                [ "--version", .. ] or 
                [ "-v", ..] => BorzoiVersion(argsList),
            [ "help", .. ] or
                [ "--help", .. ] or
                [ "-h", .. ] => BorzoiHelp(argsList),
            [] => BorzoiImage(argsList),
            [ .. ] => BorzoiUsage(argsList, false),
        };

        if(result.Message != "")
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(result.Message);
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.White;
        }

        return result.Code;
    }

    private static Result BorzoiVersion(Stack<string> args)
    {
        Console.WriteLine($"Current version: {Settings.Version}");
        Console.WriteLine($"You can ompare it to the current one at {Settings.Link}");
        return new();
    }

    private static Result BorzoiHelp(Stack<string> args)
    {
        args.Pop();
        if(args.Count <= 0) return BorzoiUsage(args, true);
        var subcommand = args.Pop();

        // This is kinda bad, but I don't want to redesign this
        var parser = new ArgumentParser();
        if(subcommand == "build" || subcommand == "run")
        {
            BuildArguments(parser);
            if(subcommand == "run")
            { parser.FlagArgument(["--exit-code"], new(), description: "Display the exit code after program's execution"); }

            foreach(var argument in parser.PositionalArguments)
            {
                var list = argument.Tail ? "..." : "";
                Console.WriteLine($"<{list}{argument.Aliases.First()}> - {argument.Description}");
                if(argument.AllowedValues != "") Console.WriteLine($"\tAllowed values: {argument.AllowedValues}");
                if(argument.Aliases.Count > 1) Console.WriteLine($"\tAliases: {string.Join(", ", argument.Aliases)}");
                Console.WriteLine();
            }

            foreach(var argument in parser.SpecifiedArguments)
            {
                var list = argument.Tail ? "..." : "";
                Console.WriteLine($"[{list}{argument.Aliases.First()}] - {argument.Description}");
                if(argument.AllowedValues != "") Console.WriteLine($"\tAllowed values: {argument.AllowedValues}");
                if(argument.Aliases.Count > 1) Console.WriteLine($"\tAliases: {string.Join(", ", argument.Aliases)}");
                Console.WriteLine();
            }

            return new();
        }

        return BorzoiUsage(args, false);
    }

    private static Result BorzoiUsage(Stack<string> args, bool intentional)
    {
        Result result = intentional ? new() : new(1, "");
        if(!intentional)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Unknown usage!");
            Console.ForegroundColor = ConsoleColor.White;
        }
        Console.WriteLine("Usage:\n");
        Console.WriteLine("Build program - borzoi build <..files> [..options]");
        Console.WriteLine("Build and run program - borzoi run <..files> [..options]");
        Console.WriteLine("View the version - borzoi version");
        Console.WriteLine("Do the funny - borzoi");
        return result;
    }

    private static Result BorzoiImage(Stack<string> args)
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
        ArgValue<bool> Rawdog,
        ArgValue<bool> DisplayParser,
        ArgValue<bool> DisplayLexer,
        ArgValue<bool> Stats)
    BuildArguments(ArgumentParser parser)
    {
        return (
            parser.TailArgument(0, "files", new(), str => str[0], description: "File(-s) to be compiled"),
            parser.ListArgument(["--lib-search-path", "-ls"], new(), str => str[0], description: "Folders to search for libraries"),
            parser.SingleArgument(["--output", "-o"], new(), str => str[0], description: "Executable output path"),
            parser.SingleArgument(["--output-obj", "-oo"], new(), str => str[0], description: "Object file output path"),
            parser.SingleArgument(["--output-nasm", "-onasm"], new(), str => str[0], description: "Nasm file output path"),
            parser.SingleArgument<bool?>(["--platform", "-p"], new(), str => str[0] switch 
                { "win" or "win64" => true, "linux" or "lin" or "linux64" or "lin64" => false, _ => null }, description: "Target platform", allowedValues: "win, linux"),
            parser.FlagArgument(["--rawdog"], new(), description: "Disable the borzoi compiler, only rebuild the nasm file, in case you need to rawdog assembly"),
            parser.FlagArgument(["--display-parser", "-parser"], new(), description: "Display the output of the parsing step"),
            parser.FlagArgument(["--display-lexer", "-lexer"], new(), description: "Display the output of the lexing step"),
            parser.FlagArgument(["--stats", "-s"], new(), description: "Display compilation stats")
        );
    }
    
    private static Result BorzoiBuild(Stack<string> args, bool run)
    {
        DateTime reference = DateTime.Now;

        args.Pop();

        var argParser = new ArgumentParser();
        var build = BuildArguments(argParser);

        ArgValue<bool>? exitCode = null;
        if(run)
        {
            exitCode = argParser.FlagArgument(["--exit-code"], new());
        }

        var filePaths = build.FilePaths;
        var libSearchPaths = build.LibSearchPaths;
        var displayStats = build.Stats;

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
        if(outputPath.EndsWith(".")) outputPath = outputPath.Substring(0, outputPath.Length - 1);


        var files = new List<string>();
        foreach(var filePath in filePaths)
        {
            if(!File.Exists(filePath))
            { return new($"File {filePath} does not exist"); }

            var file = File.ReadAllText(filePath);
            files.Add(file);
        }

        var compilerResult = GenerateNasm(files, platformWindows, build.DisplayParser.Get(false), build.DisplayLexer.Get(false));


if(build.Rawdog.Get(false)) goto rawdog;
        if(compilerResult.result.Error) return compilerResult.result;
        File.WriteAllText(outputNasmPath, compilerResult.data!.Nasm);
rawdog:

        DateTime linkReference = DateTime.Now;
        var buildResult = BuildExecutable(
                outputNasmPath, outputObjPath, outputPath, 
                platformWindows, 
                compilerResult.data!.Links, libSearchPaths);
        if(buildResult.Error) return buildResult;
        compilerResult.data.LinkingTime = DateTime.Now - linkReference;

        // FIX: gcc non-zero exit code doesn't get detected???
        Console.WriteLine("Build was successful!");

        compilerResult.data.TotalTime = DateTime.Now - reference;
        if(displayStats.Get(false))
        {
            Console.WriteLine($"Parsing time: {compilerResult.data.ParsingTime}");
            Console.WriteLine($"Analyzing time: {compilerResult.data.AnalyzingTime}");
            Console.WriteLine($"Generating time: {compilerResult.data.GeneratingTime}");
            Console.WriteLine($"Linking time: {compilerResult.data.LinkingTime}");
            Console.WriteLine($"Total time: {compilerResult.data.TotalTime}");
        }

        if(run)
        {
            var process = new Process();
            process.StartInfo = new(outputPath) 
            { UseShellExecute = false };
            process.Start();
            process.WaitForExit();

            if(exitCode!.Get(false))
            {
                Console.WriteLine($"\nProgram finished with exit code {process.ExitCode}");
            }
        }

        return new();
    }

    private class CompileData { public string Nasm = ""; public List<string> Links = new();
        public TimeSpan ParsingTime; public TimeSpan AnalyzingTime; public TimeSpan GeneratingTime; public TimeSpan LinkingTime; public TimeSpan TotalTime; }
    private static (Result result, CompileData? data) GenerateNasm(List<string> files, bool platformWindows, bool displayParser, bool displayLexer)
    {
        DateTime reference = DateTime.Now;
        var parser = new Parser(null);

        foreach(var file in files)
        {
            if(displayLexer)
            {
                var l = new Lexer(file);
                do Console.WriteLine(l.Pop()); while(!l.Peek().Is(TokenType.EOF));
            }

            var lexer = new Lexer(file);
            parser.Parse(lexer);

            foreach(var err in lexer.Errors) Console.WriteLine($"\n\n{err}");
            foreach(var err in parser.Errors) Console.WriteLine($"\n\n{err}");

            if(!lexer.Success || !parser.Success) return (new("A big fucky wucky happened while parsing"), null);
        }
        var parsingTime = DateTime.Now - reference;
        reference = DateTime.Now;

        if(displayParser) Console.WriteLine(parser.AST);

        var analyzer = new Analyzer(parser.AST);
        analyzer.Analyze();

        var analyzingTime = DateTime.Now - reference;
        reference = DateTime.Now;

        foreach(var err in analyzer.Errors) Console.WriteLine($"\n\n{err}");
        if(!analyzer.Success) return (new("A big fucky wucky happened while analyzing"), null);

        var generator = new Generator(analyzer.AST, platformWindows, true);
        var nasm = generator.Generate();

        var generatingTime = DateTime.Now - reference;

        return (new(), new CompileData{ Links = parser.AST.Links, Nasm = nasm, ParsingTime = parsingTime, AnalyzingTime = analyzingTime, GeneratingTime = generatingTime });
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

        AddGccArgument("-fPIE");

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
