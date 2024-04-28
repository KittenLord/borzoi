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
            }

            fnBoilerplate = string.Format(fnBoilerplate, fnCode);

            result += fnBoilerplate;
        }



        return result;
    }

    private string GenerateExpr(FndefNode fn, IExpr expr)
    {
        string result = "";
        if(expr is BinopNode binop)
        {
            result += GenerateExpr(fn, binop.Left);
            result += GenerateExpr(fn, binop.Right);
            result += "pop rbx\n";
            result += "pop rax\n";

            result += binop.Operator.Type switch 
            {
                TokenType.Plus => "add rax, rbx\n",
                TokenType.Minus => "sub rax, rbx\n",
                TokenType.Mul => "mul rbx\n",
                _ => throw new System.Exception()
            };

            result += "push rax\n";
        }
        else if(expr is IntLit intlit)
        {
            result += $"push {intlit.Value}\n";
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
