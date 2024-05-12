using System.Collections.Generic;
using System.Linq;

using EdComp.ASTn;

namespace EdComp.Generation;

public class Generator
{
    private AST AST;
    private bool Windows;
    private bool Optimize;
    public Generator(AST ast, bool windows, bool optimize)
    {
        AST = ast;
        Windows = windows;
        Optimize = optimize;
    }

    public string Generate()
    {
        string result = 
        "BITS 64\n" +
        (Windows ? "extern ExitProcess\n" : "") +
        "extern malloc\n" + // We ball
        "extern realloc\n" +
        "extern calloc\n" +
        "extern free\n" +
        "extern printf\n" +

        "section .data\n" +
        "$OutOfBounds: db \"Attempted to get item %d of an array with length %d\",0xA,0\n" +
        "$gclen: dq -1\n" +
        $"$gccap: dq {Settings.GCStackSize}\n" +
        "$gcptr: dq 0\n" +

        "section .text\n" +

        


        "global _start\n" +
        "_start:\n" +
        "and rsp, -32\n" +
        "mov rbp, rsp\n" +
        "sub rsp, 32\n" +

        $"mov {(Windows ? "rcx" : "rdi")}, { Settings.GCStackSize * Settings.Bytes }\n" +
        "call malloc\n" +
        "mov [rel $gcptr], rax\n" +

        "sub rsp, 32\n" +
        "call main\n" +
        (Windows ? "mov rcx, [rsp]\ncall ExitProcess\n" : "") +
        "mov rbx, [rsp]\n" +
        "mov rax, 1\n" +
        "int 80h\n" + 





        "$error:\n" +
        "sub rsp, 32\n" +
        "call printf\n" +
        $"{(Windows ? "mov rcx, -1\ncall ExitProcess\n" : "")}" +
        "mov rbx, 1\n" +
        "mov rax, 0\n" +
        "int 80h\n" +




        // FIXME: THE FUCKING GARBAGE COLLECTOR DOESNT WORK AAAAAAAAAAAAAAAAAAAAAAAAA
        "$gccheck:\n" +
        "push rbp\n" +
        "mov rax, [rel $gccap]\n" +
        "sub rax, 1\n" +
        "mov rbx, [rel $gclen]\n" +
        "cmp rax, rbx\n" +
        "jg $gccheckret\n" +
        "xor rdx, rdx\n" +
        "mov rax, [rel $gccap]\n" +
        "mov rbx, 2\n" +
        "mul rbx\n" +
        "mov [rel $gccap], rax\n" +

        "sub rsp, 32\n" +
        $"mov {(Windows ? "rcx" : "rdi")}, [rel $gcptr]\n" +
        "mov rax, [rel $gccap]\n" +
        $"mov rbx, {Settings.Bytes}\n" +
        "mul rax\n" +
        $"mov {(Windows ? "rdx" : "rsi")}, rax\n" +
        "call realloc\n" +
        "mov [rel $gcptr], rax\n" +
        "add rsp, 32\n" +

        "$gccheckret:\n" +
        "pop rbp\n" +
        "ret\n" +


        "$gcpush:\n" +
        "push rbp\n" +
        "mov rbx, [rel $gclen]\n" +
        "inc rbx\n" +
        "mov [rel $gclen], rbx\n" +
        "call $gccheck\n" +
        "mov rax, [rel $gcptr]\n" +
        "mov rbx, [rel $gclen]\n" +
        "mov [rax + rbx*8], r12\n" +
        "pop rbp\n" +
        "ret\n" +


        "$gcframe:\n" + 
        "push rbp\n" +
        "mov r12, 0\n" +
        "call $gcpush\n" +
        "pop rbp\n" +
        "ret\n" +


        "$gcclear:\n" +
        "push rbp\n" +
        "$gcclearloop:\n" +
        "call $gcpop\n" +
        "cmp r12, 0\n" +
        "jne $gcclearloop\n" +
        "pop rbp\n" +
        "ret\n" +


        "$gcpop:\n" +
        "push rbp\n" +

        "mov rbx, [rel $gclen]\n" +
        "mov r12, [rel $gcptr]\n" +
        "mov rdi, [r12 + rbx*8]\n" +
        $"{(Windows ? "mov rcx, rdi\n" : "")}" +
        "mov r12, rdi\n" +

        "sub rsp, 32\n" +
        "call free\n" +
        "add rsp, 32\n" +

        "mov rax, [rel $gclen]\n" +
        "sub rax, 1\n" +
        "mov [rel $gclen], rax\n" +
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
            string fnBoilerplate = 
            $"{fn.Name}:\n" +
            "push rbp\n" +
            "mov rbp, rsp\n" +
            $"sub rsp, {fn.StackSize}\n" +
            "{0}" +
            // "push QWORD[rbp]\n" + // Long live my own approach, succumbed under the pressure of world's most used ABIs
            "mov rsp, rbp\n" +
            "pop rbp\n" + 
            "ret\n";

            string fnCode = GenerateBlock(fn, fn.Block, 1);

            fnBoilerplate = string.Format(fnBoilerplate, fnCode);

            result += fnBoilerplate;
        }



