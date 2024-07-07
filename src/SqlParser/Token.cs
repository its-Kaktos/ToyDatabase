using System.Text;

namespace SqlParser;

public record Token(TokenType Type, string? Value = null)
{
    public static readonly Token EofToken = new(TokenType.EOF);

    public override string ToString()
    {
        var sb = new StringBuilder()
            .Append("Token(")
            .Append(Type.ToString());

        if (Value is not null) sb.Append(',').Append(Value);
        sb.Append(')');

        return sb.ToString();
    }
}