namespace SqlParser.Nodes;

public record ProgramNode : IAST
{
    public ProgramNode(Token token, IAST name, IAST block)
    {
        Token = token;
        Name = name;
        Block = block;
    }
    
    public Token Token { get; init; }
    public IAST Name { get; init; }
    public IAST Block { get; init; }

    public IEnumerable<IAST> GetChildren()
    {
        yield return Block;
    }
    
    public string GetValue()
    {
        return Token.Type.ToHumanReadableString();
    }
}