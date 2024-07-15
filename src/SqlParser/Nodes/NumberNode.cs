namespace SqlParser.Nodes;

public record NumberNode : IAST
{
    public NumberNode(Token token)
    {
        Token = token;
    }

    public Token Token { get; init; }

    public string GetValue()
    {
        if (Token.Type is TokenType.Integer or TokenType.Real)
        {
            return Token.Type.ToHumanReadableString();
        }

        return Token.Value ?? "";
    }
}