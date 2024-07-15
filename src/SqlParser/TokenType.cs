namespace SqlParser;

// ReSharper disable once InconsistentNaming
public enum TokenType
{
    Integer,
    Plus,
    Minus,
    Multiply,
    Divide,
    LParen,
    RParen,
    EOF,
    Dot,
    Begin,
    End,
    Semi,
    Assign,
    Id
}

public static class TokenTypeExtensions
{
    public static string ToHumanReadableString(this TokenType tokenType)
    {
        return tokenType switch
        {
            TokenType.Integer => tokenType.ToString(),
            TokenType.Plus => "+",
            TokenType.Minus => "-",
            TokenType.Multiply => "*",
            TokenType.Divide => "/",
            TokenType.LParen => "(",
            TokenType.RParen => ")",
            TokenType.EOF => tokenType.ToString(),
            TokenType.Dot => ".",
            TokenType.Begin => "BEGIN",
            TokenType.End => "END",
            TokenType.Semi => ";",
            TokenType.Assign => ":=",
            TokenType.Id => "ID",
            _ => throw new ArgumentOutOfRangeException(nameof(tokenType), tokenType, null)
        };
    }
}