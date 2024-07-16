namespace SqlParser;

public record Token
{
    public Token(TokenType type)
    {
        Type = type;
    }

    public Token(TokenType type, string? value)
    {
        Type = type;
        Value = value;
    }

    public TokenType Type { get; init; }
    public string? Value { get; set; }
}