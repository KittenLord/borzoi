using System.Collections.Generic;
using System.Linq;
using System;

using Borzoi.ASTn;

namespace Borzoi.Generation;

public class Generator
{

    private AST AST;
    private bool Windows;
    private bool Optimize;

    private Dictionary<string, int> StringLiterals;
    private void AddStringLiteral(string s)
    {
        StringLiterals[s] = LastString;
        var bytes = System.Text.Encoding.UTF8.GetBytes(s)!;

        var label = $"STR_{LastString}@@";
        StringData += 
        $"{label}: db {string.Join("", bytes.Select(b => b.ToString() + ","))}0,0,0,0\n";
        StringData += $"{label}LEN: dq {bytes.Length}\n";
        LastString++;
    }

    private string GetStringLabel(string str)
    {
        if(StringLiterals.ContainsKey(str)) 
            return $"STR_{StringLiterals[str]}@@";
        AddStringLiteral(str);
        return $"STR_{StringLiterals[str]}@@";
    }

    private int LastString;
    private string StringData;

    public Generator(AST ast, bool windows, bool optimize)
    {
        AST = ast;
        Windows = windows;
        Optimize = optimize;
        LastString = 0;
        StringData = "";
        StringLiterals = new();
    }

    public string Generate()
    {
        List<string> externFunctions = 
            [ "malloc", "realloc", "calloc", "free", "printf" ];
        if(Windows) externFunctions.AddRange(
            [ "ExitProcess", "signal" ]);

        foreach(var cfn in AST.CFndefs)
        {
            if(!externFunctions.Contains(cfn.CName))
                { externFunctions.Add(cfn.CName); }
        }

        // I fucking hate windows
        string entry = Windows ? "main" : "_start";

        string result = 
        "BITS 64\n" +
        string.Join("", externFunctions.Select(fn => $"extern {fn}\n")) +

        "section .data\n" +
        "{0}" +
        "error@@OutOfBounds: db \"You a big stupid, the array has only %d elements and you're trying to access index %d? Are you crazy?!\",0xA,0\n" +
        "error@@SEGFAULT: db \"Oopsie daisy, a segfowolt has occurred\",0xA,0\n" +
        "gclen@@: dq -1\n" +
        $"gccap@@: dq {Settings.GCStackSize}\n" +
        "gcptr@@: dq 0\n" +

        "section .text\n" +

        


        $"global {entry}\n" +
        $"{entry}:\n" +
        "and rsp, -32\n" +
        "mov rbp, rsp\n" +
        "sub rsp, 32\n" +

        $"mov {(Windows ? "rcx" : "rdi")}, { Settings.GCStackSize * Settings.Bytes }\n" +
        "call malloc\n" +
        "mov [rel gcptr@@], rax\n" +

        "mov rcx, 11\n" +
        "mov rdx, handler@@sigsegv\n" +
        "call signal\n" +

        "sub rsp, 32\n" +
        "mov QWORD [rsp], 0\nmov QWORD [rsp+8], 0\n mov QWORD [rsp+16], 0\nmov QWORD [rsp+24], 0\n" +
        "call main@@\n" +
        (Windows ? "mov rcx, [rsp]\ncall ExitProcess\n" : "") +
        "mov rbx, [rsp]\n" +
        "mov rax, 1\n" +
        "int 80h\n" + 





        "error@@:\n" +
        "push rbp\n" +
        "sub rsp, 32\n" +
        "call printf\n" +
        $"{(Windows ? "mov rcx, -1\ncall ExitProcess\n" : "")}" +
        "mov rbx, 1\n" +
        "mov rax, 0\n" +
        "int 80h\n" +

        "handler@@sigsegv:\n" +
        "push rbp\n" +
        "mov rcx, error@@SEGFAULT\n" +
        "call error@@\n" +





        "gccheck@@:\n" +
        "push rbp\n" +
        "mov rax, [rel gccap@@]\n" +
        "sub rax, 1\n" +
        "mov rbx, [rel gclen@@]\n" +
        "cmp rax, rbx\n" +
        "jg gccheckret@@\n" +
        "xor rdx, rdx\n" +
        "mov rax, [rel gccap@@]\n" +
        "mov rbx, 2\n" +
        "mul rbx\n" +
        "mov [rel gccap@@], rax\n" +

        "sub rsp, 32\n" +
        $"mov {(Windows ? "rcx" : "rdi")}, [rel gcptr@@]\n" +
        "mov rax, [rel gccap@@]\n" +
        $"mov rbx, {Settings.Bytes}\n" +
        "mul rax\n" +
        $"mov {(Windows ? "rdx" : "rsi")}, rax\n" +
        "call realloc\n" +
        "mov [rel gcptr@@], rax\n" +
        "add rsp, 32\n" +

        "gccheckret@@:\n" +
        "pop rbp\n" +
        "ret\n" +


        "gcpush@@:\n" +
        "push rbp\n" +
        "cmp r12, 0\n" +
        "jne gcpush@@body\n" +
        "cmp r13, 0\n" +
        "jl gcpush@@exit\n" +
        "gcpush@@body:\n" +
        "mov rbx, [rel gclen@@]\n" +
        "inc rbx\n" +
        "mov [rel gclen@@], rbx\n" +
        "call gccheck@@\n" +
        "mov rax, [rel gcptr@@]\n" +
        "mov rbx, [rel gclen@@]\n" +
        "mov [rax + rbx*8], r12\n" +
        "gcpush@@exit:\n" +
        "pop rbp\n" +
        "ret\n" +


        "gcframe@@:\n" + 
        "push rbp\n" +
        "mov r12, 0\n" +
        "mov r13, 69\n" +
        "call gcpush@@\n" +
        "mov r13, -69\n" +
        "pop rbp\n" +
        "ret\n" +


        "gcclear@@:\n" +
        "push rbp\n" +
        "gcclearloop@@:\n" +
        "call gcpop@@\n" +
        "cmp r12, 0\n" +
        "jne gcclearloop@@\n" +
        "pop rbp\n" +
        "ret\n" +


        "gcpop@@:\n" +
        "push rbp\n" +

        "mov rbx, [rel gclen@@]\n" +
        "mov r12, [rel gcptr@@]\n" +
        "mov rdi, [r12 + rbx*8]\n" +
        $"{(Windows ? "mov rcx, rdi\n" : "")}" +
        "mov r12, rdi\n" +

        "sub rsp, 32\n" +
        "call free\n" +
        "add rsp, 32\n" +

        "mov rax, [rel gclen@@]\n" +
        "sub rax, 1\n" +
        "mov [rel gclen@@], rax\n" +
        "pop rbp\n" +
        "ret\n" + 


        "gctrypop@@:\n" +
        "push rbp\n" +

        "mov rbx, [rel gclen@@]\n" +
        "mov rax, [rel gcptr@@]\n" +
        "mov rax, [rax + rbx*8]\n" +
        "cmp rax, r12\n" +
        "jne .gctrypop@@exit\n" +

        "mov rbx, [rel gclen@@]\n" +
        "dec rbx\n" +
        "mov [rel gclen@@], rbx\n" +

        ".gctrypop@@exit:\n" +
        "pop rbp\n" +
        "ret\n";

        // Preparations
        foreach(var fn in AST.Fndefs)
        {
            int offset = 0;
            for(int i = 0; i < fn.VarsInternal.Count; i++)
            {
                var vari = fn.VarsInternal[i];
                var info = vari.Type.GetInfo(AST.TypeInfos);

                offset = offset.Pad(info.Alignment);
                offset += info.ByteSize;

                vari.Offset = offset;
            }

            // Enforce bit alignment
            fn.StackSize = offset.Pad(16);

            offset = 0;
            for(int i = 0; i < fn.ArgsInternal.Count; i++)
            {
                var arg = fn.ArgsInternal[i];
                var info = arg.Type.GetInfo(AST.TypeInfos);

                offset = offset.Pad(info.Alignment);
                // while(offset % info.Alignment != 0) offset++;
                arg.Offset = offset;

                offset += info.ByteSize;
            }

            fn.ArgsSize = offset;
            fn.ArgsPadSize = offset.Pad(16) - fn.ArgsSize;
        }

        foreach(var fn in AST.Fndefs)
        {
            var fnName = fn.Name;
            if(fnName == "main") fnName = "main@@";

            string fnBoilerplate = 
            $"{fnName}:\n" +
            "push rbp\n" +
            "mov rbp, rsp\n" +
            $"sub rsp, {fn.StackSize}\n" +
            "{0}" +
            // "push QWORD[rbp]\n" + // Long live my own approach, succumbed under the pressure of world's most used ABIs
            "mov rsp, rbp\n" +
            "pop rbp\n" + 
            "ret\n";

            string fnCode = GenerateBlock(fn, fn.Block, 0);

            fnBoilerplate = string.Format(fnBoilerplate, fnCode);

            result += fnBoilerplate;
        }

        result = string.Format(result, StringData);

        return result;
    }

