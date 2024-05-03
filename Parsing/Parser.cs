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

            var argType = ParseType();
            if(argType is null) { return null; }

            if(!Peek().Is(TokenType.Id)) 
            { Report(Error.Expected([TokenType.Id], Peek())); return null; }

            var argName = Pop();

            args.Add((argType, argName));

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

        var type = VType.Void;
        var typeT = (Token?)null;
        if(Peek().Is(TokenType.Id)) { typeT = Peek(); type = ParseType(); }

        if(!Peek().Is(TokenType.LCurly)) 
        { Report(Error.Expected([TokenType.LCurly], Peek())); return null; }

        var block = ParseBlock();
        if(block is null) { return null; }

        return new FndefNode(origin, name, args, type, typeT, block);
    }

    private VType? ParseType() 
    {
        var type = new VType(Pop().Value);
        TokenType[] modlah = [TokenType.LBrack, TokenType.Pointer];

        while(modlah.Contains(Peek().Type))
        {
            if(Peek().Is(TokenType.LBrack))
            {
                _ = Pop();
                if(Peek().Is(TokenType.IntLit)) 
                    { var size = Pop().IntValue; type.Mods.Add(VTypeMod.Arr(size)); }
                else 
                    type.Mods.Add(VTypeMod.Arr());
                if(Peek().Is(TokenType.RBrack)) { Pop(); continue; }
                Report(Error.Expected([TokenType.RBrack], Peek()));
                return null;
            }
            else if(Peek().Is(TokenType.Pointer))
            {
                var origin = Pop();
                type.Mods.Add(VTypeMod.Pointer());
            }
            else throw new System.Exception("AAAAAAAA");
        }

        return type;
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
            else if(Peek().Is(TokenType.Ret))
            {
                var @return = ParseReturn();
                if(@return is null) { return null; }
                statements.Add(@return);
            }
            else if(Peek().Is(TokenType.Do, TokenType.While))
            {
                var @while = ParseWhile();
                if(@while is null) { return null; }
                statements.Add(@while);
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

        var type = ParseType();
        if(type is null) { return null; }

        if(!Peek().Is(TokenType.Id)) 
        { Report(Error.Expected([TokenType.Id], Peek())); return null; }

        var name = Pop();

        if(!Peek().Is(TokenType.Assign)) 
        { Report(Error.Expected([TokenType.Assign], Peek())); return null; }
        Pop();

        var expr = ParseExpr();
        if(expr is null) { return null; }
        
        // TODO: Type mods
        return new LetNode(origin, type, name, expr);
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

    private ReturnNode? ParseReturn()
    {
        var origin = Pop();

        if(!CanStartLeaf(Peek().Type)) 
            return new ReturnNode(origin);

        var expr = ParseExpr();
        if(expr is null) { return null; }

        return new ReturnNode(origin, expr);
    }

    private WhileNode? ParseWhile()
    {
        var origin = Pop();
        bool doo = origin.Is(TokenType.Do);

        if(doo && !Peek().Is(TokenType.While))
        {
            Report(Error.Expected([TokenType.While], Peek()));
            return null;
        }
        else if(doo)
        {
            _ = Pop();
        }

        var expr = ParseExpr();
        if(expr is null) return null;

        var block = ParseBlock();
        if(block is null) return null;

        return new WhileNode(origin, expr, doo, block);
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

        if(!Peek().Is(TokenType.Else)) 
            return new IfNode(origin, expr, block);

        _ = Pop();
        if(Peek().Is(TokenType.LCurly))
        {
            var @elseBlock = ParseBlock();
            if(@elseBlock is null) { return null; }
            return new IfNode(origin, expr, block, @elseBlock);
        }
        else if(Peek().Is(TokenType.If))
        {
            var elseif = ParseIf();
            if(elseif is null) { return null; }
            var @elseBlock = new BlockNode([elseif]);
            return new IfNode(origin, expr, block, @elseBlock);
        }

        Report(Error.Expected([TokenType.If, TokenType.LCurly], Peek()));
        return null;
    }

    private static readonly TokenType[] Binops = [TokenType.Plus, TokenType.Minus, TokenType.Mul, TokenType.Div, TokenType.Mod, TokenType.Eq, TokenType.Neq, TokenType.Ls, TokenType.Le, TokenType.Gr, TokenType.Ge];
    private bool IsBinop(Token binop) { return Binops.Contains(binop.Type); }
    private int GetBinopPriority(Token binop)
    {
        return binop.Type switch 
        {
            TokenType.Eq or TokenType.Neq or 
            TokenType.Ls or TokenType.Le or TokenType.Gr or TokenType.Ge => 0,
            TokenType.Plus or TokenType.Minus => 1,
            TokenType.Mul or TokenType.Div or TokenType.Mod => 2,
            _ => throw new System.Exception($"Token {binop} is not a binary operator kys")
        };
    }

    private static readonly TokenType[] leafTokens = [TokenType.IntLit, TokenType.BoolLit, TokenType.Id, TokenType.LParen, TokenType.LBrack, TokenType.Mul, TokenType.Pointer];
    private bool CanStartLeaf(TokenType t) => leafTokens.Contains(t);
    private IExpr? ParseExprLeaf()
    {
        if(!CanStartLeaf(Peek().Type)) throw new System.Exception("FUUUCK");
        if(Peek().Is(TokenType.IntLit)) return new IntLit(Pop().IntValue);
        if(Peek().Is(TokenType.BoolLit)) return new BoolLit(Pop().BoolValue);
        if(Peek().Is(TokenType.Pointer))
        {
            var origin = Pop();
            if(!Peek().Is(TokenType.Id))
            {
                Report(Error.Expected([TokenType.Id], Peek()));
                return null;
            }

            var expr = ParseExpr();
            if(expr is null) { return null; }

            return new PointerOp(origin, expr);
        }
        if(Peek().Is(TokenType.Id)) 
        {
            var varn = new Var(Peek(), Pop().Value);

            TokenType[] acclah = [TokenType.LParen, TokenType.LBrack, TokenType.Period, TokenType.Pointer];
            while(Peek().Is(acclah))
            {
                if(Peek().Is(TokenType.LParen))
                {
                    var func = new FuncAcc();
                    varn.Accessors.Add(func);

                    _ = Pop();

                    bool exit = false;
                    if(!Peek().Is(TokenType.RParen))
                    {
                        while(CanStartLeaf(Peek().Type))
                        {
                            var expr = ParseExpr();
                            if(expr is null) { return null; }
                            func.Args.Add(expr);

                            if(Peek().Is(TokenType.Comma)) { _ = Pop(); continue; }
                            if(Peek().Is(TokenType.RParen)) { exit = true; _ = Pop(); break; }
                        }
                        if(!exit)
                        {
                            Report(Error.Expected([TokenType.RParen], Peek()));
                            return null;
                        }
                    }
                    else Pop();
                }
                else if(Peek().Is(TokenType.LBrack))
                {
                    var origin = Pop();
                    if(!CanStartLeaf(Peek().Type))
                    {
                        Report(Error.Expected(leafTokens, Peek()));
                        return null;
                    }

                    var index = ParseExpr();
                    if(index is null) { return null; }

                    if(!Peek().Is(TokenType.RBrack))
                    {
                        Report(Error.Expected([TokenType.RBrack], Peek()));
                        return null;
                    }

                    _ = Pop();
                    varn.Accessors.Add(new ArrayAcc(origin, index));
                }
                else if(Peek().Is(TokenType.Period))
                {
                    var origin = Pop();
                    if(!Peek().Is(TokenType.Id))
                    {
                        Report(Error.Expected([TokenType.Id], Peek()));
                        return null;
                    }

                    varn.Accessors.Add(new MemberAcc(origin, Pop()));
                }
                else if(Peek().Is(TokenType.Pointer))
                {
                    var origin = Pop();
                    varn.Accessors.Add(new PointerAcc(origin));
                }
            }

            return varn;
        }
        if(Peek().Is(TokenType.LParen))
        {
            _ = Pop();
            var expr = ParseExpr();
            if(expr is null) return null;
            if(Peek().Is(TokenType.RParen)) 
            {
                Pop();
                return expr;
            }

            Report(Error.Expected([TokenType.RParen], Peek()));
            return null;
        }
        if(Peek().Is(TokenType.LBrack))
        {
            _ = Pop();
            var arr = new ArrayLit();
            while(CanStartLeaf(Peek().Type))
            {
                var expr = ParseExpr();
                if(expr is null) { return null; }
                arr.Elems.Add(expr);

                if(Peek().Is(TokenType.Comma)) { Pop(); continue; }
                if(Peek().Is(TokenType.RBrack)) { break; }
            }

            if(Peek().Is(TokenType.RBrack)) { Pop(); return arr; }
            Report(Error.Expected([TokenType.Comma, TokenType.RBrack], Peek()));
            return null;
        }
        if(Peek().Is(TokenType.Mul))
        {
            var origin = Pop();
            if(!CanStartLeaf(Peek().Type)) 
                { Report(Error.Expected(leafTokens, Peek())); return null; }
            var expr = ParseExpr();
            if(expr is null) { return null; }
            return new ArrayInitOp(origin, expr);
        }
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
        if(GetBinopPriority(binop) <= precedence) return expr;

        Pop();
        var right = ParseExpr(GetBinopPriority(binop));
        if(right is null) return null;

        return new BinopNode(binop, expr, right);
    }
}
