public class Message
{
    public string Msg;
    public Token Attached;

    public Message(string msg, Token attached)
    {
        Msg = msg;
        Attached = attached;
    }

    public override string ToString() { return $"{Msg}\nat {Attached?.ToString("")}"; }
}

namespace EdComp.Parsing.Msg
{
    public static class Error
    {
        public static Message Expected(TokenType[] expected, Token found) =>
        new Message(
            $"Expected token of type {string.Join(" | ", expected)}. Found: {found.Type}",
            found);
    }
}

namespace EdComp.Lexing.Msg
{
    public static class Error
    {
    }
}

namespace EdComp.Analysis.Msg
{
    // TODO: Add message positions for some errors that I was too lazy to implement
    public static class Error
    {
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

        public static Message BinaryOperatorUndefined(Token op, VType left, VType right, Token position) =>
        new Message(
            $"Operator {op.Value} for types {left} and {right} is not defined",
            position);

        public static Message NoEntryPoint() =>
        new Message(
            $"The program does not contain a \"main\" function",
            new Token(TokenType.Illegal, "", 0, 0, 0));

        public static Message CantAccess(VType type, EdComp.ASTn.IAccessor access) =>
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
    }
}
