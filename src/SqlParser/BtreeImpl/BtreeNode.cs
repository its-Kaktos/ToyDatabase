namespace SqlParser.BtreeImpl;

public record BtreeNode
{
    public BtreeNode()
    {
        Keys = [];
        Child = [];
    }
    
    public List<int> Keys { get; set; }
    public List<BtreeNode> Child { get; set; }
}