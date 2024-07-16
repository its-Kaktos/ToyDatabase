namespace SqlParser.Nodes;

public interface IAST
{
    IEnumerable<IAST> GetChildren()
    {
        return [];
    }

    string GetValue()
    {
        return "";
    }
}