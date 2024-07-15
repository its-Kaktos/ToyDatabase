namespace SqlParser.Nodes;

public record NoOpNode : IAST
{
    public static readonly NoOpNode Instance = new NoOpNode();
    
    public string GetValue()
    {
        return nameof(NoOpNode);
    }
}