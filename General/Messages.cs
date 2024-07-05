using System.Collections.Generic;
using System.Linq;


public class Message
{
    public string Msg;
    public Borzoi.Token Attached;

    public Message(string msg, Borzoi.Token attached)
    {
        Msg = msg;
        Attached = attached;
    }

    public override string ToString() { return $"{Msg}\nat {Attached?.ToString("")}"; }
}

namespace Borzoi.Parsing.Msg
{
    public static class Error
    {
        public static Message Expected(TokenType[] expected, Token found) =>
        new Message(
            $"Expected token of type {string.Join(" | ", expected)}. Found: {found.Type}",
            found);

        public static Message MutDestinationAcc(Token pos) =>
        new Message(
            $"Destination of mut must be a variable, an array, a member or a pointer",
            pos);

        public static Message InvalidPointerTarget(Token pos) =>
        new Message(
            $"You can only point to a place in memory, not to an evaluated expression",
            pos);
    }
}

namespace Borzoi.Lexing.Msg
{
    public static class Error
    {
        public static Message NumberLiteralTooLong(Token token) =>
        new Message(
            $"\"{token.Value}\" is too long to be any integer type",
            token);

        public static Message FloatWrongFormat(Token token) =>
        new Message(
            $"Floating point number is written in the wrong format",
            token);

        public static Message InvalidEscapeCharacter(char c, Token pos) =>
        new Message(
            $"\"\\{c}\" is not a valid escape sequence",
            pos);

        public static Message StringNotClosed(Token pos) =>
        new Message(
            $"A string must be opened and closed on the same line",
            pos);
    }
}

namespace Borzoi.Analysis.Msg
{
    // TODO: Add message positions for some errors that I was too lazy to implement
    public static class Error
    {
        public static Message NotPointerType(VType type, Token origin) =>
        new Message(
            $"Type {type} is not a pointer type",
            origin);

        public static Message CantFigureTypes(List<Borzoi.ASTn.TypedefNode> types) =>
        new Message(
            $"Cannot figure out types. There is most probably a circular reference somewhere among there types: {string.Join(", ", types.Select(t => t.Name))}",
            types.First().Origin);

        public static Message ConstructorArgumentsFormat(Token token) =>
        new Message(
            $"Constructor's arguments must either all be explicitly named, or all unnamed",
            token);

        public static Message ConstructorNotEnoughArgs(Token token) =>
        new Message(
            $"Constructor doesn't cover all the members",
            token);

        public static Message AlreadyExists(string name, Token original, Token repeat) =>
        new Message(
            $"Id {name} has been already defined at {original.ToString("")}",
            repeat);

        public static Message UnknownType(string name, Token position) =>
        new Message(
            $"Unknown type {name}",
            position);

        public static Message LetTypeMismatch(string varname, string intended, string actual, Token position) =>
        new Message(
            $"Variable {varname} is declared as type {intended}, but is assigned the value of type {actual}",
            position);

        public static Message VariableDoesntExist(string varname, Token position) =>
        new Message(
            $"Variable {varname} is not declared in this scope",
            position);

        public static Message NoReturn(string fnName, VType type, Token position) =>
        new Message(
            $"Function {fnName} must return a value of type {type} in the end",
            position);

        public static Message ReturnEmpty(bool empty, Token position) =>
        new Message(
            $"Return statement returns {(empty ? "nothing" : "a value")}, but is not supposed to",
            position);

        public static Message RetTypeMismatch(string fn, VType supposed, VType actual, Token position) =>
        new Message(
            $"Function {fn} must return {supposed}, but instead returns {actual}",
            position);

        public static Message TypeMismatch(VType supposed, VType actual, Token position) =>
        new Message(
            $"Expected expression of type {supposed}, found one of type {actual}",
            position);

        public static Message BreakContinueNotWithinLoop(Token origin) =>
        new Message(
            $"continue and break statements can only occur within a loop",
            origin);

        public static Message TypeMismatchMany(VType[] supposed, VType actual, Token position) =>
        new Message(
            $"Expected expression of one of the following types: {string.Join(", ", supposed.Select(t => t.ToString()))}, found one of type {actual}",
            position);

        public static Message BinaryOperatorUndefined(Token op, VType left, VType right, Token position) =>
        new Message(
            $"Operator {op.Value} for types {left} and {right} is not defined",
            position);

        public static Message NoEntryPoint() =>
        new Message(
            $"The program does not contain a \"main\" function",
            new Token(TokenType.Illegal, "", 0, 0, 0));

        public static Message CantAccess(VType type, Borzoi.ASTn.IAccessor access) =>
        new Message(
            $"Can't access value of type {type} using {access.Label} access",
            new Token(TokenType.Illegal, "", 0, 0, 0));

        public static Message FnCallArgsCount(int expected, int actual) =>
        new Message(
            $"Invalid function call - expected {expected} arguments, but was provided {actual}",
            new Token(TokenType.Illegal, "", 0, 0, 0));

        public static Message FnCallArgType(int pos, VType expected, VType actual) =>
        new Message(
            $"Invalid function call - argument {pos} is defined to be {expected}, but {actual} value has been provided",
            new Token(TokenType.Illegal, "", 0, 0, 0));

        public static Message DynamicToFixedArray(Token pos) =>
        new Message(
            $"Cannot use a variable-sized array as a fixed-sized one",
            pos);

        public static Message InvalidVarargPosition(Token pos) =>
        new Message(
            $"Variadic argument can only appear once at the very end",
            pos);

        // public static Message InvalidMutDest(Token pos) =>
        // new Message(
        //     $"Cannot use a variable-sized array as a fixed-sized one",
        //     pos);
    }
}
