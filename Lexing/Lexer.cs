using System;
using System.Linq;
using System.Collections.Generic;

namespace Borzoi.Lexing;
using Borzoi.Lexing.Msg;

public class Lexer
{
    private Stack<char> Text;
    private Queue<Token> TokenStream;
    public int Char { get; private set; }
    public int Line { get; private set; }
    private int PeekStart;

    public List<Message> Errors { get; private set; }
    public bool Success { get; private set; }

    private void Report(string error, Token position) => Report(new Message(error, position));
    private void Report(Message error)
    {
        Success = false;
        Errors.Add(error);
    }

    private bool IsNewLine(char c) => c == '\n';
    private bool IsWhitespace(char c) => " \t\r\n".Contains(c);

    private bool CanStartId(char c) => char.IsLetter(c) || "_".Contains(c);
    private bool CanBeId(char c) => char.IsLetterOrDigit(c) || "_".Contains(c);

    private bool CanStartNum(char c) => char.IsDigit(c);
    private bool CanBeNum(char c) => char.IsDigit(c);

    private Func<char, bool> Eq(params char[] cs) => (c) => cs.Any(cx => cx == c);

    private Token Put(TokenType type, char c) => Put(type, c.ToString());
    private Token Put(TokenType type, string value = "") 
    {
        var length = Char - PeekStart;
        var token = new Token(type, value, Line, PeekStart, length);
        TokenStream.Enqueue(token);
        return token;
    }

    private bool PeekPred() => Text.TryPeek(out var c);
    private bool PeekPred(string s) => Text.TryPeek(out var c) && s.Contains(c);
    private bool PeekPred(Func<char, bool> predicate) => Text.TryPeek(out var c) && predicate(c);
    private char Popc() 
    { 
        Char++; 
        var c = Text.Pop(); 
        if(IsNewLine(c)) { Line++; Char = 0; }
        return c;
    }

    public Lexer(string text)
    {
        Text = new(text.Reverse());
        TokenStream = new();
        Char = Line = PeekStart = 0;
        Errors = new();
        Success = true;
    }

    public Token Pop()
    {
        if(TokenStream.Count <= 0) Peek();
        return TokenStream.Dequeue();
    }

    public Token Peek()
    {
        if(TokenStream.Count > 0) return TokenStream.First();

        while(PeekPred(IsWhitespace)) Popc();
        if(PeekPred("#")) { do Popc(); while(!PeekPred(IsNewLine)); Popc(); return Peek(); }

        PeekStart = Char;

        if(!PeekPred()) return Put(TokenType.EOF);
        if(PeekPred(CanStartId)) return ReadIdentifier();
        if(PeekPred(CanStartNum)) return ReadNumber();
        if(PeekPred(Eq('\"'))) return ReadString();

        if(PeekPred(Eq('+'))) return Put(TokenType.Plus, Popc());
        if(PeekPred(Eq('-'))) return Put(TokenType.Minus, Popc());
        if(PeekPred(Eq('*'))) return Put(TokenType.Mul, Popc());
        if(PeekPred(Eq('/'))) return Put(TokenType.Div, Popc());
        if(PeekPred(Eq('%'))) return Put(TokenType.Mod, Popc());
        if(PeekPred(Eq(','))) return Put(TokenType.Comma, Popc());
        if(PeekPred(Eq('.'))) return Put(TokenType.Period, Popc());
        if(PeekPred(Eq('@'))) return Put(TokenType.Pointer, Popc());

        if(PeekPred(Eq('('))) return Put(TokenType.LParen, Popc());
        if(PeekPred(Eq(')'))) return Put(TokenType.RParen, Popc());
        if(PeekPred(Eq('{'))) return Put(TokenType.LCurly, Popc());
        if(PeekPred(Eq('}'))) return Put(TokenType.RCurly, Popc());
        if(PeekPred(Eq('['))) return Put(TokenType.LBrack, Popc());
        if(PeekPred(Eq(']'))) return Put(TokenType.RBrack, Popc());

        if(PeekPred(Eq('=', '<', '>', '!', '&'))) return ReadOperator();

        Popc();
        return Put(TokenType.Illegal);
    }

