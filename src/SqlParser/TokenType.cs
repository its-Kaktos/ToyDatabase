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
    Power,
    EOF
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
            TokenType.Power => "*",
            TokenType.EOF => tokenType.ToString(),
            _ => throw new ArgumentOutOfRangeException(nameof(tokenType), tokenType, null)
        };
    }
}