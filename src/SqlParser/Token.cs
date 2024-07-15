using System.Text;

namespace SqlParser;

public record Token
{
    public static readonly Token EofToken = new(TokenType.EOF);

    public Token(TokenType Type, string? Value = null)
    {
        this.Type = Type;
        this.Value = Value;
        parseValueBasedOnType();
    }

    public TokenType Type { get; init; }
    public string? Value { get; init; }

    public int? ValueAsInt { get; private set; }
    public float? ValueAsFloat { get; private set; }

    public override string ToString()
    {
        var sb = new StringBuilder()
            .Append("Token(")
            .Append(Type.ToString());

        if (Value is not null) sb.Append(',').Append(Value);
        sb.Append(')');

        return sb.ToString();
    }

    private void parseValueBasedOnType()
    {
        if(Value is null) return;

        switch (Type)
        {
            case TokenType.Integer:
                ValueAsInt = int.Parse(Value);
                break;
            case TokenType.Real:
                ValueAsFloat = float.Parse(Value!);
                break;
        }
    }
}