namespace SqlParser;

// ReSharper disable InconsistentNaming
public enum TokenType
{
    Select,
    From,
    Name,
    WhiteSpace,
    EOF
}

public static class TokenTypeExtensions
{
    public static string ToHumanReadableString(this TokenType tokenType)
    {
        return tokenType switch
        {
            
            TokenType.EOF => tokenType.ToString(),
            TokenType.Select => "SELECT",
            TokenType.From => "FROM",
            TokenType.Name => "NAME",
            TokenType.WhiteSpace => "WHITE_SPACE",
            _ => throw new ArgumentOutOfRangeException(nameof(tokenType), tokenType, null)
        };
    }
}