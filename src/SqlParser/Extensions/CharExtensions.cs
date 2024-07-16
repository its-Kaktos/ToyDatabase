namespace SqlParser.Extensions;

public static class CharExtensions
{
    public static bool IsAlphabetical(this char? c)
    {
        return c is not null && IsAlphabetical(c.Value);
    }
    
    public static bool IsAlphabetical(this char c)
    {
        return c is >= 'a' and <= 'z'
            or >= 'A' and <= 'Z'
            or '_';
    }
    
    public static bool IsAlphanumeric(this char? c)
    {
        return c is not null && IsAlphanumeric(c.Value);
    }

    public static bool IsAlphanumeric(this char c)
    {
        return IsAlphabetical(c) || char.IsDigit(c);
    }
}