        return result;
    }

    private string GenerateBlock(FndefNode fn, BlockNode block, int nest)
    {
        string fnCode = "";
        fnCode += "call $gcframe\n";

        foreach(var line in block.Statements)
        {
            if(line is LetNode let)
            {
                var vari = fn.GetVar(let.Var.WorkingName);
                var info = vari.Type.GetInfo(AST.TypeInfos);

                if(Optimize && let.Expr is IntLit intlit)
                {

                fnCode += $"mov QWORD [rbp-{vari.Offset}], {intlit.Value}\n";

                }
                else if(Optimize && let.Expr is BoolLit boollit)
                {

                fnCode += $"mov BYTE [rbp-{vari.Offset}], {(boollit.Value ? 1 : 0)}\n";

                }
                else
                {

                fnCode += GenerateExpr(fn, let.Expr);

                if(Optimize && let.Expr.Type.GetInfo(AST.TypeInfos).ByteSize == 8)
                {

                fnCode += "pop rax\n";
                fnCode += $"mov [rbp-{vari.Offset}], rax\n";
                fnCode += $"add rsp, 8\n";

                }
                else if(Optimize && let.Expr.Type.GetInfo(AST.TypeInfos).ByteSize == 16)
                {
                
                fnCode += "pop rax\n";
                fnCode += $"mov [rbp-{vari.Offset}], rax\n";
                fnCode += "pop rax\n";
                fnCode += $"mov [rbp-{vari.Offset}+8], rax\n";

                }
                else
                {

                fnCode += $"lea rsi, [rsp]\n"; // source
                fnCode += $"lea rdi, [rbp-{vari.Offset}]\n"; // destination
                fnCode += $"mov rcx, {info.ByteSize}\n"; // length
                fnCode += $"rep movsb\n"; // pretty much memcpy
                fnCode += $"add rsp, {info.ByteSize.Pad(16)}\n";

                }


                }
            }
            else if(line is CallNode call)
            {
                fnCode += GenerateExpr(fn, call.Expr);
                fnCode += $"add rsp, {call.Expr.Type.GetInfo(AST.TypeInfos).ByteSize.Pad(16)}\n";
            }
            else if(line is MutNode mut)
            {
                var size = mut.Expr.Type.GetInfo(AST.TypeInfos).ByteSize;

                if(Optimize && mut.Expr is IntLit intlit)
                {

                fnCode += GenerateExprAddress(fn, mut.Var);
                fnCode += $"mov QWORD [rax], {intlit.Value}\n";

                }
                else if(Optimize && mut.Expr is BoolLit boollit)
                {

                fnCode += GenerateExprAddress(fn, mut.Var);
                fnCode += $"mov BYTE [rax], {(boollit.Value ? 1 : 0)}\n";

                }
                else
                {

                fnCode += GenerateExpr(fn, mut.Expr);
                fnCode += GenerateExprAddress(fn, mut.Var);

                if(Optimize && mut.Expr.Type.GetInfo(AST.TypeInfos).ByteSize == 8)
                {

                fnCode += "pop rbx\n";
                fnCode += $"mov [rax], rbx\n";
                fnCode += $"add rsp, 8\n";

                }
                else if(Optimize && mut.Expr.Type.GetInfo(AST.TypeInfos).ByteSize == 8)
                {

                fnCode += "pop rbx\n";
                fnCode += $"mov [rax], rbx\n";
                fnCode += $"pop rbx\n";
                fnCode += $"mov [rax+8], rbx\n";

                }
                else
                {

                fnCode += $"lea rsi, [rsp]\n";
                fnCode += $"mov rdi, rax\n";
                fnCode += $"mov rcx, {size}\n";
                fnCode += "rep movsb\n";
                fnCode += $"add rsp, {size.Pad(16)}\n";

                }


                }

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

                fnCode += "call $gcclear\n".Repeat(nest);

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

                fnCode += GenerateBlock(fn, ifn.Block, nest + 1);

                fnCode += $"jmp {endifLabel}\n";
                fnCode += $"{label}:\n";

                if(ifn.Else is not null)
                {
                    fnCode += GenerateBlock(fn, ifn.Else, nest + 1);
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

                var blockCode = GenerateBlock(fn, wh.Block, nest + 1);

                fnCode += $"{doLabel}:\n";
                if(!wh.Do) { fnCode += condition + blockCode; }
                else       { fnCode += blockCode + condition; }
                fnCode += $"jmp {doLabel}\n";
                fnCode += $"{endLabel}:\n";
            }
            else throw new System.Exception();
        }

        fnCode += "call $gcclear\n";
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
                    TokenType.Mod => "xor rdx, rdx\ncqo\nidiv rbx\nmov rax, rdx",

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
            result += $"push 0\npush {intlit.Value}\n";
        }
        else if(expr is BoolLit boolLit)
        {
            result += $"push 0\npush {(boolLit.Value ? 1 : 0)}\n";
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
        else if(expr is Var varl)
        {
            // TODO: I'm almost sure that I fucked up accessors' order here
            if(varl.Accessors.Count == 0 || varl.Accessors.First() is not FuncAcc)
            {
                var isArg = fn.IsArg(varl.WorkingName, out var sv);
                var info = sv.Type.GetInfo(AST.TypeInfos);
                var index = isArg
                    ? sv.Offset + Settings.Bytes * 2
                    : sv.Offset;
                var op = isArg ? "+" : "-";

                if(Optimize && info.ByteSize % 8 == 0)
                {

                if((info.ByteSize / 8) % 2 == 1) 
                    result += "push 0\n";
                for(int i = info.ByteSize / 8 - 1; i >= 0; i--)
                {

                    result += $"push QWORD [rbp{op}{index}+{i * 8}]\n";

                }

                }
                else
                {

                result += $"sub rsp, {info.ByteSize.Pad(16)}\n";
                result += $"lea rsi, [rbp{op}{index}]\n";
                result += $"lea rdi, [rsp]\n";
                result += $"mov rcx, {info.ByteSize}\n";
                result += $"rep movsb\n";

                }

            }
            else if(varl.Accessors.First() is FuncAcc func)
            {
                var label = varl.WorkingName;
                var fnn = AST.Fndefs.Find(f => f.Name == varl.Name);
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
                    result += $"add rsp, {offset+typeInfo.ByteSize.Pad(16)}\n";
                }

                result += $"call {label}\n";
                result += $"add rsp, {fnn.ArgsPadSize + fnn.ArgsSize}\n";
            }

            var type = varl.Type.Copy();
            foreach(var accessor in varl.Accessors)
            {
                type.RemoveLastMod();
                var info = type.GetInfo(AST.TypeInfos);

                if(accessor is PointerAcc)
                {
                    result += $"pop rax\n";
                    result += "add rsp, 8\n";
                    result += $"sub rsp, {info.ByteSize.Pad(16)}\n";
                    result += $"lea rsi, [rax]\n";
                    result += $"lea rdi, [rsp]\n";
                    result += $"mov rcx, {info.ByteSize}\n";
                    result += "rep movsb\n";
                }
                // HACK: Bounds checking
                else if(accessor is ArrayAcc arr)
                {
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
                    result += "mov rcx, $OutOfBounds\n";

                    result += $"call $error\n";
                    result += $"{ib}:\n";

                    result += $"lea rsi, [rax+rbx*{info.ByteSize}]\n";
                    result += $"sub rsp, {info.ByteSize.Pad(16)}\n";
                    result += $"lea rdi, [rsp]\n";
                    result += $"mov rcx, {info.ByteSize}\n";
                    result += "rep movsb\n";
                }
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
            result += $"mov {(Windows ? "rcx" : "rdi")}, {arr.Elems.Count}\n";
            result += $"mov {(Windows ? "rdx" : "rsi")}, {info.ByteSize}\n";
            result += "sub rsp, 32\n";
            result += $"call calloc\n";
            result += "add rsp, 32\n";
            result += $"mov [rsp], rax\n";
            // TODO: Not push if manual mode specified
            result += $"mov r12, rax\n";
            result += $"call $gcpush\n";

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
            result += $"mov {(Windows ? "rcx" : "rdi")}, rax\n";
            result += $"mov {(Windows ? "rdx" : "rsi")}, {info.ByteSize}\n";
            result += "sub rsp, 32\n";
            result += $"call calloc\n";
            result += "add rsp, 32\n";
            result += $"mov [rsp], rax\n";
            // TODO: Not push if manual mode specified
            result += $"mov r12, rax\n";
            result += $"call $gcpush\n";
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

        var type = expr.Type.Copy();
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
                result += "mov rax, [rax]\n";
                result += "mov r12, rax\n";
                result += GenerateExpr(fn, arrAcc.Index);
                result += "mov rbx, [rsp]\n";
                result += "mov rax, r12\n";

                result += $"lea rax, [rax+rbx*{info.ByteSize}]\n";
                result += $"add rsp, 16\n";
            }
            else throw new System.Exception("SHIIIT");
        }

        return result;
    }
}
