namespace SqlParser.Nodes;

public record SelectExpressionNode : IAST
{
    public SelectExpressionNode(TokenType type, string value)
    {
        Type = type;
        Value = value;
    }
    
    public TokenType Type { get; init; }
    public string Value { get; init; }
    
    public string GetValue()
    {
        return Value;
    }
}