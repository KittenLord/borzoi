public enum TokenType
{
    EOF,
    Illegal,


    LParen, RParen,
    LCurly, RCurly,
    LBrack, RBrack,


    Fn,
    Return,
    Let,
    Mut,
    Ret,
    If,


    Plus, Minus, Mul, Div, Mod,
    Eq, Neq, Ge, Le, Gr, Ls,
    Not, 
    Assign,
    Comma,


    Id,
    IntLit,
    StrLit,
    BoolLit, True, False
}
