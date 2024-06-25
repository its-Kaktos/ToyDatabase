namespace SqlParser;

public class Token
{
    public required TokenType TokenType { get; init; }
    public char? ValueChar { get; init; }
    public byte? ValueByte { get; init; }
    public string? ValueStr { get; init; }
    public long? ValueLong { get; init; }
}