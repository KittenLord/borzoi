public enum TokenType
{
    EOF,
    Illegal,


    LParen, RParen,
    LCurly, RCurly,
    LBrack, RBrack,


    Fn,
    Cfn,
    Call,
    Let, LetAlloc,
    Mut,
    Ret,
    If,
    Else,
    While,
    Do,
    From,
    For,
    Until,
    Type,
    Continue,
    Break,
    Embed,
    As,


    Link,


    Plus, Minus, Mul, Div, Mod, Modt,
    Eq, Neq, Ge, Le, Gr, Ls,
    NotFull, And, Or, Xor,
    Not, 
    Assign,
    Comma,
    Period,
    Pointer,
    Manual,
    ManualBlock,


    Id,
    IntLit,
    StrLit,
    BoolLit,
    Null,
}
