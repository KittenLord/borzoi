using System.Collections.Generic;
using System.Linq;

namespace EdComp.ASTn;

public interface IExpr 
{
    public VType Type { get; set; }
}

public class Var : IExpr
{
    public string Name;
    public string WorkingName;
    public VType Type { get; set; }
    public Token Origin;

    public List<IAccessor> Accessors = new();

    public Var(Token origin, string name, string wname = null, VType type = null)
    {
        Origin = origin;
        Name = name;
        WorkingName = wname;
        Type = type;
    }

    public override string ToString() { return $"{Name} (Var)\n{(string.Join("\n", Accessors).Indent())}"; }
}

public interface IAccessor { string Label { get; } }
public class FuncAcc : IAccessor
{
    public string Label => "function";
    public List<IExpr> Args = new();

    public FuncAcc() : this(new List<IExpr>()) {}
    public FuncAcc(IEnumerable<IExpr> args) { Args = args.ToList(); }

    public override string ToString() { return $"$\n{string.Join("\n", Args).Indent()}"; }
}

public class ArrayAcc : IAccessor
{
    public string Label => "array";

    public Token Origin;
    public IExpr Index;
    public ArrayAcc(Token origin, IExpr index) { Origin = origin; Index = index; }

    public override string ToString() { return $"[$]\n{Index.ToString().Indent()}"; }
}

public class IntLit : IExpr
{
    public VType Type { get; set; } = VType.Int;
    public int Value;
    public IntLit(int value) { Value = value; }

    public override string ToString() { return $"{Value} :: Int"; }
}

public class BoolLit : IExpr
{
    public VType Type { get; set; } = VType.Bool;
    public bool Value;
    public BoolLit(bool value) { Value = value; }

    public override string ToString() { return $"{Value} :: Bool"; }
}

public class ArrayLit : IExpr
{
    public VType? Type { get; set; }
    public List<IExpr> Elems = new();

    public override string ToString() { return $"[{Elems.Count}]\n{string.Join("\n", Elems).Indent()}"; }
}

public class ArrayInitOp : IExpr
{
    public VType? Type { get; set; }
    public Token Origin;
    public IExpr Expr;

    public ArrayInitOp(Token origin, IExpr expr) { Origin = origin; Expr = expr; }
    public override string ToString() { return $"[*]\n{Expr.ToString().Indent()}"; }
}

public class BinopNode : IExpr
{
    public VType Type { get; set; }
    public Token Operator;
    public IExpr Left;
    public IExpr Right;

    public VType LeftType;
    public VType RightType;

    public BinopNode(Token op, IExpr lhs, IExpr rhs)
    {
        Left = lhs;
        Right = rhs;
        Operator = op;
    }

    public override string ToString() { return $"{Operator.Value}\n{Left.ToString().Indent()}\n{Right.ToString().Indent()}"; }
}