    private Token ReadString()
    {
        Popc(); // Opening
        bool fail = false;
        System.Text.StringBuilder sb = new();
        System.Text.StringBuilder sbtrue = new();
        while(!PeekPred("\"")) 
        {
            sbtrue.Append(Text.Peek());
            if(PeekPred("\n"))
            {
                fail = true;
                break;
            }
            if(PeekPred("\\"))
            {
                Popc();

                     if(PeekPred(Eq('"'))) { Popc(); sb.Append('"'); }
                else if(PeekPred(Eq('n'))) { Popc(); sb.Append('\n'); }
                else if(PeekPred(Eq('\\'))) { Popc(); sb.Append('\\'); }
                else if(PeekPred(Eq('t'))) { Popc(); sb.Append('\t'); }
                else if(PeekPred(Eq('0'))) { Popc(); sb.Append('\0'); }
                else { Report(Error.InvalidEscapeCharacter(Popc(), Token.Pos(Line, Char))); }
            }
            else
            {
                sb.Append(Popc());
            }
        }

        if(PeekPred("\"")) Popc();
        else fail = true;

        if(fail) return Put(TokenType.Illegal, sbtrue.ToString());
        var token = Put(TokenType.StrLit, sbtrue.ToString());
        token.StringValue = sb.ToString();
        return token;
    }

    private Token ReadOperator()
    {
        if(PeekPred(Eq('&')))
        {
            Popc();
            if(PeekPred(Eq('&')))
            { Popc(); return Put(TokenType.ManualBlock, "&&"); }
            return Put(TokenType.Manual, "&");
        }

        if(PeekPred(Eq('=')))
        {
            Popc();
            if(PeekPred(Eq('=')))
            { Popc(); return Put(TokenType.Eq, "=="); }
            return Put(TokenType.Assign, "=");
        }

        if(PeekPred(Eq('!')))
        {
            Popc();
            if(PeekPred(Eq('=')))
            { Popc(); return Put(TokenType.Neq, "!="); }
            return Put(TokenType.Not, "!");
        }

        if(PeekPred(Eq('<')))
        {
            Popc();
            if(PeekPred(Eq('=')))
            { Popc(); return Put(TokenType.Le, "<="); }
            return Put(TokenType.Ls, "<");
        }

        if(PeekPred(Eq('>')))
        {
            Popc();
            if(PeekPred(Eq('=')))
            { Popc(); return Put(TokenType.Ge, ">="); }
            return Put(TokenType.Gr, ">");
        }

        return Put(TokenType.Illegal, Popc());
    }

    private Token ReadIdentifier()
    {
        var ids = new List<char>();
        ids.Add(Popc());
        while(PeekPred(CanBeId)) ids.Add(Popc());
        var id = ids.str();

        if(id == "fn") return Put(TokenType.Fn, id);
        if(id == "cfn") return Put(TokenType.Cfn, id);
        if(id == "from") return Put(TokenType.From, id);
        if(id == "null") return Put(TokenType.Null, id);
        if(id == "link") return Put(TokenType.Link, id);
        if(id == "call") return Put(TokenType.Call, id);
        if(id == "mut") return Put(TokenType.Mut, id);
        if(id == "ret") return Put(TokenType.Ret, id);
        if(id == "if") return Put(TokenType.If, id);
        if(id == "else") return Put(TokenType.Else, id);
        if(id == "while") return Put(TokenType.While, id);
        if(id == "do") return Put(TokenType.Do, id);
        if(id == "for") return Put(TokenType.For, id);
        if(id == "until") return Put(TokenType.Until, id);
        if(id == "type") return Put(TokenType.Type, id);
        if(id == "let") 
        {
            if(PeekPred(Eq('@'))) 
                { Popc(); return Put(TokenType.LetAlloc, id + "@"); }
            return Put(TokenType.Let, id);
        }
        if(id == "true" || id == "false") 
        {
            var boolean = Put(TokenType.BoolLit, id);
            boolean.BoolValue = id == "true";
            return boolean;
        }

        return Put(TokenType.Id, id);
    }

    private Token ReadNumber()
    {
        var nums = new List<char>();
        while(PeekPred(CanBeNum)) nums.Add(Popc());

        var result = int.TryParse(nums.str(), out var i);
        var token = Put(TokenType.IntLit, nums.str());
        token.IntValue = i;
        return token;
    }
}
