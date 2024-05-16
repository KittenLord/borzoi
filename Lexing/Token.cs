using System;
using System.Linq;
using System.Collections.Generic;

namespace Borzoi;

public class Token
{
    public TokenType Type { get; private set; }

    public string Value { get; set; }

    public List<Borzoi.VType> PossibleTypes = new();
    public long IntValue; 

    public double DoubleValue; 
    public float FloatValue; 

    public bool BoolValue;
    public string StringValue;

    public int Line { get; private set; }
    public int Char { get; private set; }
    public int Length { get; private set; }

    public bool nEOF => Type != TokenType.EOF;
    public bool Is(params TokenType[] types) => types.Contains(this.Type);

    public Token(TokenType type, string value, int line, int c, int l)
    {
        Type = type;
        Value = value;
        Line = line;
        Char = c;
        Length = l;
    }

    public static Token Pos(int line, int charn)
    {
        var token = new Token(TokenType.Illegal, "", line, charn, 0);
        return token;
    }

    public override string ToString()
    {
        return $"{Type} \"{Value}\" [ {Line} : {Char} ]";
    }

    public string ToString(string s)
    {
        return $"[ {Line + 1} : {Char + 1} ]";
    }
}
