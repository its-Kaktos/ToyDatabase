namespace SqlParser;

public class Token
{
    public required TokenType TokenType { get; init; }
    public char? ValueChar { get; init; }
    public int? ValueInt { get; init; }
    public decimal? ValueDouble { get; init; }
}