namespace SqlParser;

// ReSharper disable once InconsistentNaming
public enum TokenType
{
    Integer,
    Plus,
    Minus,
    Multiply,
    IntegerDivide,
    LParen,
    RParen,
    EOF,
    Dot,
    Begin,
    End,
    Semi,
    Assign,
    Id,
    Program,
    VarDecl,
    Real,
    RealDivide,
    Comma,
    Colon,
    IntegerConst,
    RealConst
}

public static class TokenTypeExtensions
{
    public static string ToHumanReadableString(this TokenType tokenType)
    {
        return tokenType switch
        {
            TokenType.Integer => "INTEGER",
            TokenType.Plus => "+",
            TokenType.Minus => "-",
            TokenType.Multiply => "*",
            TokenType.IntegerDivide => "/",
            TokenType.LParen => "(",
            TokenType.RParen => ")",
            TokenType.EOF => "EOF",
            TokenType.Dot => ".",
            TokenType.Begin => "BEGIN",
            TokenType.End => "END",
            TokenType.Semi => ";",
            TokenType.Assign => ":=",
            TokenType.Id => "ID",
            TokenType.Program => "PROGRAM",
            TokenType.VarDecl => "VAR_DECL",
            TokenType.Real => "REAL",
            TokenType.RealDivide => "DIV",
            TokenType.Comma => ",",
            TokenType.Colon => ":",
            TokenType.IntegerConst => "INTEGER_CONST",
            TokenType.RealConst => "REAL_CONST",
            _ => throw new ArgumentOutOfRangeException(nameof(tokenType), tokenType, null)
        };
    }
}