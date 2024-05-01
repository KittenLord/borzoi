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
        if(IntIntIntBinops.Contains(binop.Type) && left == VType.Int && right == VType.Int) 
            return VType.Int;
        if(IntIntBoolBinops.Contains(binop.Type) && left == VType.Int && right == VType.Int) 
            return VType.Bool;

        Report(Error.BinaryOperatorUndefined(binop, left, right, binop));
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

            var type = fn.RetType.Copy();
            type.Mods.Add(VTypeMod.Fn(fn.Args.Select(a => a.Type)));

            if(fn.RetTypeT is not null && !TypeExists(type.Name, out _))
            {
                Report(Error.UnknownType(type.Name, fn.RetTypeT));
                return false;
            }

            if(fn.RetType != VType.Void)
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
    
    private Identifier? GetRelevantId(Var varn, string prefix)
    {
        var stack = new Stack<string>(prefix.Split("$").Where(p => p != ""));
        while(stack.Count > 0) // > 0 <=> prefix == "funcName$"
        {
            stack.Push(varn.Name);
            var wname = string.Join("$", stack.Reverse());
            var id = Identifiers.Find(i => i.WorkingName == wname);
            if(id is not null) { varn.Type = id.Type; varn.WorkingName = wname; return id; }
            stack.Pop();
            stack.Pop();
        }
        stack.Push(varn.Name);
        var wname_ = string.Join("$", stack.Reverse());
        var id_ = Identifiers.Find(i => i.WorkingName == wname_);
        if(id_ is not null) { varn.Type = id_.Type; varn.WorkingName = wname_; return id_; }
        return null;
    }

    private VType FigureOutTheTypeOfAExpr(string prefix, IExpr expr)
    {
        if(expr is Var varn)
        {
            var type = varn.Type;
            if(type is null)
            {
                var id = GetRelevantId(varn, prefix);
                if(id is not null) type = id.Type;
            }

            if(type is null)
            {
                Report(Error.VariableDoesntExist(varn.Name, varn.Origin));
                return VType.Invalid;
            }

            type = type.Copy();
            foreach(var accessor in varn.Accessors)
            {
                if(type.Mods.Count <= 0) 
                {
                    Report(Error.CantAccess(accessor));
                    return VType.Invalid;
                }

                if(accessor is FuncAcc func)
                {
                    if(type.Mods[0] is not VFunc funcProto)
                    {
                        Report(Error.CantAccess(accessor));
                        return VType.Invalid;
                    }

                    if(funcProto.Args.Count != func.Args.Count)
                    {
                        Report(Error.FnCallArgsCount(funcProto.Args.Count, func.Args.Count));
                        return VType.Invalid;
                    }

                    for(int i = 0; i < funcProto.Args.Count; i++)
                    {
                        var argType = FigureOutTheTypeOfAExpr(prefix, func.Args[i]);
                        if(argType != funcProto.Args[i])
                        {
                            Report(Error.FnCallArgType(i, funcProto.Args[i], argType));
                            return VType.Invalid;
                        }
                    }

                    type.Mods.RemoveAt(0);
                }
                else throw new System.Exception();
            }
            return type;
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
            else if(line is MutNode mut)
            {
                mut.Var = new(mut.NameT, mut.Name);

                var id = GetRelevantId(mut.Var, prefix);
                if(id is null) return false;

                var type = FigureOutTheTypeOfAExpr(prefix, mut.Expr);
                if(id.Type != type) 
                {
                    Report(Error.TypeMismatch(id.Type, type, mut.Origin));
                    return false;
                }
            }
            else if(line is IfNode ifn)
            {
                var iprefix = GetPrefix("if");

                var exprType = FigureOutTheTypeOfAExpr(prefix, ifn.Condition);
                if(exprType != VType.Bool)
                {
                    Report(Error.TypeMismatch(VType.Bool, exprType, ifn.Origin));
                    return false;
                }

                var result = FigureOutTypesAndStuffForABlock(ifn.Block, fn, prefix + iprefix + "$");
                if(!result) return false;
                if(ifn.Else is not null)
                    result = FigureOutTypesAndStuffForABlock(ifn.Else, fn, prefix + iprefix + "$");
                if(!result) return false;
            }
            else if(line is WhileNode wh)
            {
                var iprefix = GetPrefix("while");

                var exprType = FigureOutTheTypeOfAExpr(prefix, wh.Condition);
                if(exprType != VType.Bool)
                {
                    Report(Error.TypeMismatch(VType.Bool, exprType, wh.Origin));
                    return false;
                }

                var result = FigureOutTypesAndStuffForABlock(wh.Block, fn, prefix + iprefix + "$");
                if(!result) return false;
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
                    if(!exprType.Valid) return false;
                    // FIXME: Implement actual types in fn.RetType
                    if(exprType != fn.RetType)
                    {
                        System.Console.WriteLine($"MISMATCH");
                        Report(Error.RetTypeMismatch(fn.Name, fn.RetType, exprType, ret.Origin));
                        return false;
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
        Types.Add(VType.Void);
        if(!AST.Fndefs.Any(fn => fn.Name == "main"))
        {
            Report(Error.NoEntryPoint());
            return;
        }
        var result = FigureOutTypesAndStuff();
    }
}
