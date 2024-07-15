namespace SqlParser.Nodes;

public record VariableNode : IAST
{
    public VariableNode(Token op)
    {
        Operator = op;
        // Variables in pascal are case-insensitive
        // so make them lower for easier clash checking.
        Value = op.Value!.ToLower();
    }

    public Token Operator { get; init; }
    public string Value { get; init; }

    public string GetValue()
    {
        return Value;
    }
}