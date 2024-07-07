namespace SqlParser;

public class Token3
{
    public required TokenType3 TokenType3 { get; init; }
    public char? ValueChar { get; init; }
    public int? ValueInt { get; init; }
    public decimal? ValueDouble { get; init; }
}