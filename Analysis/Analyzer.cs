using System.Linq;
using System.Collections.Generic;
using Borzoi.ASTn;
using System.Collections.Generic;

using Borzoi.Analysis.Msg;
namespace Borzoi.Analysis;

public class Identifier
{
    public string UserName; // this variable name jumpscared me lol
    public string WorkingName;
    public VType Type;
    public Token Definition;

    public string? EmbedPath;
    public bool IsEmbed => EmbedPath is not null;

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

    public Dictionary<string, TypeInfo> TypeInfos;
    public List<VType> Types;

    public void RegisterType(VType vtype, TypeInfo info)
    {
        if(TypeExists(vtype.Name, out _)) 
        {
            // TODO: Err
            throw new System.Exception("AAAAAA");
            return;
        }

        Types.Add(vtype);
        TypeInfos[vtype.Name] = info;
    }

    public List<Message> Errors { get; private set; }
    public bool Success { get; private set; }
    public Analyzer(AST ast)
    {
        AST = ast;
        Errors = new();
        Identifiers = new();
        Types = new();
        TypeInfos = new();
        Success = true;
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

    private static readonly TokenType[] CompareBinops = [TokenType.Eq, TokenType.Neq, TokenType.Ls, TokenType.Le, TokenType.Gr, TokenType.Ge];
    private static readonly TokenType[] NumericBinops = [TokenType.Plus, TokenType.Minus, TokenType.Mul, TokenType.Div, TokenType.Mod, TokenType.Modt ];
    private static readonly TokenType[] FloatBinops = [ TokenType.Plus, TokenType.Minus, TokenType.Mul, TokenType.Div ];

    private VType GetBinopResult(Token binop, VType left, VType right)
    {
        if(FloatBinops.Contains(binop.Type) && left == right && (left == VType.Float || left == VType.Double))
            return left.Copy();
        if(NumericBinops.Contains(binop.Type) && left == right && (left == VType.Int || left == VType.I32 || left == VType.Byte)) 
            return left.Copy();
        if(CompareBinops.Contains(binop.Type) && left == right && (left == VType.Int || left == VType.I32 || left == VType.Byte || left == VType.Float || left == VType.Double)) 
            return VType.Bool;

        Report(Error.BinaryOperatorUndefined(binop, left, right, binop));
        return VType.Invalid;
    }

    private bool FigureOutTypesAndStuff()
    {
        foreach(var cfn in AST.CFndefs)
        {
            if(WIdExists(cfn.Name, out var def)) 
            { 
                Report(Error.AlreadyExists(cfn.Name, def, cfn.Origin));
                return false;
            }

            var type = cfn.RetType.Copy();
            var mod = VTypeMod.CFn(cfn.Args.Select(a => 
                a.vararg ? VType.VARARGS : a.Type!));
            type.Mods.Add(mod);

            if(mod.Args.Any(a => a == VType.VARARGS))
            {
                for(int i = 0; i < mod.Args.Count - 1; i++)
                {
                    if(mod.Args[i] == VType.VARARGS)
                    {
                        Report(Error.InvalidVarargPosition(cfn.Origin));
                        return false;
                    }
                }
            }

            foreach(var argt in mod.Args)
            {
                if(argt == VType.VARARGS) continue;
                if(!TypeExists(argt.Name, out _))
                {
                    Report(Error.UnknownType(argt.Name, cfn.Origin));
                    return false;
                }
            }

            if(cfn.RetTypeT is not null && !TypeExists(type.Name, out _))
            {
                Report(Error.UnknownType(type.Name, cfn.RetTypeT));
                return false;
            }

            Identifiers.Add(new Identifier(cfn.Name, cfn.Name, type.Copy(), cfn.Origin));
        }

        foreach(var fn in AST.Fndefs)
        {
            if(WIdExists(fn.Name, out var def)) 
            { 
                Report(Error.AlreadyExists(fn.Name, def, fn.Origin));
                return false;
            }

            var type = fn.RetType.Copy();
            var mod = VTypeMod.Fn(fn.Args.Select(a => a.Type));
            type.Mods.Add(mod);

            foreach(var argt in mod.Args)
            {
                if(!TypeExists(argt.Name, out _))
                {
                    Report(Error.UnknownType(argt.Name, fn.Origin));
                    return false;
                }
            }

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

            Identifiers.Add(new Identifier(fn.Name, fn.Name, type.Copy(), fn.Origin));
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

                var id = new Identifier(arg.Name, wname, arg.Type.Copy(), fn.Origin);
                fn.ArgsInternal.Add(new StackVar(arg.Type, wname, -1));
                Identifiers.Add(id);
            }

            var result = FigureOutTypesAndStuffForABlock(fn.Block, fn, fn.Name + "$", false);
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
            // System.Console.WriteLine($"{varn.Name}");
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

    private VType FigureOutTheTypeOfAExpr(string prefix, IExpr expr, VType hint)
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
                // if(type.Mods.Count <= 0) 
                // {
                //     Report(Error.CantAccess(type, accessor));
                //     return VType.Invalid;
                // }

                if(accessor is FuncAcc func)
                {
                    if(!type.Is<VFunc>(out var funcProto))
                    {
                        Report(Error.CantAccess(type, accessor));
                        return VType.Invalid;
                    }

                    var isVarArg = funcProto.Args.Count > 0 &&
                                   funcProto.Args.Last() == VType.VARARGS;
                    if(funcProto.Args.Count != func.Args.Count && !isVarArg)
                    {
                        Report(Error.FnCallArgsCount(funcProto.Args.Count, func.Args.Count));
                        return VType.Invalid;
                    }

                    if(isVarArg && func.Args.Count - funcProto.Args.Count < -1)
                    {
                        Report(Error.FnCallArgsCount(funcProto.Args.Count, func.Args.Count));
                        return VType.Invalid;
                    }

                    for(int i = 0; i < func.Args.Count; i++)
                    {
                        var argHint = i >= funcProto.Args.Count ? VType.Invalid : funcProto.Args[i];
                        var argType = FigureOutTheTypeOfAExpr(prefix, func.Args[i], argHint);
                        if(i >= funcProto.Args.Count) continue;
                        if(i >= funcProto.Args.Count - 1 && 
                           funcProto.Args.Count > 0 && 
                           funcProto.Args.Last() == VType.VARARGS) 
                            continue;
                        if(argType != funcProto.Args[i])
                        {
                            Report(Error.FnCallArgType(i, funcProto.Args[i], argType));
                            return VType.Invalid;
                        }
                    }

                    type.RemoveLastMod();
                }
                else if(accessor is ArrayAcc arrAcc)
                {
                    if(!type.Is<VArray>(out var arrProto))
                    {
                        Report(Error.CantAccess(type, accessor));
                        return VType.Invalid;
                    }

                    var indexType = FigureOutTheTypeOfAExpr(prefix, arrAcc.Index, VType.Int);
                    if(indexType != VType.Int)
                    {
                        Report(Error.TypeMismatch(VType.Int, indexType, arrAcc.Origin));
                        return VType.Invalid;
                    }

                    type.RemoveLastMod();
                }
                else if(accessor is PointerAcc ptrAcc)
                {
                    if(!type.Is<VPointer>()) 
                    {
                        Report(Error.CantAccess(type, accessor));
                        return VType.Invalid;
                    }

                    type.RemoveLastMod();
                }
                else if(accessor is MemberAcc mAcc)
                {
                    var info = type.GetInfo(TypeInfos);
                    var member = info.Members.Find(m => m.Name == mAcc.Member);

                    if(!info.Members.Any(m => m.Name == mAcc.Member))
                    {
                        Report(Error.CantAccess(type, accessor));
                        return VType.Invalid;
                    }

                    type = member.Type;
                }
                else throw new System.Exception();
            }

            varn.Type = type;
            return type;
        }
        if(expr is IntLit il) 
        {
            var types = il.Value.PossibleTypes;
            if(types.Count <= 0) return VType.Invalid;

            var type = types.Find(t => t == hint);
            if(type is not null)
            {
                il.Type = hint;
                return hint;
            }

            il.Type = types.First();
            return types.First();
        }
        if(expr is BoolLit) return VType.Bool;
        if(expr is StrLit) return VType.Byte.Modify(VTypeMod.Arr());
        if(expr is NegateOp negop)
        {
            var type = FigureOutTheTypeOfAExpr(prefix, negop.Expr, hint);
            if(type != VType.Int && type != VType.Bool)
            {
                Report(Error.TypeMismatchMany([VType.Int, VType.Bool], type, negop.Origin));
                return VType.Invalid;
            }

            negop.Type = type;
            return type;
        }
        if(expr is MinusOp minus)
        {
            var type = FigureOutTheTypeOfAExpr(prefix, minus.Expr, hint);
            if(type != VType.Int && type != VType.I32 && type != VType.Byte)
            {
                Report(Error.TypeMismatchMany([VType.Int, VType.I32, VType.Byte], type, minus.Origin));
                return VType.Invalid;
            }

            minus.Type = type;
            return type;
        }
        if(expr is ManualOp manop)
        {
            var type = FigureOutTheTypeOfAExpr(prefix, manop.Expr, hint);
            if(!type.Is<VPointer>() && !type.Is<VArray>())
            {
                // FIXME: Error
                Report(Error.TypeMismatchMany([VType.Int, VType.Bool], type, manop.Origin));
                return VType.Invalid;
            }

            manop.Type = type;
            return type;
        }
        if(expr is ArrayLit arr)
        {
            // Infer type of an empty array
            if(arr.Elems.Count <= 0 && 
               hint != VType.Invalid &&
               hint.Is<VArray>())
            {
                arr.Type = hint.Copy();
                return arr.Type;
            }

            // Not enough information to infer the type from
            // TODO: I don't remember what this means, seems error prone
            if(arr.Elems.Count <= 0)
            { 
                if(!hint.Valid)
                {
                    return VType.Invalid; 
                }

                hint = hint.Copy();
                hint.Mods.Add(VTypeMod.Arr());
                arr.Type = hint;
                return hint;
            }

            var elemHint = VType.Invalid;
            if(hint.Is<VArray>())
            {
                elemHint = hint.Copy();
                elemHint.RemoveLastMod();
            }

            var types = arr.Elems.Select(elem => FigureOutTheTypeOfAExpr(prefix, elem, elemHint));
            var type = types.First().Copy();
            var same = types.All(t => t == type);

            if(!same)
            {
                // TODO: Report your errors you lazy fuck
                return VType.Invalid;
            }

            type.Mods.Add(VTypeMod.Arr(arr.Elems.Count));
            arr.Type = type;
            return type;
        }
        if(expr is ArrayInitOp arri)
        {
            if(hint == VType.Invalid) return VType.Invalid;
            if(hint.Mods.Count == 0) return VType.Invalid;
            if(hint.Mods.Last() is not VArray arrHint) return VType.Invalid;
            if(arrHint.Fixed)
                { Report(Error.DynamicToFixedArray(arri.Origin)); return VType.Invalid; }

            var sizeExprType = FigureOutTheTypeOfAExpr(prefix, arri.Expr, VType.Int);
            if(sizeExprType != VType.Int)
            {
                Report(Error.TypeMismatch(VType.Int, sizeExprType, arri.Origin));
                return VType.Invalid;
            }

            var type = hint.Copy();
            arri.Type = type;
            return type;
        }
        if(expr is PointerOp ptrop)
        {
            var innerHint = hint?.Copy() ?? VType.Invalid;
            if(innerHint.Is<VPointer>()) innerHint.RemoveLastMod();

            var exprType = FigureOutTheTypeOfAExpr(prefix, ptrop.Expr, innerHint).Copy();
            exprType.Mods.Add(VTypeMod.Pointer());

            ptrop.Type = exprType;

            return exprType;
        }
        if(expr is BinopNode binop) 
        {
            if(hint == VType.Bool) hint = VType.Invalid;
            var leftType = FigureOutTheTypeOfAExpr(prefix, binop.Left, hint);
            if(hint == VType.Invalid) hint = leftType.Copy();
            var rightType = FigureOutTheTypeOfAExpr(prefix, binop.Right, hint);
            var result = GetBinopResult(binop.Operator, leftType, rightType);
            binop.Type = result;
            binop.LeftType = leftType;
            binop.RightType = rightType;
            binop.Left.Type = leftType;
            binop.Right.Type = rightType;
            return result;
        }
        if(expr is ConstructorLit ctor)
        {
            if(!TypeExists(ctor.Type.Name, out var type))
            {
                Report(Error.UnknownType(ctor.Type.Name, ctor.Origin));
                return VType.Invalid;
            }

            var info = type.GetInfo(TypeInfos);

            var sameFormat = ctor.Arguments.Count <= 0 ||
                ctor.Arguments.All(arg => 
                    (arg.Name is null) == 
                    (ctor.Arguments.First().Name is null));

            if(!sameFormat)
            { Report(Error.ConstructorArgumentsFormat(ctor.Origin)); return VType.Invalid; }

            var explicitNames = ctor.Arguments.Count > 0 && ctor.Arguments.First().Name is not null;

            if(!explicitNames)
            {
                if(ctor.Arguments.Count != info.Members.Count)
                { Report(Error.ConstructorNotEnoughArgs(ctor.Origin)); return VType.Invalid; }

                for(int i = 0; i < ctor.Arguments.Count; i++)
                {
                    var ctorExpr = ctor.Arguments[i].Expr;
                    var mType = info.Members[i].Type;

                    var ctorExprType = FigureOutTheTypeOfAExpr(prefix, ctorExpr, mType);
                    if(ctorExprType != mType)
                    { Report(Error.TypeMismatch(mType, ctorExprType, ctor.Origin)); return VType.Invalid; }
                }

                return ctor.Type;
            }

            if(!ctor.Arguments.All(arg => info.Members.Any(m => m.Name == arg.Name)))
            { Report(Error.ConstructorNotEnoughArgs(ctor.Origin)); return VType.Invalid; }

            foreach(var arg in ctor.Arguments)
            {
                var member = info.Members.Find(m => m.Name == arg.Name);
                var argType = FigureOutTheTypeOfAExpr(prefix, arg.Expr, member.Type);

                if(argType != member.Type)
                { Report(Error.TypeMismatch(member.Type, argType, ctor.Origin)); return VType.Invalid; }
            }

            var newList = new List<(string? Name, IExpr Expr)>();            
            foreach(var member in info.Members)
            {
                var arg = ctor.Arguments.Find(a => a.Name == member.Name);
                newList.Add(arg);
            }
            ctor.Arguments = newList;

            return ctor.Type;
        }
        throw new System.Exception("dunno");
        return VType.Invalid;
    }

    private bool FigureOutTypesAndStuffForABlock(IContainer container, FndefNode fn, string prefix, bool withinLoop)
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

                var exprType = FigureOutTheTypeOfAExpr(prefix, let.Expr, let.Type);

                if(exprType != let.Type)
                {
                    Report(Error.LetTypeMismatch(let.Name, let.Type.ToString(), exprType.ToString(), let.Origin));
                    return false;
                }

                var letType = let.Type.Copy();
                if(let.Alloc) letType.Modify(VTypeMod.Pointer());

                let.Var = new Var(let.Origin, let.Name, wname, letType);
                var id = new Identifier(let.Name, wname, letType, let.Origin);
                fn.VarsInternal.Add(new StackVar(letType, wname, -1));
                Identifiers.Add(id);
            }
            else if(line is CallNode call)
            {
                FigureOutTheTypeOfAExpr(prefix, call.Expr, VType.Invalid);
            }
            else if(line is MutNode mut)
            {
                // var id = GetRelevantId(mut.Var, prefix);
                // if(id is null) return false;

                var destType = FigureOutTheTypeOfAExpr(prefix, mut.Var, VType.Invalid);
                var type = FigureOutTheTypeOfAExpr(prefix, mut.Expr, destType);
                mut.Expr.Type = type;

                if(destType != type) 
                {
                    Report(Error.TypeMismatch(destType, type, mut.Origin));
                    return false;
                }
            }
            else if(line is BreakNode || line is ContinueNode)
            {
                if(!withinLoop)
                {
                    return false;
                }

            }
            else if(line is IfNode ifn)
            {
                var iprefix = GetPrefix("if");

                var exprType = FigureOutTheTypeOfAExpr(prefix, ifn.Condition, VType.Bool);
                if(exprType != VType.Bool)
                {
                    Report(Error.TypeMismatch(VType.Bool, exprType, ifn.Origin));
                    return false;
                }

                var result = FigureOutTypesAndStuffForABlock(ifn.Block, fn, prefix + iprefix + "$", withinLoop);
                if(!result) return false;
                if(ifn.Else is not null)
                    result = FigureOutTypesAndStuffForABlock(ifn.Else, fn, prefix + iprefix + "$", withinLoop);
                if(!result) return false;
            }
            else if(line is WhileNode wh)
            {
                var iprefix = GetPrefix("while");

                var exprType = FigureOutTheTypeOfAExpr(prefix, wh.Condition, VType.Bool);
                if(exprType != VType.Bool)
                {
                    Report(Error.TypeMismatch(VType.Bool, exprType, wh.Origin));
                    return false;
                }

                var result = FigureOutTypesAndStuffForABlock(wh.Block, fn, prefix + iprefix + "$", true);
                if(!result) return false;
            }
            else if(line is ForNode forn)
            {
                var iprefix = GetPrefix("for");

                var initType = FigureOutTheTypeOfAExpr(prefix, forn.Init, VType.Int);
                var untilType = FigureOutTheTypeOfAExpr(prefix, forn.Until, VType.Int);

                if(initType != VType.Int)
                {
                    Report(Error.TypeMismatch(VType.Int, initType, forn.Origin));
                    return false;
                }

                if(untilType != VType.Int)
                {
                    Report(Error.TypeMismatch(VType.Int, untilType, forn.Origin));
                    return false;
                }

                var wname = prefix + iprefix + "$" + forn.Iterator.Name;
                forn.Iterator.WorkingName = wname;
                forn.Iterator.Type = VType.Int;
                var id = new Identifier(forn.Iterator.Name, wname, VType.Int, forn.Iterator.Origin);
                fn.VarsInternal.Add(new StackVar(VType.Int, wname, -1));
                Identifiers.Add(id);

                var result = FigureOutTypesAndStuffForABlock(forn.Block, fn, prefix + iprefix + "$", true);
                if(!result) return false;
            }
            else if(line is ReturnNode ret)
            {
                if(ret.Nothing != (fn.RetType == VType.Void)) 
                {
                    if(ret.Nothing) Report(Error.ReturnEmpty(true, ret.Origin));
                    else Report(Error.ReturnEmpty(false, ret.Origin));
                    return false;
                }

                if(!ret.Nothing)
                {
                    var exprType = FigureOutTheTypeOfAExpr(prefix, ret.Expr, fn.RetType);
                    if(!exprType.Valid) return false;
                    if(exprType != fn.RetType)
                    {
                        // System.Console.WriteLine($"MISMATCH");
                        Report(Error.RetTypeMismatch(fn.Name, fn.RetType, exprType, ret.Origin));
                        return false;
                    }
                }
            }
            else throw new System.Exception("aboba");
        }

        return true;
    }

    public void Analyze()
    {
        RegisterType(VType.Int, new(8));
        RegisterType(VType.I32, new(4));
        RegisterType(VType.Byte, new(1));

        RegisterType(VType.Double, new(8));
        RegisterType(VType.Float, new(4));

        RegisterType(VType.Bool, new(1));

        RegisterType(VType.Void, new(0));

        if(!AST.Fndefs.Any(fn => fn.Name == "main"))
        {
            Report(Error.NoEntryPoint());
            return;
        }

        List<TypedefNode> workList = new(AST.TypeDefs);
        int previousAmount = workList.Count;

        while(workList.Count > 0)
        {
            List<TypedefNode> figuredOut = new();
            foreach(var typedef in workList)
            {
                if(!typedef.Members.All(member => TypeExists(member.Type.Name, out _)))
                { continue; }

                var alignment = typedef.Members.Max(m => m.Type.GetInfo(TypeInfos).Alignment);

                var vtype = new VType(typedef.Name);

                var typeinfo = new TypeInfo(0, alignment);
                
                int offset = 0;
                foreach(var member in typedef.Members)
                {
                    var minfo = member.Type.GetInfo(TypeInfos);

                    offset = offset.Pad(minfo.Alignment);
                    typeinfo.Members.Add((member.Name, member.Type, offset));
                    offset += minfo.ByteSize;
                }
                offset = offset.Pad(alignment);
                typeinfo.ByteSize = offset;

                figuredOut.Add(typedef);
                RegisterType(vtype, typeinfo);
            }

            workList.RemoveAll(w => figuredOut.Contains(w));

            // Can't figure out types
            if(figuredOut.Count <= 0)
            {
                Report(Error.CantFigureTypes(workList));
                return;
            }
        }

        foreach(var type in Types)
        {
            var info = type.GetInfo(TypeInfos);
            // System.Console.WriteLine($"{type}");
            // System.Console.WriteLine($"{info}");
            // System.Console.WriteLine($"");
        }

        foreach(var embed in AST.Embeds)
        {
            var id = new Identifier(embed.Id.Name, embed.Id.Name, VType.Byte.Modify(VTypeMod.Arr()), embed.Origin);
            id.EmbedPath = embed.Path;
            Identifiers.Add(id);
        }

        var result = FigureOutTypesAndStuff();
        AST.TypeInfos = TypeInfos;
        AST.Identifiers = Identifiers;
    }
}
