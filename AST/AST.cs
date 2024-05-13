using System.Collections.Generic;
using System.Linq;

namespace EdComp.ASTn;

public interface IContainer
{
    BlockNode GetBlock();
    List<LetNode> GetVariables();
}

public class StackVar {
    public VType Type;
    public string WorkingName;
    public int Offset;

    public StackVar(VType type, string workingName, int offset)
    {
        Type = type;
        WorkingName = workingName;
        Offset = offset;
    }
}

public class CFndefNode
{
    public string Name => NameT.Value;
    public string CName => CNameT?.Value ?? Name;

    public List<(VType? Type, string? Name, bool vararg)> Args => 
        ArgsT.Select(a => (a.Type, a.Name?.Value, a.vararg))
             .ToList();

    public Token Origin;
    public Token NameT;
    public Token? CNameT;
    public List<(VType? Type, Token? Name, bool vararg)> ArgsT;
    public VType RetType;
    public Token? RetTypeT;

    public CFndefNode(Token origin, Token name, Token? cname, List<(VType? type, Token? name, bool vararg)> args, VType retType, Token? retTypeT)
    {
        Origin = origin;
        NameT = name;
        CNameT = cname;
        ArgsT = args;
        RetType = retType;
        RetTypeT = retTypeT;
        ArgsT = args;
    }

    public override string ToString() 
    { return $"cfn {Name}{(CNameT is not null ? $" ({CName})" : "")} :: ({string.Join(" , ", Args.Select(a => a.vararg ? "*" : $"{(a.Name ?? "---")} :: {a.Type}"))}) -> {(RetType)}\n"; }
}

public class FndefNode : IContainer
{
    public string Name => NameT.Value;
    public List<(VType Type, string Name)> Args => 
        ArgsT.Select(a => (a.Type, a.Name.Value))
             .ToList();

    public Token Origin;
    public Token NameT;
    public List<(VType Type, Token Name)> ArgsT;
    public VType RetType;
    public Token? RetTypeT;
    public BlockNode Block;

    public int StackSize;

    public int ArgsSize;
    public int ArgsPadSize;

    public bool IsArg(string wname, out StackVar svar)
    {
        var argf = ArgsInternal.Find(v => v.WorkingName == wname);
        if(argf is not null) { svar = argf; return true; }
        svar = VarsInternal.Find(v => v.WorkingName == wname)!;
        return false;
    }

    public StackVar GetVar(string wname) { return VarsInternal.Find(v => v.WorkingName == wname)!; }

    public List<StackVar> ArgsInternal;
    public List<StackVar> VarsInternal;

    public FndefNode(Token origin, Token name, List<(VType type, Token name)> args, VType retType, Token? retTypeT, BlockNode block)
    {
        Origin = origin;
        NameT = name;
        ArgsT = args;
        RetType = retType;
        RetTypeT = retTypeT;
        ArgsT = args;
        Block = block;

        ArgsInternal = new();
        VarsInternal = new();
    }

    public List<LetNode> GetVariables() => Block.GetVariables();
    public BlockNode GetBlock() => Block;

    public override string ToString() 
    { return $"fn {Name} :: ({string.Join(" , ", Args.Select(a => $"{a.Name} :: {a.Type}"))}) -> {(RetType)}\n{Block}"; }
}

public class BlockNode : IContainer
{
    public List<IStatement> Statements;

    public BlockNode(List<IStatement> statements)
    {
        Statements = statements;
    }

    public override string ToString() { return string.Join("\n", Statements); }
    public BlockNode GetBlock() => this;

    public List<LetNode> GetVariables() 
    { 
        var list = new List<LetNode>();
        list.AddRange(Statements.Where(s => s is LetNode).Select(s => (LetNode)s));
        list.AddRange(Statements.Where(s => s is IContainer).SelectMany(s => ((IContainer)s).GetVariables()));
        return list;
    }
}

public class AST
{
    public List<FndefNode> Fndefs;
    public List<CFndefNode> CFndefs;
    public List<string> Links;

    public Dictionary<string, TypeInfo> TypeInfos;

    public AST()
    {
        Fndefs = new();
        CFndefs = new();
        TypeInfos = new();
        Links = new();
    }

    public override string ToString() { return 
        $"{string.Join("\n", Fndefs)}\n\n{string.Join("\n", CFndefs)}\n\nlinks: [ {string.Join(", ", Links.Select(l => $"\"{l}\""))} ]"; }
}
