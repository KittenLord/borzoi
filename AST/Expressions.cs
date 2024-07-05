using System.Collections.Generic;
using System.Linq;

namespace Borzoi.ASTn;

public interface IExpr 
{
    public VType? Type { get; set; }
    public string ToString();
}

public class Var : IExpr
{
    public string Name;
    public string WorkingName;
    public VType? Type { get; set; }
    public Token Origin;

    public List<IAccessor> Accessors = new();

    public Var(Token origin, string name, string wname = "", VType? type = null)
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
    public VType? Type { get; set; } = VType.Int;
    public Token Value;
    public IntLit(Token value) { Value = value; }

    public override string ToString() { return $"{Value.IntValue} :: Int/Double"; }
}

public class ConstructorLit : IExpr
{
    public Token Origin;
    public VType? Type { get; set; }
    public List<(string? Name, IExpr Expr)> Arguments;

    public ConstructorLit(Token origin, VType type, List<(string? Name, IExpr Expr)> arguments)
    {
        Origin = origin;
        Type = type;
        Arguments = arguments;
    }

    public override string ToString() { return $"{{\n{string.Join("\n", Arguments.Select(arg => $"{(arg.Name ?? "?")}\n{(arg.Expr.ToString() ?? "").Indent()}")).Indent()}\n}}"; }
}

public class NullLit : IExpr
{
    public VType? Type { get; set; }
    public NullLit() {}

    public override string ToString() { return $"NULL"; }
}

public class BoolLit : IExpr
{
    public VType? Type { get; set; } = VType.Bool;
    public bool Value;
    public BoolLit(bool value) { Value = value; }

    public override string ToString() { return $"{Value} :: Bool"; }
}

public class StrLit : IExpr
{
    public VType? Type { get; set; } = VType.Byte.Modify(VTypeMod.Arr());
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

public class ConvertNode : IExpr
{
    public VType? Type { get; set; }
    public Token Origin;
    public IExpr Expr;

    public ConvertNode(Token origin, IExpr expr, VType type) { Origin = origin; Expr = expr; Type = type; }
    public override string ToString() { return $"-> {Type}\n{Expr.ToString().Indent()}"; }
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

public class MinusOp : IExpr
{
    public VType? Type { get; set; }
    public Token Origin;
    public IExpr Expr;

    public MinusOp(Token origin, IExpr expr) { Origin = origin; Expr = expr; }
    public override string ToString() { return $"-\n{Expr.ToString().Indent()}"; }
}

public class BinopNode : IExpr
{
    public VType? Type { get; set; }
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

        // NOTE: this might break stuff
        LeftType = VType.Void;
        RightType = VType.Void;
        Type = VType.Void;
    }

    public override string ToString() { return $"{Operator.Value}\n{Left.ToString().Indent()}\n{Right.ToString().Indent()}"; }
}
