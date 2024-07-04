using System;
using System.Linq;
using System.Collections.Generic;

namespace Borzoi.CLI;

public class ArgValue<T>
{
    public T Value = default;
    public bool HasValue = false;

    public void Set(T value) { Value = value; HasValue = true; }
    public T Get() => Value;
    public T Get(T defaultValue) => HasValue ? Value : defaultValue;

    public ArgValue(T value = default) { Value = value; }
    public override string? ToString() { return Value?.ToString(); }
}

public class ArgumentParser
{
    public class Argument
    {
        public List<string> Aliases = new();

        public int Span = 1;

        public int Position = 0;
        public bool Tail = false;

        public object Reference;
        public Func<List<string>, object?> Parser = default;

        public Func<Stack<string>, Argument, bool> Handler = default;

        public string Description = "";
        public string AllowedValues = "";
    }

    private List<Argument> ArgumentDefinitions = new();
    public IReadOnlyList<Argument> SpecifiedArguments => ArgumentDefinitions; // please dont mutate this list uwu

    private int Position;
    private List<Argument> PositionalArgumentDefinitions = new();
    public IReadOnlyList<Argument> PositionalArguments => PositionalArgumentDefinitions;

    private bool HandleSingleArgument<T>(Stack<string> args, Argument argument)
        => HandleArgument<T>(args, argument, (reference, value) => ((ArgValue<T>)reference).Set(value));

    private bool HandleListArgument<T>(Stack<string> args, Argument argument)
        => HandleArgument<T>(args, argument, (reference, value) => ((List<T>)reference).Add(value));

    private bool HandleArgument<T>(Stack<string> args, Argument argument, Action<object, T> action)
    {
        var list = new List<string>();
        for(int i = 0; i < argument.Span; i++)
        {
            if(!args.TryPop(out var arg))
            { return false; }
            list.Add(arg);
        }

        var value = (T?)argument.Parser(list);
        if(value is null) return false;
        action(argument.Reference, value);
        return true;
    }






    public ArgValue<T> SingleArgument<T>(List<string> aliases, ArgValue<T> reference, Func<List<string>, T?> parser, int span = 1, string description = "", string allowedValues = "")
        => SpecifiedArgument(aliases, reference, HandleSingleArgument<T>, parser, span, description, allowedValues, false);
    
    public List<T> ListArgument<T>(List<string> aliases, List<T> reference, Func<List<string>, T?> parser, int span = 1, string description = "", string allowedValues = "")
        => SpecifiedArgument(aliases, reference, HandleListArgument<T>, parser, span, description, allowedValues, true);

    public ArgValue<bool> FlagArgument(List<string> aliases, ArgValue<bool> reference, bool value = true, string description = "", string allowedValues = "")
        => SpecifiedArgument(aliases, reference, HandleSingleArgument<bool>, _ => value, 0, description, allowedValues, false);

    private R SpecifiedArgument<T, R>(
        List<string> aliases, 
        R reference, 
        Func<Stack<string>, Argument, bool> handler, 
        Func<List<string>, T?> parser,
        int span,
        string description,
        string allowedValues,
        bool isList)
    {
        if(ArgumentDefinitions.Any(arg => arg.Aliases.Any(alias => aliases.Contains(alias))))
        { throw new System.Exception("Bad"); }

        ArgumentDefinitions.Add(new Argument{ 
            Tail = isList,
            Aliases = aliases,
            Span = span,
            Reference = reference,
            Handler = handler,
            Parser = list => parser(list),
            Description = description,
            AllowedValues = allowedValues
        });

        return reference;
    }


    public ArgValue<T> PositionalArgument<T>(int position, string name, ArgValue<T> reference, Func<List<string>, T?> parser, int span = 1, string description = "", string allowedValues = "")
        => BasePositionalArgument(position, name, false, reference, HandleSingleArgument<T>, parser, span, description, allowedValues);

    public List<T> TailArgument<T>(int position, string name, List<T> reference, Func<List<string>, T?> parser, int span = 1, string description = "", string allowedValues = "")
        => BasePositionalArgument(position, name, true, reference, HandleListArgument<T>, parser, span, description, allowedValues);

    private R BasePositionalArgument<T, R>(
        int position, 
        string name,
        bool tail,
        R reference, 
        Func<Stack<string>, Argument, bool> handler,
        Func<List<string>, T?> parser,
        int span,
        string description,
        string allowedValues)
    {
        if(PositionalArgumentDefinitions.Any(arg => arg.Position >= position))
        { throw new System.Exception("Bad"); }

        PositionalArgumentDefinitions.Add(new Argument{ 
            Position = position,
            Aliases = [name],
            Span = span,
            Tail = tail,
            Reference = reference,
            Handler = handler,
            Parser = (str) => parser(str),
            Description = description,
            AllowedValues = allowedValues
        });

        return reference;
    }





    public bool Parse(IEnumerable<string> argsArr)
    {
        var args = new Stack<string>(argsArr.Reverse());
        Position = 0;

        while(args.TryPop(out var arg))
        {
            bool result;
            var isPositional = !arg.StartsWith("-");

            if(isPositional)
            {
                args.Push(arg);
                var posargdef = PositionalArgumentDefinitions.Find(a => a.Position >= Position);

                if(posargdef is null)
                {
                    return false;
                }

                if(posargdef.Position > Position && !posargdef.Tail)
                {
                    return false;
                }

                Position++;

                result = posargdef.Handler(args, posargdef);

                continue;
            }

            var argdef = ArgumentDefinitions.Find(a => a.Aliases.Contains(arg));
            if(argdef is null)
            {
                return false;
            }

            result = argdef!.Handler(args, argdef);
        }

        return true;
    }
}
