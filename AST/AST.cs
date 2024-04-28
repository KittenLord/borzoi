using System.Collections.Generic;
using System.Linq;

namespace EdComp.ASTn;

public interface IContainer
{
    BlockNode GetBlock();
    List<LetNode> GetVariables();
}

public class FndefNode : IContainer
{
    public string Name => NameT.Value;
    public List<(VType Type, string Name)> Args => 
        ArgsT.Select(a => (a.Type, a.Name.Value))
             .ToList();
    public string? RetType => RetTypeT?.Value;

    public Token Origin;
    public Token NameT;
    public List<(VType Type, Token Name)> ArgsT;
    public Token? RetTypeT;
    public BlockNode Block;

    public List<string> ArgsInternal;
    public List<string> VarsInternal;

    public FndefNode(Token origin, Token name, List<(VType type, Token name)> args, Token? retType, BlockNode block)
    {
        Origin = origin;
        NameT = name;
        ArgsT = args;
        RetTypeT = retType;
        ArgsT = args;
        Block = block;

        ArgsInternal = new();
        VarsInternal = new();
    }

    public List<LetNode> GetVariables() => Block.GetVariables();
    public BlockNode GetBlock() => Block;

    public override string ToString() 
    { return $"fn {Name} :: ({string.Join(" , ", Args.Select(a => $"{a.Name} :: {a.Type}"))}) -> {(RetType ?? "e")}\n{Block}"; }
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

    public AST()
    {
        Fndefs = new();
    }

    public override string ToString() { return string.Join("\n", Fndefs); }
}
