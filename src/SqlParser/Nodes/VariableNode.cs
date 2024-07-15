namespace SqlParser.Nodes;

public record VariableNode : IAST
{
    public VariableNode(Token op)
    {
        Operator = op;
        Value = op.Value!;
    }

    public Token Operator { get; init; }
    public string Value { get; init; }

    public string GetValue()
    {
        return Value;
    }
}