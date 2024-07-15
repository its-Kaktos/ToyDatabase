namespace SqlParser.Nodes;

public record CompoundNode : IAST
{
    public CompoundNode(List<IAST> children)
    {
        Children = children;
    }

    public List<IAST> Children { get; init; }

    public IEnumerable<IAST> GetChildren()
    {
        return Children;
    }

    public string GetValue()
    {
        return "Compound";
    }
}