using SqlParser.Nodes;

namespace SqlParser;

// Grammar:
// SELECT: SELECT selectExpression FROM tableExpression
// 
// selectExpression: *
// 
// tableExpression: name
// 
// name: A-Z 0-9
public class Parser
{
    private readonly Lexer _lexer;
    private Token _currentToken;

    public Parser(Lexer lexer)
    {
        _lexer = lexer;
        _currentToken = _lexer.NextToken();
    }

    public IAST Parse()
    {
        return GetSelect();
    }

    // SELECT: SELECT selectExpression FROM tableExpression
    private SelectNode GetSelect()
    {
        Eat(TokenType.Select);
        var selectExpr = GetSelectExpression();
        Eat(TokenType.From);
        var tableExpr = GetTableExpression();

        return new SelectNode(TokenType.Select, selectExpr, tableExpr);
    }

    // selectExpression: *
    private SelectExpressionNode GetSelectExpression()
    {
        var current = _currentToken;
        Eat(TokenType.Name);
        if (current.Value is not "*")
        {
            throw new InvalidOperationException($"{_currentToken.Value} is not a valid select expression.");
        }

        return new SelectExpressionNode(current.Type, current.Value!);
    }
    
    // tableExpression: name
    private TableExpressionNode GetTableExpression()
    {
        var current = GetName();

        return new TableExpressionNode(current.Type, current.Value!);
    }
    
    // name: A-Z 0-9
    private Token GetName()
    {
        var current = _currentToken;
        Eat(TokenType.Name);

        return current;
    }
    
    private void Eat(TokenType tokenType)
    {
        if (_currentToken.Type != tokenType)
        {
            throw new InvalidOperationException($"{_currentToken.Type} is not valid at this position.");
        }

        _currentToken = _lexer.NextToken();
    }
}