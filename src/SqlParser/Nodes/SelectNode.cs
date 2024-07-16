namespace SqlParser.Nodes;

public record SelectNode : IAST
{
    public SelectNode(TokenType type, SelectExpressionNode selectExpression, TableExpressionNode tableExpression)
    {
        Type = type;
        SelectExpression = selectExpression;
        TableExpression = tableExpression;
    }

    public TokenType Type { get; set; }
    public SelectExpressionNode SelectExpression { get; init; }
    public TableExpressionNode TableExpression { get; init; }

    public IEnumerable<IAST> GetChildren()
    {
        yield return SelectExpression;
        yield return TableExpression;
    }

    public string GetValue()
    {
        return Type.ToHumanReadableString();
    }
}