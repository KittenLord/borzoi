using System.Collections.Generic;
using System.Linq;

namespace EdComp.ASTn;

public interface IStatement 
{

}

public class MutNode : IStatement
{
    public string Name => NameT.Value;

    public Token Origin;
    public Token NameT;
    public IExpr Expr;

    public MutNode(Token origin, Token name, IExpr expr)
    {
        Origin = origin;
        NameT = name;
        Expr = expr;
    }

    public override string ToString() { return $"MUT {Name}\n{Expr.ToString()?.Indent()}"; }
}

public class IfNode : IStatement, IContainer
{
    public IExpr Condition;
    public BlockNode Block;
    public Token Origin;

    public IfNode(Token origin, IExpr condition, BlockNode block)
    {
        Origin = origin;
        Condition = condition;
        Block = block;
    }

    public List<LetNode> GetVariables() => Block.GetVariables();
    public BlockNode GetBlock() => Block;

    public override string ToString() { return $"IF\n{Condition.ToString()!.Indent()}\n{Block.ToString().Indent()}"; }
}

public class LetNode : IStatement
{
    public VType Type;
    public string Name => NameT.Value;

    public Token Origin;
    public Token NameT;
    public IExpr Expr;

    public LetNode(Token origin, VType type, Token name, IExpr expr)
    {
        Origin = origin;
        Type = type;
        NameT = name;
        Expr = expr;
    }

    public override string ToString() { return $"LET {Name} :: {Type}\n{Expr.ToString()?.Indent()}"; }
}