    private string GenerateBlock(FndefNode fn, BlockNode block, int nest)
    {
        string fnCode = "";
        if(!block.Manual) fnCode += "call gcframe@@\n";
        if(!block.Manual) nest++;

        foreach(var line in block.Statements)
        {
            if(line is LetNode let)
            {
                var vari = fn.GetVar(let.Var.WorkingName);
                var info = vari.Type.GetInfo(AST.TypeInfos);

                fnCode += GenerateExpr(fn, let.Expr);

                if(!let.Alloc)
                {
                    fnCode += $"lea rsi, [rsp]\n"; // source
                    fnCode += $"lea rdi, [rbp-{vari.Offset}]\n"; // destination
                    fnCode += $"mov rcx, {info.ByteSize}\n"; // length
                    fnCode += $"rep movsb\n"; // pretty much memcpy
                    fnCode += $"add rsp, {info.ByteSize.Pad(16)}\n";
                }
                else
                {
                    var allocType = vari.Type.Copy();
                    allocType.RemoveLastMod();
                    var allocInfo = allocType.GetInfo(AST.TypeInfos);

                    fnCode += $"mov rdi, 1\nmov rcx, 1\n";
                    fnCode += $"mov rsi, {allocInfo.ByteSize}\n";
                    fnCode += $"mov rdx, {allocInfo.ByteSize}\n";
                    fnCode += "sub rsp, 32\ncall calloc\nadd rsp, 32\n";
                    fnCode += $"mov rdi, rax\nmov rsi, rsp\nmov rcx, {allocInfo.ByteSize}\n";
                    fnCode += "rep movsb\n";
                    fnCode += $"add rsp, {allocInfo.ByteSize.Pad(16)}\n";
                    fnCode += $"mov [rbp-{vari.Offset}], rax\n";
                }

            }
            else if(line is CallNode call)
            {
                fnCode += GenerateExpr(fn, call.Expr);
                fnCode += $"add rsp, {call.Expr.Type.GetInfo(AST.TypeInfos).ByteSize.Pad(16)}\n";
            }
            else if(line is MutNode mut)
            {
                System.Console.WriteLine($"{mut.Expr.Type}");
                var size = mut.Expr.Type.GetInfo(AST.TypeInfos).ByteSize;

                fnCode += GenerateExpr(fn, mut.Expr);
                fnCode += GenerateExprAddress(fn, mut.Var);

                System.Console.WriteLine($"{mut.Origin} {size} {mut.Expr.Type}");
                fnCode += $"lea rsi, [rsp]\n";
                fnCode += $"mov rdi, rax\n";
                fnCode += $"mov rcx, {size}\n";
                fnCode += "rep movsb\n";
                fnCode += $"add rsp, {size.Pad(16)}\n";
            }
            else if(line is ReturnNode ret)
            {
                // if(ret.Nothing) fnCode += $"mov QWORD [{rbp+}], 0\n";
                if(!ret.Nothing)
                {
                    var returnAddress = 
                        Settings.Bytes + Settings.Bytes + 
                        fn.ArgsSize + fn.ArgsPadSize;

                    fnCode += GenerateExpr(fn, ret.Expr);
                    fnCode += $"lea rdi, [rbp+{returnAddress}]\n";
                    fnCode += "lea rsi, [rsp]\n";
                    fnCode += $"mov rcx, {fn.RetType.GetInfo(AST.TypeInfos).ByteSize}\n";
                    fnCode += "rep movsb\n";
                }

                fnCode += "call gcclear@@\n".Repeat(nest);

                fnCode += "mov rsp, rbp\n";
                fnCode += "pop rbp\n";
                fnCode += "ret\n";
            }
            else if(line is IfNode ifn)
            {
                var endifLabel = GetLabel("endif");
                var label = GetLabel("else");

                fnCode += GenerateExpr(fn, ifn.Condition);
                fnCode += $"mov rax, [rsp]\nadd rsp, 16\nand rax, 1\ncmp rax, 1\njne {label}\n";

                fnCode += GenerateBlock(fn, ifn.Block, nest);

                fnCode += $"jmp {endifLabel}\n";
                fnCode += $"{label}:\n";

                if(ifn.Else is not null)
                {
                    fnCode += GenerateBlock(fn, ifn.Else, nest);
                }

                fnCode += $"{endifLabel}:\n";
            }
            else if(line is WhileNode wh)
            {
                var endLabel = GetLabel("endwhile");
                var doLabel = GetLabel("do");

                var condition = GenerateExpr(fn, wh.Condition);
                condition += $"mov rax, [rsp]\n";
                condition += $"add rsp, 16\n";
                condition += $"cmp rax, 1\n";
                condition += $"jne {endLabel}\n";

                var blockCode = GenerateBlock(fn, wh.Block, nest);

                fnCode += $"{doLabel}:\n";
                if(!wh.Do) { fnCode += condition + blockCode; }
                else       { fnCode += blockCode + condition; }
                fnCode += $"jmp {doLabel}\n";
                fnCode += $"{endLabel}:\n";
            }
            else if(line is ForNode forn)
            {
                var iter = fn.GetVar(forn.Iterator.WorkingName);

                var checkLabel = GetLabel("forcheck");
                var endLabel = GetLabel("forend");
                var stepLabel = GetLabel("forstep");
                var tempLabel = GetLabel("fortemp");

                fnCode += GenerateExpr(fn, forn.Init);
                fnCode += GenerateExpr(fn, forn.Until);
                fnCode += "pop rax\nadd rsp, 8\n";
                fnCode += "pop rbx\nmov [rsp], rbx\n";
                fnCode += "push rax\n";
                fnCode += $"mov [rbp-{iter.Offset}], rbx\n";
                fnCode += $"jmp {checkLabel}\n";

                fnCode += $"{stepLabel}:\n";
                fnCode += "mov rax, [rsp]\n";
                fnCode += "mov rbx, [rsp+8]\n";
                fnCode += "cmp rbx, rax\n"; // iter <> until
                fnCode += $"jl {tempLabel}\n";
                fnCode += "sub rbx, 2\n";
                fnCode += $"{tempLabel}:\n";
                fnCode += "inc rbx\n";
                fnCode += $"mov [rsp+8], rbx\n";
                fnCode += $"mov [rbp-{iter.Offset}], rbx\n";

                fnCode += $"{checkLabel}:\n";
                fnCode += "mov rax, [rsp]\n";
                fnCode += "mov rbx, [rsp+8]\n";
                fnCode += "cmp rax, rbx\n";
                fnCode += $"je {endLabel}\n";

                fnCode += GenerateBlock(fn, forn.Block, nest);
                fnCode += $"jmp {stepLabel}\n";
                fnCode += $"{endLabel}:\n";
            }
            else throw new System.Exception();
        }

        if(!block.Manual) fnCode += "call gcclear@@\n";
        return fnCode;
    }

