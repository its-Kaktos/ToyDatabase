namespace SqlParser.Nodes;

public record UnaryOperator : IAST
{
    public UnaryOperator(Token op, IAST child)
    {
        Child = child;
        Operator = op;
    }

    public IAST Child { get; init; }
    public Token Operator { get; init; }

    public IEnumerable<IAST> GetChildren()
    {
        yield return Child;
    }

    public string GetValue()
    {
        return Operator.Type.ToHumanReadableString();
    }
}