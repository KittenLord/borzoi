using System.Collections.Generic;
using System.Linq;
using EdComp.Lexing;
using EdComp.ASTn;

namespace EdComp.Parsing;
using EdComp.Parsing.Msg;

public class Parser
{
    private Lexer Lexer;
    public AST AST { get; set; }
    public List<Message> Errors { get; private set; }
    public bool Success { get; private set; }

    public Parser(Lexer lexer) 
    { 
        Errors = new();
        AST = new();
        Lexer = lexer; 
        Success = true;
    }

    private Token Pop() => Lexer.Pop();
    private Token Peek() => Lexer.Peek();

    private void Report(string error, Token position) => Report(new Message(error, position));
    private void Report(Message error)
    {
        Success = false;
        Errors.Add(error);
    }

    public void Parse(Lexer? l = null)
    {
        Lexer = Lexer ?? l ?? throw new System.Exception("Provide a not-null lexer");

        while(Peek().nEOF)
        {
            if(Peek().Is(TokenType.Fn))
            {
                var fn = ParseFn();
                if(fn is not null) AST.Fndefs.Add(fn);
                // else return;
                continue;
            }
            return;
        }
    }

    public FndefNode? ParseFn()
    {
        var origin = Pop();

        if(!Peek().Is(TokenType.Id)) 
        { Report(Error.Expected([TokenType.Id], Peek())); return null; }

        var name = Pop();


        if(!Peek().Is(TokenType.LParen)) 
        { Report(Error.Expected([TokenType.LParen], Peek())); return null; }

        _ = Pop();

        List<(VType type, Token name)> args = new();
        while(!Peek().Is(TokenType.RParen))
        {
            if(!Peek().Is(TokenType.Id)) 
            { Report(Error.Expected([TokenType.Id], Peek())); return null; }

            var argType = Pop();

            if(!Peek().Is(TokenType.Id)) 
            { Report(Error.Expected([TokenType.Id], Peek())); return null; }

            var argName = Pop();

            var argVType = new VType(argType.Value);
            args.Add((argVType, argName));

            if(Peek().Is(TokenType.RParen)) continue;
            else if(Peek().Is(TokenType.Comma)) 
            {
                _ = Pop();
                if(Peek().Is(TokenType.RParen)) { Report(Error.Expected([TokenType.Id], Peek())); return null; }
            }
            else { Report(Error.Expected([TokenType.Comma, TokenType.RParen], Peek())); return null; }
        }

        _ = Pop();

        if(!Peek().Is(TokenType.Id, TokenType.LCurly)) 
        { Report(Error.Expected([TokenType.Id, TokenType.LCurly], Peek())); return null; }

        var type = (Token?)null;
        if(Peek().Is(TokenType.Id)) { type = Pop(); }

        if(!Peek().Is(TokenType.LCurly)) 
        { Report(Error.Expected([TokenType.LCurly], Peek())); return null; }

        var block = ParseBlock();
        if(block is null) { return null; }

        return new FndefNode(origin, name, args, type, block);
    }

    private BlockNode? ParseBlock()
    {
        _ = Pop();

        List<IStatement> statements = new();
        while(!Peek().Is(TokenType.RCurly))
        {
            if(Peek().Is(TokenType.Let))
            {
                var let = ParseLet();
                if(let is null) { return null; }
                statements.Add(let);
            }
            else if(Peek().Is(TokenType.Mut))
            {
                var mut = ParseMut();
                if(mut is null) { return null; }
                statements.Add(mut);
            }
            else if(Peek().Is(TokenType.If))
            {
                var @if = ParseIf();
                if(@if is null) { return null; }
                statements.Add(@if);
            }
        }

        _ = Pop();

        return new BlockNode(statements);
    }

    private LetNode? ParseLet()
    {
        var origin = Pop();

        if(!Peek().Is(TokenType.Id)) 
        { Report(Error.Expected([TokenType.Id], Peek())); return null; }

        var type = Pop();

        if(!Peek().Is(TokenType.Id)) 
        { Report(Error.Expected([TokenType.Id], Peek())); return null; }

        var name = Pop();

        if(!Peek().Is(TokenType.Assign)) 
        { Report(Error.Expected([TokenType.Assign], Peek())); return null; }
        Pop();

        var expr = ParseExpr();
        if(expr is null) { return null; }
        
        // TODO: Type mods
        var vtype = new VType(type.Value);
        return new LetNode(origin, vtype, name, expr);
    }

    private MutNode? ParseMut()
    {
        var origin = Pop();

        if(!Peek().Is(TokenType.Id)) 
        { Report(Error.Expected([TokenType.Id], Peek())); return null; }

        var name = Pop();

        if(!Peek().Is(TokenType.Assign)) 
        { Report(Error.Expected([TokenType.Assign], Peek())); return null; }
        Pop();

        var expr = ParseExpr();
        if(expr is null) { return null; }
        
        return new MutNode(origin, name, expr);
    }

    private IfNode? ParseIf()
    {
        var origin = Pop();

        var expr = ParseExpr();
        if(expr is null) { return null; }

        if(!Peek().Is(TokenType.LCurly)) 
        { Report(Error.Expected([TokenType.LCurly], Peek())); return null; }

        var block = ParseBlock();
        if(block is null) { return null; }
        
        return new IfNode(origin, expr, block);
    }

    private static readonly TokenType[] Binops = [TokenType.Plus, TokenType.Minus, TokenType.Mul, TokenType.Div, TokenType.Mod];
    private bool IsBinop(Token binop) { return Binops.Contains(binop.Type); }
    private int GetBinopPriority(Token binop)
    {
        return binop.Type switch 
        {
            TokenType.Plus or TokenType.Minus => 1,
            TokenType.Mul or TokenType.Div or TokenType.Mod => 2,
            _ => throw new System.Exception("Token is not a binary operator kys")
        };
    }

    private IExpr? ParseExprLeaf()
    {
        if(Peek().Is(TokenType.IntLit)) return new IntLit(Pop().IntValue);
        if(Peek().Is(TokenType.Id)) return new Var(Peek(), Pop().Value);
        Report("fuck", Peek());
        return null;
    }

    private IExpr? ParseExpr(int precedence = int.MinValue)
    {
        IExpr? expr;
        var left = ParseExprLeaf();
        if(left is null) return null;
        
        while(true)
        {
            expr = ParseExprIncPrec(left, precedence);
            if(expr is null) return null;
            if(expr == left) return expr;
            left = expr;
        }
    }

    private IExpr? ParseExprIncPrec(IExpr expr, int precedence)
    {
        var binop = Peek();
        if(!IsBinop(binop)) return expr;
        if(GetBinopPriority(binop) < precedence) return expr;

        Pop();
        var right = ParseExpr(GetBinopPriority(binop));
        if(right is null) return null;

        return new BinopNode(binop, expr, right);
    }
}