    private int lastTempLabel = 0;
    private string GetLabel(string label = "temp") => $"_{label}$$${lastTempLabel++}";

    private string GenerateExpr(FndefNode fn, IExpr expr)
    {
        string result = "";
        if(expr is BinopNode binop)
        {
            result += GenerateExpr(fn, binop.Left);
            result += GenerateExpr(fn, binop.Right);

            if(binop.LeftType == VType.Int && binop.RightType == VType.Int)
            {
                result += $"mov rax, [rsp+16]\n";
                result += $"mov rbx, [rsp]\n";
                result += $"add rsp, 16\n";

                var label = GetLabel();
                result += binop.Operator.Type switch 
                {
                    TokenType.Plus => "add rax, rbx\n",
                    TokenType.Minus => "sub rax, rbx\n",
                    TokenType.Mul => "xor rdx, rdx\nimul rbx\n",
                    TokenType.Div => "xor rdx, rdx\ncqo\nidiv rbx\n",
                    TokenType.Mod => "xor rdx, rdx\ncqo\nidiv rbx\nmov rax, rdx\n",
                    TokenType.Modt => 
                        "xor rdx, rdx\n" +
                        "cqo\n" + 
                        "idiv rbx\n" +
                        "mov rax, rdx\n" +
                        "add rax, rbx\n" + 
                        "xor rdx, rdx\n" +
                        "cqo\n" + 
                        "idiv rbx\n" +
                        "mov rax, rdx\n",

                    TokenType.Eq => $"cmp rax, rbx\nmov rax, 1\nje {label}\nmov rax, 0\n{label}:\n",
                    TokenType.Neq => $"cmp rax, rbx\nmov rax, 1\njne {label}\nmov rax, 0\n{label}:\n",

                    TokenType.Ls => $"cmp rax, rbx\nmov rax, 1\njl {label}\nmov rax, 0\n{label}:\n",
                    TokenType.Le => $"cmp rax, rbx\nmov rax, 1\njle {label}\nmov rax, 0\n{label}:\n",
                    TokenType.Gr => $"cmp rax, rbx\nmov rax, 1\njg {label}\nmov rax, 0\n{label}:\n",
                    TokenType.Ge => $"cmp rax, rbx\nmov rax, 1\njge {label}\nmov rax, 0\n{label}:\n",

                    _ => throw new System.Exception()
                };

                result += $"mov [rsp], rax\n";
            }
            else throw new System.Exception();
        }
        else if(expr is IntLit intlit)
        {
            string value = intlit.Value.IntValue.ToString();
            if(intlit.Type == VType.Double || intlit.Type == VType.Float)
            {
                // if(intlit.Type == VType.Double)
                //     value = BitConverter.ToString(BitConverter.GetBytes(intlit.Value.DoubleValue));
                // else
                //     value = BitConverter.ToString(BitConverter.GetBytes(intlit.Value.FloatValue));

                var mod = intlit.Type == VType.Double ? "64" : "32";
                var v = intlit.Type == VType.Double ? intlit.Value.FloatValue.ToString() : intlit.Value.DoubleValue.ToString();
                if(!v.Contains(".")) v += ".0";
                value = $"__float{mod}__({v})";
            }

            result += $"push 0\npush {value}\n";
        }
        else if(expr is BoolLit boolLit)
        {
            result += $"push 0\npush {(boolLit.Value ? 1 : 0)}\n";
        }
        else if(expr is StrLit strlit)
        {
            var label = GetStringLabel(strlit.Value);
            var lenlabel = label + "LEN";
            result += $"mov rdi, [rel {lenlabel}]\n";
            result += "mov rsi, 1\nmov rdx, 1\n";
            result += $"push rdi\n";
            result += "push 0\n";
            result += "add rdi, 8\n"; // null termination
                if(Windows) result += "mov rcx, rdi\n";
            result += "sub rsp, 32\n";
            result += "call calloc\n";
            result += "add rsp, 32\n";
            result += "mov [rsp], rax\n";
            result += "mov r12, rax\n";
            result += "call gcpush@@\n";
            result += "mov rdi, r12\n";
            result += $"mov rsi, {label}\n";
            result += $"mov rcx, [rel {lenlabel}]\n";
            result += "rep movsb\n";
        }
        else if(expr is NegateOp negop)
        {
            result += GenerateExpr(fn, negop.Expr);

            if(negop.Type! == VType.Bool)
            {
                var label = GetLabel("not");

                result += "pop rax\n";
                result += "cmp rax, 0\n";
                result += "push 1\n";
                result += $"je {label}\n";
                result += "add rsp, 8\n";
                result += "push 0\n";
                result += $"{label}:\n";
            }
            else if(negop.Type! == VType.Int)
            {
                result += "pop rax\n";
                result += "not rax\n";
                result += "push rax\n";
            }
        }
        else if(expr is MinusOp minus)
        {
            result += GenerateExpr(fn, minus.Expr);

            if(minus.Type! == VType.Int) 
                result += "pop rax\nneg rax\npush rax\n";
            else if(minus.Type! == VType.I32) 
                result += "pop rax\nneg eax\npush rax\n";
            else if(minus.Type! == VType.Byte) 
                result += "pop rax\nneg al\npush rax\n";
            else throw new System.Exception("dasniodn");
        }
        else if(expr is ManualOp manop)
        {
            result += GenerateExpr(fn, manop.Expr);
            result += "mov r12, [rsp]\n";
            result += "call gctrypop@@\n";
        }
        else if(expr is Var varl)
        {
            var type = AST.Identifiers.Find(id => id.WorkingName == varl.WorkingName)!.Type.Copy();
            System.Console.WriteLine($"{type} {varl.WorkingName}");
            // TODO: I'm almost sure that I fucked up accessors' order here
            if(varl.Accessors.Count == 0 || varl.Accessors.First() is not FuncAcc)
            {
                var isArg = fn.IsArg(varl.WorkingName, out var sv);
                var info = sv.Type.GetInfo(AST.TypeInfos);
                var index = isArg
                    ? sv.Offset + Settings.Bytes * 2
                    : sv.Offset;
                var op = isArg ? "+" : "-";

                result += $"sub rsp, {info.ByteSize.Pad(16)}\n";
                result += $"lea rsi, [rbp{op}{index}]\n";
                result += $"lea rdi, [rsp]\n";
                result += $"mov rcx, {info.ByteSize}\n";
                result += $"rep movsb\n";

            }
            else if(varl.Accessors.First() is FuncAcc func)
            {
                FndefNode? fnn;
                CFndefNode? cfn;
                if((fnn = AST.Fndefs.Find(f => f.Name == varl.Name)) is not null)
                {
                    var label = varl.WorkingName;
                    if(label == "main") label = "main@@";

                    var retInfo = fnn.RetType.GetInfo(AST.TypeInfos);

                    result += $"sub rsp, {retInfo.ByteSize.Pad(16)}\n";
                    result += $"sub rsp, {fnn.ArgsPadSize + fnn.ArgsSize}\n";

                    var args = new List<IExpr>(func.Args);

                    for(int i = args.Count - 1; i >= 0; i--)
                    {
                        var argExpr = args[i];
                        var offset = fnn.ArgsInternal[i].Offset;
                        var typeInfo = argExpr.Type.GetInfo(AST.TypeInfos);

                        result += GenerateExpr(fn, argExpr);
                        result += $"lea rdi, [rsp+{offset+typeInfo.ByteSize.Pad(16)}]\n";
                        result += $"lea rsi, [rsp]\n";
                        result += $"mov rcx, {typeInfo.ByteSize}\n";
                        result += $"rep movsb\n";
                        result += $"add rsp, {typeInfo.ByteSize.Pad(16)}\n";
                    }

                    result += $"call {label}\n";
                    result += $"add rsp, {fnn.ArgsPadSize + fnn.ArgsSize}\n";
                }
                else if((cfn = AST.CFndefs.Find(cf => cf.Name == varl.Name)) is not null)
                {
                    var retTypeInfo = cfn.RetType.GetInfo(AST.TypeInfos);
                    result += $"sub rsp, {retTypeInfo.ByteSize.Pad(16)}\n";

                    if(Windows)
                    {
                        // Win 64
                        // 
                        // Ima write the algorithm first:
                        // 1. Evaluate all arguments
                        // 2. Calculate stack frame size
                        // 3. Assign arguments from right to left

                        int[] nonPointerSizes = [1, 2, 4, 8];
                        bool returnFitsInRegister = nonPointerSizes.Contains(retTypeInfo.ByteSize);
                        int restoreStack = 32;

                        Queue<int> offsets = new();
                        for(int i = func.Args.Count - 1; i >= 0; i--)
                        {
                            var argExpr = func.Args[i];
                            var argInfo = argExpr.Type.GetInfo(AST.TypeInfos);
                            if(i != func.Args.Count - 1)
                                offsets.Enqueue(argInfo.ByteSize.Pad(16));
                            restoreStack += argInfo.ByteSize.Pad(16);
                            result += GenerateExpr(fn, argExpr);
                        }
                        
                        var pad = func.Args.Count > 4 && func.Args.Count % 2 != 0;
                        if(pad) 
                        { 
                            restoreStack += Settings.Bytes; 
                            result += "sub rsp, 8\n"; 
                            offsets.Enqueue(Settings.Bytes);
                        }
                        if(func.Args.Count > 4) 
                            { restoreStack += func.Args.Count * Settings.Bytes; }

                        int extraOffset = 0;
                        for(int i = func.Args.Count - 1; i >= 0; i--)
                        {
                            var offset = offsets.Sum() + extraOffset;
                            var argInfo = func.Args[i].Type.GetInfo(AST.TypeInfos);

                            System.Console.WriteLine($"FN CALL {cfn.Name} {func.Args[i].Type} {argInfo}");

                            var byPointer = !nonPointerSizes.Contains(argInfo.ByteSize);
                            if(func.Args[i].Type.Is<VArray>()) byPointer = false;
                            var op = byPointer ? "lea" : "mov";
                            var ft = func.Args[i].Type == VType.Double || func.Args[i].Type == VType.Float;

                            // FIXME: Figure out what to do with non 8byte values
                            if(i == 0)      result += $"{op} {(ft ? "xmm0" : "rcx")}, [rsp+{offset}]\n";
                            else if(i == 1) result += $"{op} {(ft ? "xmm1" : "rdx")}, [rsp+{offset}]\n";
                            else if(i == 2) result += $"{op} {(ft ? "xmm2" :  "r8")}, [rsp+{offset}]\n";
                            else if(i == 3) result += $"{op} {(ft ? "xmm3" :  "r9")}, [rsp+{offset}]\n";

                            else
                            {
                                extraOffset += Settings.Bytes;
                                result += $"lea rsi, [rsp+{offset}]\n";
                                result += "push 0\n";
                                result += "lea rdi, [rsp]\n";
                                result += $"mov rcx, {argInfo.ByteSize}\n";
                                result += "rep movsb\n";
                            }

                            if(offsets.Count > 0) offsets.Dequeue();
                        }

                        result += "sub rsp, 32\n";
                        result += $"call {cfn.CName}\n";
                        result += $"add rsp, {restoreStack}\n";

                        if(returnFitsInRegister) 
                            result += $"mov [rsp], rax\n";
                    }
                    else
                    {
                        // System V
                    }
                }
                else throw new System.Exception("amgogus");
            }

            bool first = true;
            foreach(var accessor in varl.Accessors)
            {
                // HACK: Make more generic later
                if(first && accessor is FuncAcc) { first = false; continue;  }
                first = false;

                var tinfo = type.GetInfo(AST.TypeInfos);
            
                if(accessor is PointerAcc)
                {
                    type.RemoveLastMod();
                    var info = type.GetInfo(AST.TypeInfos);

                    result += $"pop rax\n";
                    result += "add rsp, 8\n";
                    result += $"sub rsp, {info.ByteSize.Pad(16)}\n";
                    result += $"lea rsi, [rax]\n";
                    result += $"lea rdi, [rsp]\n";
                    result += $"mov rcx, {info.ByteSize}\n";
                    result += "rep movsb\n";
                }
                else if(accessor is ArrayAcc arr)
                {
                    type.RemoveLastMod();
                    var info = type.GetInfo(AST.TypeInfos);

                    var oob = GetLabel("OutOfBounds");
                    var ib = GetLabel("InBounds");

                    result += GenerateExpr(fn, arr.Index);
                    result += "pop rbx\n"; // Index

                    result += "add rsp, 8\n";
                    result += $"pop rax\n"; // Address
                    // result += "add rsp, 8\n";
                    result += "pop rcx\n"; // Length
                    // result += "mov rcx, [rsp]\n"; // Length
                    result += "cmp rbx, rcx\n";
                    result += $"jge {oob}\n";
                    result += "cmp rbx, 0\n";
                    result += $"jge {ib}\n";
                    result += $"{oob}:\n";

                    result += "mov r8, rcx\n";
                    result += "mov rdx, rbx\n";

                    // Pass the string by its adress kids
                    result += "mov rcx, error@@OutOfBounds\n";

                    result += $"call error@@\n";
                    result += $"{ib}:\n";

                    System.Console.WriteLine($"{type} {info.ByteSize}");

                    result += $"lea rsi, [rax+rbx*{info.ByteSize}]\n";
                    result += $"sub rsp, {info.ByteSize.Pad(16)}\n";
                    result += $"lea rdi, [rsp]\n";
                    result += $"mov rcx, {info.ByteSize}\n";
                    result += "rep movsb\n";
                }
                else if(accessor is MemberAcc mAcc)
                {
                    System.Console.WriteLine($"{type}");
                    System.Console.WriteLine($"{tinfo}");

                    var member = tinfo.Members.Find(m => m.Name == mAcc.Member);
                    type = member.Type;

                    var minfo = member.Type.GetInfo(AST.TypeInfos);
                    var msize = minfo.ByteSize.Pad(16);
                    var tsize = tinfo.ByteSize.Pad(16);
                    result += $"lea rsi, [rsp+{member.Offset}]\n";
                    result += $"sub rsp, {msize}\n";
                    result += $"lea rdi, [rsp]\n";
                    result += $"mov rcx, {minfo.ByteSize}\n";
                    result += "rep movsb\n";

                    result += $"lea rsi, [rsp]\n";
                    // We cover the space of newly allocated member, the
                    // original aggregate type, and then add enough space
                    // to store the member
                    // This is verbose (m+t-m), but I think it's needed here
                    result += $"lea rdi, [rsp+{msize+tsize-msize}]\n";
                    result += $"mov rcx, {minfo.ByteSize}\n";
                    result += "rep movsb\n";
                    result += $"add rsp, {msize+tsize-msize}\n";
                }
                else throw new System.Exception("peepee poopoo");
            }

        }
        else if(expr is PointerOp ptr)
        {
            // This is guaranteed by the parser
            // After parsing @ unary operator, only accepted next symbol is Id
            result += GenerateExprAddress(fn, ptr.Expr as Var);
            result += $"sub rsp, 16\nmov [rsp], rax\n";
        }
        else if(expr is ArrayLit arr)
        {
            var type = arr.Type!.Copy();
            type.RemoveLastMod();
            var info = type.GetInfo(AST.TypeInfos);

            result += $"sub rsp, 16\n";
            result += $"mov QWORD [rsp+8], {arr.Elems.Count}\n";
            result += $"mov {(Windows ? "rcx" : "rdi")}, {arr.Elems.Count+1}\n";
            result += $"mov {(Windows ? "rdx" : "rsi")}, {info.ByteSize}\n";
            result += "sub rsp, 32\n";
            result += $"call calloc\n";
            result += "add rsp, 32\n";
            result += $"mov [rsp], rax\n";
            // TODO: Not push if manual mode specified
            result += $"mov r12, rax\n";
            result += $"call gcpush@@\n";

            for(int i = 0; i < arr.Elems.Count; i++)
            {
                var elem = arr.Elems[i];
                var offset = i * info.ByteSize;

                result += "mov r12, [rsp]\n";
                result += GenerateExpr(fn, elem);
                result += "lea rsi, [rsp]\n";
                result += $"lea rdi, [r12 + {offset}]\n";
                result += $"mov rcx, {info.ByteSize}\n";
                result += "rep movsb\n";
                result += $"add rsp, {info.ByteSize.Pad(16)}\n";
            }
        }
        else if(expr is ConstructorLit ctor)
        {
            var type = ctor.Type;
            var info = type.GetInfo(AST.TypeInfos);
            var size = info.ByteSize.Pad(16);

            System.Console.WriteLine($"{type}");
            System.Console.WriteLine($"{info}");
            System.Console.WriteLine($"{size}");

            var stackFrame = info.Members.Sum(m => m.Type.GetInfo(AST.TypeInfos).ByteSize.Pad(16));
            Queue<int> offsets = new();
            foreach(var arg in ctor.Arguments)
            {
                offsets.Enqueue(arg.Expr.Type.GetInfo(AST.TypeInfos).ByteSize.Pad(16));
                result += GenerateExpr(fn, arg.Expr);
            }
            if(offsets.Count > 0) offsets.Dequeue();

            result += $"sub rsp, {size}\n";

            foreach(var member in info.Members)
            {
                var offset = offsets.Sum() + size;
                result += $"lea rdi, [rsp+{member.Offset}]\n";
                result += $"lea rsi, [rsp+{offset}]\n";
                result += $"mov rcx, {member.Type.GetInfo(AST.TypeInfos).ByteSize}\n";
                result += "rep movsb\n";
                if(offsets.Count > 0) offsets.Dequeue();
            }

            result += $"lea rsi, [rsp]\n";
            result += $"lea rdi, [rsp+{stackFrame}]\n";
            result += $"mov rcx, {info.ByteSize}\n";
            result += "rep movsb\n";
            result += $"add rsp, {stackFrame}\n";
        }
        else if(expr is ArrayInitOp arrinit)
        {
            var type = arrinit.Type!.Copy();
            type.RemoveLastMod();
            var info = type.GetInfo(AST.TypeInfos);

            result += GenerateExpr(fn, arrinit.Expr);
            result += "mov rax, [rsp]\n";
            result += "add rsp, 16\n";

            result += $"sub rsp, 16\n";
            result += $"mov QWORD [rsp+8], rax\n";
            result += "add rax, 8\n"; // guaranteed null terminator
            result += $"mov {(Windows ? "rcx" : "rdi")}, rax\n";
            result += $"mov {(Windows ? "rdx" : "rsi")}, {info.ByteSize}\n";
            result += "sub rsp, 32\n";
            result += $"call calloc\n";
            result += "add rsp, 32\n";
            result += $"mov [rsp], rax\n";
            // TODO: Not push if manual mode specified
            result += $"mov r12, rax\n";
            result += $"call gcpush@@\n";
        }
        else throw new System.Exception(expr.GetType().ToString());

        return result;
    }

