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

    public Var(Token origin, string name, string wname = null, VType type = null)
    {
        Origin = origin;
        Name = name;
        WorkingName = wname;
        Type = type;
    }

    public override string ToString() { return $"{Name} (Var)"; }
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
