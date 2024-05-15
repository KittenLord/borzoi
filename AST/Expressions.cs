using System.Collections.Generic;
using System.Linq;

namespace Borzoi.ASTn;

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
    public bool C = false;

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

public class MemberAcc : IAccessor
{
    public string Label => "member";

    public Token Origin;

    public string Member => MemberT.Value;
    public Token MemberT;

    public MemberAcc(Token origin, Token member)
    {
        Origin = origin;
        MemberT = member;
    }

    public override string ToString() { return $".${Member}"; }
}

public class PointerAcc : IAccessor
{
    public string Label => "pointer";

    public Token Origin;
    public PointerAcc(Token origin) { Origin = origin; }
    public override string ToString() { return $"@$"; }
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

public class StrLit : IExpr
{
    public VType Type { get; set; } = VType.Byte.Modify(VTypeMod.Arr());
    public string Value;
    public StrLit(string value) { Value = value; }

    public override string ToString() { return $"{Value} :: String"; }
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

public class PointerOp : IExpr
{
    public VType? Type { get; set; }
    public Token Origin;
    public IExpr Expr;

    public PointerOp(Token origin, IExpr expr) { Origin = origin; Expr = expr; }
    public override string ToString() { return $"@\n{Expr.ToString().Indent()}"; }
}

public class ManualOp : IExpr
{
    public VType? Type { get; set; }
    public Token Origin;
    public IExpr Expr;

    public ManualOp(Token origin, IExpr expr) { Origin = origin; Expr = expr; }
    public override string ToString() { return $"&\n{Expr.ToString().Indent()}"; }
}

public class NegateOp : IExpr
{
    public VType? Type { get; set; }
    public Token Origin;
    public IExpr Expr;

    public NegateOp(Token origin, IExpr expr) { Origin = origin; Expr = expr; }
    public override string ToString() { return $"~\n{Expr.ToString().Indent()}"; }
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
