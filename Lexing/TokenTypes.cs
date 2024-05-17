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


    Link,


    Plus, Minus, Mul, Div, Mod, Modt,
    Eq, Neq, Ge, Le, Gr, Ls,
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
