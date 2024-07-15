namespace SqlParser.Nodes;

public record BlockNode : IAST 
{
    public BlockNode(List<IAST> declarations, IAST compoundStatement)
    {
        Declarations = declarations;
        CompoundStatement = compoundStatement;
    }

    public List<IAST> Declarations { get; init; }
    public IAST CompoundStatement { get; init; }

    public IEnumerable<IAST> GetChildren()
    {
        foreach (var declaration in Declarations)
        {
            yield return declaration;
        }

        yield return CompoundStatement;
    }
    
    public string GetValue()
    {
        return "BLOCK";
    }
}