namespace SqlParser;

public record NumberNode : IAST
{
    public NumberNode(Token token)
    {
        Token = token;
    }
    
    public Token Token { get; init; }

    public string GetValue()
    {
        return Token.ValueAsInt!.Value.ToString();
    }
}