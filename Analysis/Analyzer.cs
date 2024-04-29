using System.Linq;
using System.Collections.Generic;
using EdComp.ASTn;
using System.Collections.Generic;

using EdComp.Analysis.Msg;
namespace EdComp.Analysis;

public class Identifier
{
    public string UserName;
    public string WorkingName;
    public VType Type;
    public Token Definition;

    public Identifier(string un, string wn, VType type, Token def)
    {
        UserName = un;
        WorkingName = wn;
        Type = type;
        Definition = def;
    }
}

public class Analyzer
{
    public AST AST;
    public List<Identifier> Identifiers;
    public List<VType> Types;

    public List<Message> Errors { get; private set; }
    public bool Success { get; private set; }
    public Analyzer(AST ast)
    {
        AST = ast;
        Errors = new();
        Identifiers = new();
        Types = new();
    }

    private void Report(string error, Token position) => Report(new Message(error, position));
    private void Report(Message error)
    {
        Success = false;
        Errors.Add(error);
    }

    private bool TypeExists(string name, out VType type)
    {
        type = Types.Find(t => t.Name == name);
        return type is not null;
    }

    private bool WIdExists(string wn, out Token original)
    {
        original = Identifiers.Find(i => i.WorkingName == wn)?.Definition;
        return original is not null;
    }

    private static readonly TokenType[] IntIntBoolBinops = [TokenType.Eq, TokenType.Neq, TokenType.Ls, TokenType.Le, TokenType.Gr, TokenType.Gr];
    private static readonly TokenType[] IntIntIntBinops = [TokenType.Plus, TokenType.Minus, TokenType.Mul, TokenType.Div, TokenType.Mod];

    private VType GetBinopResult(Token binop, VType left, VType right)
    {
        if(left.Mods.Count != 0 || right.Mods.Count != 0) return VType.Invalid;

        if(IntIntIntBinops.Contains(binop.Type) && left == VType.Int && right == VType.Int) 
            return VType.Int;
        if(IntIntBoolBinops.Contains(binop.Type) && left == VType.Int && right == VType.Int) 
            return VType.Bool;

        return VType.Invalid;
    }

    private bool FigureOutTypesAndStuff()
    {
        foreach(var fn in AST.Fndefs)
        {
            if(WIdExists(fn.Name, out var def)) 
            { 
                Report(Error.AlreadyExists(fn.Name, def, fn.Origin));
                return false;
            }

            var type = fn.RetType is null ? new VType() : new VType(fn.RetType, VTypeMod.Fn);

            if(fn.RetType is not null && !TypeExists(type.Name, out _))
            {
                Report(Error.UnknownType(type.Name, fn.RetTypeT));
                return false;
            }

            if(fn.RetType is not null)
            {
                var last = fn.Block.Statements.LastOrDefault();
                if(last is null || last is not ReturnNode)
                {
                    Report(Error.NoReturn(fn.Name, fn.RetType, fn.Origin));
                    return false;
                }
            }

            Identifiers.Add(new Identifier(fn.Name, fn.Name, type, fn.Origin));
        }

        foreach(var fn in AST.Fndefs)
        {
            foreach(var arg in fn.Args)
            {
                var wname = fn.Name + "$" + arg.Name;
                if(WIdExists(wname, out var def)) 
                {
                    Report(Error.AlreadyExists(arg.Name, def, fn.Origin));
                    return false;
                }

                if(!TypeExists(arg.Type.Name, out _))
                {
                    Report(Error.UnknownType(arg.Type.Name, fn.Origin));
                    return false;
                }

                // TODO: Arg origin?

                var id = new Identifier(arg.Name, wname, arg.Type, fn.Origin);
                fn.ArgsInternal.Add(wname);
                Identifiers.Add(id);
            }

            var result = FigureOutTypesAndStuffForABlock(fn.Block, fn, fn.Name + "$");
            if(!result) return false;
        }

        return true;
    }

