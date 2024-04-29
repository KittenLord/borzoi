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
@$"BITS 64
{(windows ? "extern ExitProcess" : "")}
section .text
global _start
_start:
mov rbp, rsp
push rbp
call main
{(windows ? "mov rcx, rax\ncall ExitProcess" : "")}
mov rbx, rax
mov rax, 1
int 80h
";

        foreach(var fn in AST.Fndefs)
        {
            string fnBoilerplate = 
@$"{fn.Name}:
mov rbp, rsp
sub rsp, {Settings.Bytes * fn.VarsInternal.Count}
{{0}}
push QWORD[rbp]
ret";

            string fnCode = "";
            foreach(var line in fn.Block.Statements)
            {
                if(line is LetNode let)
                {
                    var varIndex = fn.VarsInternal.IndexOf(let.Var.WorkingName);
                    varIndex = (varIndex + 1) * Settings.Bytes;

                    var expr = GenerateExpr(fn, let.Expr);
                    fnCode += expr;
                    fnCode += $"pop rax\nmov [rbp-{varIndex}], rax\n";
                }
                else if(line is ReturnNode ret)
                {
                    if(ret.Nothing) fnCode += "mov rax, 0\n";
                    else 
                    {
                        var expr = GenerateExpr(fn, ret.Expr);
                        fnCode += expr;
                        fnCode += "pop rax\n";
                    }

                    fnCode += "push QWORD[rbp]\nret\n";
                }
            }

            fnBoilerplate = string.Format(fnBoilerplate, fnCode);

            result += fnBoilerplate;
        }



        return result;
    }

    private int lastTempLabel = 0;
    private string GetTempLabel() => $"_$$$temp{lastTempLabel++}";

    private string GenerateExpr(FndefNode fn, IExpr expr)
    {
        string result = "";
        if(expr is BinopNode binop)
        {
            result += GenerateExpr(fn, binop.Left);
            result += GenerateExpr(fn, binop.Right);
            result += "pop rbx\n";
            result += "pop rax\n";

            if(binop.LeftType == VType.Int && binop.RightType == VType.Int)
            {
                var label = GetTempLabel();
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
            }
            else throw new System.Exception();

            result += "push rax\n";
        }
        else if(expr is IntLit intlit)
        {
            result += $"push {intlit.Value}\n";
        }
        else if(expr is BoolLit boolLit)
        {
            result += $"push {(boolLit.Value ? 1 : 0)}\n"; // NOTE: perhaps 0xFFFF instead of 0x0001 ?
        }
        else if(expr is Var varl)
        {
            var isArg = fn.ArgsInternal.Contains(varl.WorkingName);
            var index = isArg 
                ? (fn.ArgsInternal.IndexOf(varl.WorkingName) + 2) * Settings.Bytes
                : (fn.VarsInternal.IndexOf(varl.WorkingName) + 1) * Settings.Bytes;
            if(isArg)
                result += $"push QWORD[rbp+{index}]\n";
            else
                result += $"push QWORD[rbp-{index}]\n";
        }

         return result;
    }
}