    // STORES ADDRESS IN RAX
    private string GenerateExprAddress(FndefNode fn, Var expr)
    {
        string result = "";

        var wname = expr.WorkingName;
        var isArg = fn.IsArg(wname, out var sv);
        var index = isArg
            ? sv.Offset + Settings.Bytes * 2
            : sv.Offset;
        var op = isArg ? "+" : "-";
        result += $"lea rax, [rbp{op}{index}]\n";

        var type = sv.Type.Copy();
        foreach(var accessor in expr.Accessors)
        {
            type.RemoveLastMod();
            var isLast = expr.Accessors.Last() == accessor;
            var info = type.GetInfo(AST.TypeInfos);
            if(accessor is PointerAcc ptrAcc)
            {
                //if(isLast) { continue; }
                result += $"mov rax, [rax]\n";
            }
            else if(accessor is ArrayAcc arrAcc)
            {
                // TODO: OOB checking
                result += "mov rax, [rax]\n";
                result += "push rax\npush 0\n";
                result += GenerateExpr(fn, arrAcc.Index);
                result += "pop rbx\n";
                result += $"add rsp, 16\n";
                result += "pop rax\n";

                result += $"lea rax, [rax+rbx*{info.ByteSize}]\n";
            }
            else if(accessor is MemberAcc mAcc)
            {
                var member = info.Members.Find(m => m.Name == mAcc.Member);
                result += $"add rax, {member.Offset}\n";
            }
            else throw new System.Exception("SHIIIT");
        }

        return result;
    }
}
