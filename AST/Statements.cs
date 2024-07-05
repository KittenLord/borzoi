using System.Collections.Generic;
using System.Linq;

namespace Borzoi.ASTn;

public interface IStatement 
{

}

public class MutNode : IStatement
{
    public Token Origin;
    public Var Var;
    public IExpr Expr;

    public MutNode(Token origin, Var varn, IExpr expr)
    {
        Origin = origin;
        Var = varn;
        Expr = expr;
    }

    public override string ToString() { return $"MUT {Var}\n{Expr.ToString()?.Indent()}"; }
}

public class IfNode : IStatement, IContainer
{
    public IExpr Condition;
    public BlockNode Block;
    public BlockNode? Else;
    public Token Origin;

    public IfNode(Token origin, IExpr condition, BlockNode block, BlockNode? elseBlock = null)
    {
        Origin = origin;
        Condition = condition;
        Block = block;
        Else = elseBlock;
    }

    public List<LetNode> GetVariables() => Block.GetVariables();
    public BlockNode GetBlock() => Block;

    public override string ToString() { return $"IF\n{Condition.ToString()!.Indent()}\n{Block.ToString().Indent()}{(Else is not null ? $"\nELSE\n{Else.ToString().Indent()}" : "")}";}
}

public class WhileNode : IStatement, IContainer
{
    public IExpr Condition;
    public bool Do;
    public BlockNode Block;
    public Token Origin;

    public WhileNode(Token origin, IExpr condition, bool doo, BlockNode block)
    {
        Origin = origin;
        Condition = condition;
        Do = doo;
        Block = block;
    }

    public List<LetNode> GetVariables() => Block.GetVariables();
    public BlockNode GetBlock() => Block;

    public override string ToString() { return $"WHILE\n{Condition.ToString()!.Indent()}\n{Block.ToString().Indent()}"; }
}

public class ForNode : IStatement, IContainer
{
    public Var Iterator;
    public IExpr Init;
    public IExpr Until;
    public BlockNode Block;
    public Token Origin;

    public ForNode(Token origin, Var iterator, IExpr init, IExpr until, BlockNode block)
    {
        Iterator = iterator;
        Origin = origin;
        Init = init;
        Until = until;
        Block = block;
    }

    public List<LetNode> GetVariables() => Block.GetVariables();
    public BlockNode GetBlock() => Block;

    public override string ToString() { return $"FOR {Iterator}\nFROM\n{Init.ToString()!.Indent()}\nUNTIL\n{Until.ToString()!.Indent()}\n{Block.ToString()}"; }
}

public class LetNode : IStatement
{
    public VType Type;
    public string Name => NameT.Value;
    public Var Var;
    public bool Alloc;

    public Token Origin;
    public Token NameT;
    public IExpr Expr;

    public LetNode(Token origin, VType type, Token name, IExpr expr, bool alloc)
    {
        Origin = origin;
        Type = type;
        NameT = name;
        Expr = expr;
        Alloc = alloc;
        // NOTE: Might break stuff idk
        Var = new Var(origin, "");
    }

    public override string ToString() { return $"LET{(Alloc ? " ALLOC" : "")} {Name} :: {Type}\n{Expr.ToString()?.Indent()}"; }
}

public class BreakNode : IStatement {
    public Token Origin;
    public BreakNode(Token origin) { Origin = origin; }
    public override string ToString() { return "BREAK"; }
}

public class ContinueNode : IStatement {
    public Token Origin;
    public ContinueNode(Token origin) { Origin = origin; }
    public override string ToString() { return "CONTINUE"; }
}

public class CallNode : IStatement
{
    public Token Origin;
    public IExpr Expr;

    public CallNode(Token origin, IExpr expr) { Origin = origin; Expr = expr; }
    public override string ToString() { return $"CALL\n{Expr.ToString().Indent()}"; }
}

public class ReturnNode : IStatement
{
    public Token Origin;

    public bool Nothing;
    public IExpr? Expr;
    public ReturnNode(Token o) { Origin = o; Nothing = true; Expr = null; }
    public ReturnNode(Token o, IExpr expr) { Origin = o; Nothing = false; Expr = expr; }

    public override string ToString() { return $"RET\n{Expr?.ToString()?.Indent()}"; }
}
