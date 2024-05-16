using System.Collections.Generic;
using System.Linq;
using Borzoi.Lexing;
using Borzoi.ASTn;

namespace Borzoi.Parsing;
using Borzoi.Parsing.Msg;

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

    private static readonly TokenType[] LeafTokens = 
        [TokenType.IntLit, TokenType.BoolLit, TokenType.StrLit, 
         TokenType.Id, 
         TokenType.LParen, TokenType.LBrack, 
         TokenType.Mul, TokenType.Pointer,
         TokenType.Not,
         TokenType.Manual];

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
            if(Peek().Is(TokenType.Type))
            {
                var type = ParseTypedef();
                if(type is not null) AST.TypeDefs.Add(type);
                // else return;
                continue;
            }
            if(Peek().Is(TokenType.Cfn))
            {
                var cfn = ParseCFn();
                if(cfn is not null) { AST.CFndefs.Add(cfn); }
                else 
                {
                    throw new System.Exception("fuck you");
                }
                continue;
            }
            if(Peek().Is(TokenType.Link))
            {
                var link = ParseLink();
                if(link is not null) { 
                    if(!AST.Links.Contains(link)) 
                        AST.Links.Add(link); }
                else throw new System.Exception("fuck you again");
                continue;
            }
            return;
        }
    }

    public TypedefNode? ParseTypedef()
    {
        var origin = Pop();

        if(!Peek().Is(TokenType.Id))
        { Report(Error.Expected([TokenType.Id], Peek())); return null; }

        var name = Pop();

        if(!Peek().Is(TokenType.LCurly))
        { Report(Error.Expected([TokenType.LCurly], Peek())); return null; }
        Pop();


        List<(VType Type, string Name)> members = new();
        while(!Peek().Is(TokenType.RCurly))
        {
            if(!Peek().Is(TokenType.Id))
            { Report(Error.Expected([TokenType.Id], Peek())); return null; }

            var mType = ParseType();
            if(mType is null) { return null; }

            if(!Peek().Is(TokenType.Id))
            { Report(Error.Expected([TokenType.Id], Peek())); return null; }

            var mName = Pop();

            members.Add((mType, mName.Value));

            if(Peek().Is(TokenType.RCurly)) continue;

            if(!Peek().Is(TokenType.Comma))
            { Report(Error.Expected([TokenType.Comma], Peek())); return null; }
            Pop();
        }

        _ = Pop();

        return new TypedefNode(origin, name.Value, members);
    }

    public string? ParseLink()
    {
        _ = Pop();
        if(!Peek().Is(TokenType.StrLit)) 
        { Report(Error.Expected([TokenType.StrLit], Peek())); return null; }
        
        return Pop().Value;
    }

    public CFndefNode? ParseCFn()
    {
        var origin = Pop();

        if(!Peek().Is(TokenType.Id)) 
        { Report(Error.Expected([TokenType.Id], Peek())); return null; }

        var name = Pop();
        var cname = (Token?)null;

        if(!Peek().Is(TokenType.LParen, TokenType.From)) 
        { Report(Error.Expected([TokenType.LParen, TokenType.From], Peek())); return null; }

        if(Peek().Is(TokenType.From))
        {
            _ = Pop(); // from
            if(!Peek().Is(TokenType.Id)) 
            { Report(Error.Expected([TokenType.Id], Peek())); return null; }
            cname = Pop();
        }

        if(!Peek().Is(TokenType.LParen)) 
        { Report(Error.Expected([TokenType.LParen], Peek())); return null; }

        _ = Pop();

        List<(VType? type, Token? name, bool vararg)> args = new();
        while(!Peek().Is(TokenType.RParen))
        {
            if(!Peek().Is(TokenType.Id, TokenType.Mul)) 
            { Report(Error.Expected([TokenType.Id, TokenType.Mul], Peek())); return null; }

            if(Peek().Is(TokenType.Mul))
            {
                _ = Pop();
                args.Add((null, null, true));
            }
            else
            {
                var argType = ParseType();
                if(argType is null) { return null; }

                var argName = (Token?)null;

                if(Peek().Is(TokenType.Id)) 
                { argName = Pop(); }

                args.Add((argType, argName, false));
            }

            if(Peek().Is(TokenType.RParen)) continue;
            else if(Peek().Is(TokenType.Comma)) 
            {
                _ = Pop();
                if(Peek().Is(TokenType.RParen)) { Report(Error.Expected([TokenType.Id], Peek())); return null; }
            }
            else { Report(Error.Expected([TokenType.Comma, TokenType.RParen], Peek())); return null; }
        }

        _ = Pop();

        var type = VType.Void;
        var typeT = (Token?)null;
        if(Peek().Is(TokenType.Id)) { typeT = Peek(); type = ParseType(); }

        return new CFndefNode(origin, name, cname, args, type, typeT);
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

        var type = VType.Void;
        var typeT = (Token?)null;
        if(Peek().Is(TokenType.Id)) { typeT = Peek(); type = ParseType(); }

        var block = ParseBlock(type != VType.Void, true);
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

    private static List<TokenType> StatementKeyTokens = 
        [ TokenType.Let, TokenType.LetAlloc, TokenType.Call, TokenType.Mut, 
          TokenType.If, TokenType.Ret, TokenType.Do, TokenType.While,
          TokenType.For ];
    private static List<TokenType> StatementTokens = LeafTokens.Concat(StatementKeyTokens).ToList();
    private BlockNode? ParseBlock(bool returnExpr, bool forceCurly = false)
    {
        if(forceCurly && !Peek().Is(TokenType.LCurly, TokenType.ManualBlock))
        { Report(Error.Expected([TokenType.ManualBlock, TokenType.LCurly], Peek())); return null; }
        
        bool manualBlock = false;
        if(Peek().Is(TokenType.ManualBlock)) { Pop(); manualBlock = true; }

        bool singleLine = Peek().Is(StatementTokens.ToArray());
        if(!singleLine && !Peek().Is(TokenType.LCurly))
        { Report(Error.Expected([TokenType.LCurly], Peek())); return null; }

        if(!singleLine) _ = Pop();

        List<IStatement> statements = new();

        while(!Peek().Is(TokenType.RCurly) || singleLine)
        {
            if(!Peek().Is(StatementTokens.ToArray()))
            { Report(Error.Expected(StatementTokens.ToArray(), Peek())); return null; }

            if(Peek().Is(TokenType.Let, TokenType.LetAlloc))
            {
                var let = ParseLet();
                if(let is null) { return null; }
                statements.Add(let);
            }
            else if(Peek().Is(TokenType.Call))
            {
                var call = ParseCall();
                if(call is null) { return null; }
                statements.Add(call);
            }
            else if(Peek().Is(TokenType.Mut))
            {
                var mut = ParseMut();
                if(mut is null) { return null; }
                statements.Add(mut);
            }
            else if(Peek().Is(TokenType.If))
            {
                var @if = ParseIf(returnExpr);
                if(@if is null) { return null; }
                statements.Add(@if);
            }
            else if(Peek().Is(TokenType.Ret))
            {
                var @return = ParseReturn(returnExpr);
                if(@return is null) { return null; }
                statements.Add(@return);
            }
            else if(Peek().Is(TokenType.For))
            {
                var @for = ParseFor(returnExpr);
                if(@for is null) { return null; }
                statements.Add(@for);
            }
            else if(Peek().Is(TokenType.Do, TokenType.While))
            {
                var @while = ParseWhile(returnExpr);
                if(@while is null) { return null; }
                statements.Add(@while);
            }
            else throw new System.Exception($"{Peek()}");

            if(singleLine) break;
        }

        if(!singleLine) _ = Pop();

        return new BlockNode(statements, manualBlock);
    }

    private LetNode? ParseLet()
    {
        var origin = Pop();
        var alloc = origin.Is(TokenType.LetAlloc);

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
        
        return new LetNode(origin, type, name, expr, alloc);
    }

    private CallNode? ParseCall()
    {
        var origin = Pop();

        if(!CanStartLeaf(Peek().Type))
        { Report(Error.Expected(LeafTokens, Peek())); return null; }

        var expr = ParseExpr();
        if(expr is null) { return null; }

        return new CallNode(origin, expr);
    }

    private MutNode? ParseMut()
    {
        var origin = Pop();

        if(!Peek().Is(TokenType.Id)) 
        { Report(Error.Expected([TokenType.Id], Peek())); return null; }

        // TODO: Report error?
        var name = ParseExpr();
        if(name is null || name is not Var varn) { return null; }
        if(varn.Accessors.Count > 0)
        { 
            var last = varn.Accessors.Last();
            if(last is not ArrayAcc && last is not PointerAcc && last is not MemberAcc)
            {
                Report(Error.MutDestinationAcc(origin));
                return null; 
            }
        }

        if(!Peek().Is(TokenType.Assign)) 
        { Report(Error.Expected([TokenType.Assign], Peek())); return null; }
        Pop();

        var expr = ParseExpr();
        if(expr is null) { return null; }
        
        return new MutNode(origin, varn, expr);
    }

    private ReturnNode? ParseReturn(bool returnExpr)
    {
        var origin = Pop();

        if(!returnExpr) 
            return new ReturnNode(origin);

        var expr = ParseExpr();
        if(expr is null) { return null; }

        return new ReturnNode(origin, expr);
    }

    private ForNode? ParseFor(bool returnExpr)
    {
        var origin = Pop();

        if(!Peek().Is(TokenType.Id))
        { Report(Error.Expected([TokenType.Id], Peek())); return null; }

        var itT = Pop();
        Var iterator = new Var(itT, itT.Value);

        if(!Peek().Is(TokenType.From))
        { Report(Error.Expected([TokenType.From], Peek())); return null; }
        Pop();

        if(!CanStartLeaf(Peek().Type))
        { Report(Error.Expected(LeafTokens, Peek())); return null; }

        var init = ParseExpr();
        if(init is null) { return null; }

        if(!Peek().Is(TokenType.Until))
        { Report(Error.Expected([TokenType.Until], Peek())); return null; }
        Pop();

        if(!CanStartLeaf(Peek().Type))
        { Report(Error.Expected(LeafTokens, Peek())); return null; }

        var until = ParseExpr();
        if(until is null) { return null; }

        var block = ParseBlock(returnExpr);
        if(block is null) { return null; }

        return new ForNode(origin, iterator, init, until, block);
    }


    private WhileNode? ParseWhile(bool returnExpr)
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

        var block = ParseBlock(returnExpr);
        if(block is null) return null;

        return new WhileNode(origin, expr, doo, block);
    }

    private IfNode? ParseIf(bool returnExpr)
    {
        var origin = Pop();

        var expr = ParseExpr();
        if(expr is null) { return null; }

        var block = ParseBlock(returnExpr);
        if(block is null) { return null; }

        if(!Peek().Is(TokenType.Else)) 
            return new IfNode(origin, expr, block);
        _ = Pop();

        var elseBlock = ParseBlock(returnExpr);
        if(elseBlock is null) { return null; }
        return new IfNode(origin, expr, block, elseBlock);

        // _ = Pop();
        // if(Peek().Is(TokenType.LCurly))
        // {
        //     var @elseBlock = ParseBlock();
        //     if(@elseBlock is null) { return null; }
        //     return new IfNode(origin, expr, block, @elseBlock);
        // }
        // else if(Peek().Is(TokenType.If))
        // {
        //     var elseif = ParseIf();
        //     if(elseif is null) { return null; }
        //     var @elseBlock = new BlockNode([elseif], elseif.Block.Manual);
        //     return new IfNode(origin, expr, block, @elseBlock);
        // }
    }

    private static readonly TokenType[] Binops = 
        [TokenType.Plus, TokenType.Minus, TokenType.Mul, TokenType.Div, TokenType.Mod, 
         TokenType.Eq, TokenType.Neq, 
         TokenType.Ls, TokenType.Le, TokenType.Gr, TokenType.Ge];

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


    private bool CanStartLeaf(TokenType t) => LeafTokens.Contains(t);
    private IExpr? ParseExprLeaf()
    {
        if(!CanStartLeaf(Peek().Type)) throw new System.Exception("FUUUCK");
        if(Peek().Is(TokenType.IntLit)) return new IntLit(Pop().IntValue);
        if(Peek().Is(TokenType.BoolLit)) return new BoolLit(Pop().BoolValue);
        if(Peek().Is(TokenType.StrLit)) return new StrLit(Pop().StringValue);
        if(Peek().Is(TokenType.Pointer))
        {
            var origin = Pop();
            if(!Peek().Is(TokenType.Id))
            {
                Report(Error.InvalidPointerTarget(origin));
                return null;
            }

            var expr = ParseExprLeaf() as Var;
            if(expr is null) { return null; }
            if(expr.Accessors.Count > 0 && expr.Accessors.Last() is FuncAcc)
            {
                Report(Error.InvalidPointerTarget(origin));
                return null;
            }

            return new PointerOp(origin, expr);
        }
        if(Peek().Is(TokenType.Manual))
        {
            var origin = Pop();
            if(!CanStartLeaf(Peek().Type))
            {
                Report(Error.Expected(LeafTokens, Peek()));
                return null;
            }

            var expr = ParseExprLeaf();
            if(expr is null) { return null; }

            return new ManualOp(origin, expr);
        }
        if(Peek().Is(TokenType.Not))
        {
            var origin = Pop();
            if(!CanStartLeaf(Peek().Type))
            {
                Report(Error.Expected(LeafTokens, Peek()));
                return null;
            }

            var expr = ParseExpr();
            if(expr is null) { return null; }

            return new NegateOp(origin, expr);
        }
        if(Peek().Is(TokenType.Id)) 
        {
            var corigin = Pop();

            if(Peek().Is(TokenType.Not))
            {
                _ = Pop();

                if(!Peek().Is(TokenType.LCurly))
                { Report(Error.Expected([TokenType.LCurly], Peek())); return null; }
                Pop();

                List<(string? Name, IExpr Expr)> arguments = new();
                while(!Peek().Is(TokenType.RCurly))
                {
                    if(!CanStartLeaf(Peek().Type))
                    { Report(Error.Expected(LeafTokens, Peek())); return null; }

                    var firstExpr = ParseExpr();
                    if(firstExpr is null) { return null; }

                    if(Peek().Is(TokenType.Assign))
                    {
                        Pop();

                        if(firstExpr is not Var cvarn || cvarn.Accessors.Count > 0)
                        { throw new System.Exception("todo error"); return null; }

                        if(!CanStartLeaf(Peek().Type))
                        { Report(Error.Expected(LeafTokens, Peek())); return null; }

                        var secondExpr = ParseExpr();
                        if(secondExpr is null) { return null; }

                        arguments.Add((cvarn.Name, secondExpr));
                    }
                    else { arguments.Add((null, firstExpr)); }

                    if(Peek().Is(TokenType.RCurly)) continue;

                    if(!Peek().Is(TokenType.Comma))
                    { Report(Error.Expected([TokenType.Comma], Peek())); return null; }

                    Pop();
                }

                _ = Pop();

                return new ConstructorLit(corigin, new VType(corigin.Value), arguments);
            }

            var varn = new Var(corigin, corigin.Value);
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
                        Report(Error.Expected(LeafTokens, Peek()));
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
                { Report(Error.Expected(LeafTokens, Peek())); return null; }
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
