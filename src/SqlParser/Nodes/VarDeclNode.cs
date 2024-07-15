namespace SqlParser.Nodes;

public class VarDeclNode : IAST
{
    public VarDeclNode(IAST variable, IAST variableType)
    {
        Token = new Token(TokenType.VarDecl);
        Variable = variable;
        VariableType = variableType;
    }

    public Token Token { get; init; }
    public IAST Variable { get; init; }
    public IAST VariableType { get; init; }
    
    public IEnumerable<IAST> GetChildren()
    {
        yield return Variable;
        yield return VariableType;
    }

    public string GetValue()
    {
        return Token.Type.ToHumanReadableString();
    }
}