    private VType FigureOutTheTypeOfAExpr(string prefix, IExpr expr)
    {
        if(expr is Var varn)
        {
            if(varn.Type is not null) return varn.Type;
            var stack = new Stack<string>(prefix.Split("$").Where(p => p != ""));
            while(stack.Count > 0) // > 0 <=> prefix == "funcName$"
            {
                stack.Push(varn.Name);
                var wname = string.Join("$", stack.Reverse());
                var id = Identifiers.Find(i => i.WorkingName == wname);
                if(id is not null) { varn.Type = id.Type; varn.WorkingName = wname; return id.Type; }
                stack.Pop();
                stack.Pop();
            }

            Report(Error.VariableDoesntExist(varn.Name, varn.Origin));

            return VType.Invalid;
        }
        if(expr is IntLit) return VType.Int;
        if(expr is BoolLit) return VType.Bool;
        if(expr is BinopNode binop) 
        {
            var leftType = FigureOutTheTypeOfAExpr(prefix, binop.Left);
            var rightType = FigureOutTheTypeOfAExpr(prefix, binop.Right);
            var result = GetBinopResult(binop.Operator, leftType, rightType);
            binop.Type = result;
            binop.LeftType = leftType;
            binop.RightType = rightType;
            return result;
        }
        return VType.Invalid;
    }

    private bool FigureOutTypesAndStuffForABlock(IContainer container, FndefNode fn, string prefix)
    {
        var block = container.GetBlock();
        var prefixes = new Dictionary<string, int>();

        string GetPrefix(string p) 
        {
            if(!prefixes.ContainsKey(p)) prefixes[p] = -1;
            prefixes[p]++;
            return p + prefixes[p];
        }

        foreach(var line in block.Statements)
        {
            if(line is IContainer c)
            {
                var iprefix = "";
                if(c is IfNode) iprefix = "if";
                else throw new System.Exception("cocaine");
                iprefix = GetPrefix(iprefix);
                var result = FigureOutTypesAndStuffForABlock(c, fn, prefix + iprefix + "$");
                if(!result) return false;
            }
            else
            {
                if(line is LetNode let)
                {
                    var wname = prefix + let.Name;
                    if(WIdExists(wname, out var def)) 
                    {
                        Report(Error.AlreadyExists(let.Name, def, let.Origin));
                        return false;
                    }

                    if(!TypeExists(let.Type.Name, out _))
                    {
                        Report(Error.UnknownType(let.Type.Name, let.Origin));
                        return false;
                    }

                    var exprType = FigureOutTheTypeOfAExpr(prefix, let.Expr);
                    // FIXME: If exprType == VType.Invalid, report that the operator for those types isn't defined
                    if(exprType != let.Type)
                    {
                        Report(Error.LetTypeMismatch(let.Name, let.Type.ToString(), exprType.ToString(), let.Origin));
                        return false;
                    }

                    let.Var = new Var(let.Origin, let.Name, wname, let.Type);
                    var id = new Identifier(let.Name, wname, let.Type, let.Origin);
                    fn.VarsInternal.Add(wname);
                    Identifiers.Add(id);
                }
                else if(line is ReturnNode ret)
                {
                    if(ret.Nothing != fn.RetType is null) 
                    {
                        if(ret.Nothing) Report(Error.ReturnEmpty(true, ret.Origin));
                        else Report(Error.ReturnEmpty(false, ret.Origin));
                        return false;
                    }

                    if(!ret.Nothing)
                    {
                        var exprType = FigureOutTheTypeOfAExpr(prefix, ret.Expr);
                        if(!exprType.Valid) throw new System.Exception("gg");
                        // FIXME: Implement actual types in fn.RetType
                        if(exprType.Name != fn.RetType)
                        {
                            System.Console.WriteLine($"MISMATCH");
                            Report(Error.RetTypeMismatch(fn.Name, new VType(fn.RetType), exprType, ret.Origin));
                            return false;
                        }
                    }
                }
            }
        }

        return true;
    }

    public void Analyze()
    {
        Types.Add(VType.Int);
        Types.Add(VType.Bool);
        var result = FigureOutTypesAndStuff();
    }
}
