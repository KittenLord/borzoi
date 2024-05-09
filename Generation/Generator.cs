using System.Collections.Generic;
using System.Linq;

using EdComp.ASTn;

namespace EdComp.Generation;

public class Generator
{
    private AST AST;
    public Generator(AST ast)
    {
        AST = ast;
    }

    public string Generate(bool windows)
    {
        string result = 
        "BITS 64\n" +
        (windows ? "extern ExitProcess\n" : "") +
        "section .text\n" +
        "global _start\n" +
        "_start:\n" +
        "and rsp, -16\n" +
        "mov rbp, rsp\n" +
        "sub rsp, 32\n" + // Why not
        "call main\n" +
        (windows ? "mov rcx, [rsp]\ncall ExitProcess\n" : "") +
        "mov rbx, rax\n" +
        "mov rax, 1\n" +
        "int 80h\n";

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

            string fnCode = GenerateBlock(fn, fn.Block);

            fnBoilerplate = string.Format(fnBoilerplate, fnCode);

            result += fnBoilerplate;
        }



        return result;
    }

    private string GenerateBlock(FndefNode fn, BlockNode block)
    {
        string fnCode = "";

        foreach(var line in block.Statements)
        {
            if(line is LetNode let)
            {
                var vari = fn.GetVar(let.Var.WorkingName);
                var info = vari.Type.GetInfo(AST.TypeInfos);

                fnCode += GenerateExpr(fn, let.Expr);

                fnCode += $"lea rsi, [rsp]\n"; // source
                fnCode += $"lea rdi, [rbp-{vari.Offset}]\n"; // destination
                fnCode += $"mov rcx, {info.ByteSize}\n"; // length
                fnCode += $"rep movsb\n"; // pretty much memcpy

                fnCode += $"add rsp, {info.ByteSize.Pad(16)}\n";
            }
            else if(line is CallNode call)
            {
                fnCode += GenerateExpr(fn, call.Expr);
                fnCode += $"add rsp, {call.Expr.Type.GetInfo(AST.TypeInfos).ByteSize.Pad(16)}\n";
            }
            else if(line is MutNode mut)
            {
                var size = mut.Expr.Type.GetInfo(AST.TypeInfos).ByteSize;
                fnCode += GenerateExpr(fn, mut.Expr);
                fnCode += GenerateExprAddress(fn, mut.Var);
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

                fnCode += GenerateBlock(fn, ifn.Block);

                fnCode += $"jmp {endifLabel}\n";
                fnCode += $"{label}:\n";

                if(ifn.Else is not null)
                {
                    fnCode += GenerateBlock(fn, ifn.Else);
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
                condition += $"and rax, 1\n";
                condition += $"cmp rax, 1\n";
                condition += $"jne {endLabel}\n";

                var blockCode = GenerateBlock(fn, wh.Block);

                fnCode += $"{doLabel}:\n";
                if(!wh.Do) { fnCode += condition + blockCode; }
                else       { fnCode += blockCode + condition; }
                fnCode += $"jmp {doLabel}\n";
                fnCode += $"{endLabel}:\n";
            }
            else throw new System.Exception();
        }
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
            result += $"sub rsp, 16\nmov QWORD [rsp], {intlit.Value}\n";
        }
        else if(expr is BoolLit boolLit)
        {
            result += $"sub rsp, 16\nmov QWORD [rsp], {(boolLit.Value ? 1 : 0)}\n";
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

                result += $"sub rsp, {info.ByteSize.Pad(16)}\n";
                result += $"lea rsi, [rbp{op}{index}]\n";
                result += $"lea rdi, [rsp]\n";
                result += $"mov rcx, {info.ByteSize}\n";
                result += $"rep movsb\n";
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

            foreach(var accessor in varl.Accessors)
            {
                // TODO: Array/pointer stuff here
                if(accessor is PointerAcc)
                {
                    result += $"pop rax\n";
                    result += $"mov rax, [rax]\n";
                    result += $"push rax\n";
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

        return result;
    }

    // STORES ADDRESS IN RAX
    private string GenerateExprAddress(FndefNode fn, Var expr)
    {
        // TODO: apparently "lea" instruction does exactly what I need
        string result = "";

        var wname = expr.WorkingName;
        var isArg = fn.IsArg(wname, out var sv);
        var index = isArg
            ? sv.Offset + Settings.Bytes * 2
            : sv.Offset;
        var op = isArg ? "+" : "-";
        result += $"lea rax, [rbp{op}{index}]\n";

        foreach(var accessor in expr.Accessors)
        {
            var isLast = expr.Accessors.Last() == accessor;
            if(accessor is PointerAcc ptrAcc)
            {
                //if(isLast) { continue; }
                result += $"mov rax, [rax]\n";
            }
            else throw new System.Exception("SHIIIT");
        }

        return result;
    }
}
