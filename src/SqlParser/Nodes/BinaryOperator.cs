namespace SqlParser.Nodes;

public record BinaryOperator : IAST
{
    public BinaryOperator(IAST left, Token op, IAST right)
    {
        Left = left;
        Operator = op;
        Right = right;
    }

    public IAST Left { get; init; }
    public Token Operator { get; init; }
    public IAST Right { get; init; }

    public IEnumerable<IAST> GetChildren()
    {
        yield return Left;
        yield return Right;
    }

    public string GetValue()
    {
        return Operator.Type.ToHumanReadableString();
    }
}