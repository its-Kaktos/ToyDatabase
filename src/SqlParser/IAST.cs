namespace SqlParser;

public interface IAST
{
    /// <summary>
    /// Returns children from left to right
    /// </summary>
    /// <returns>Children from left ot right</returns>
    IEnumerable<IAST> GetChildren()
    {
        return [];
    }

    /// <summary>
    /// Returns current node value
    /// </summary>
    /// <returns>Node value</returns>
    string GetValue()
    {
        return string.Empty;
    }
}