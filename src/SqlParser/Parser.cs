using System.Collections.Frozen;
using SqlParser.Nodes;

namespace SqlParser;

/*
Grammar:
program : compound_statement DOT

compound_statement : BEGIN statement_list END

statement_list : statement
                | statement SEMI statement_list

statement : compound_statement
            | assignment_statement
            | empty

assignment_statement : variable ASSIGN expr

empty :

expr: term ((PLUS | MINUS) term)*

term: factor ((MUL | DIV) factor)*

factor : PLUS factor
        | MINUS factor
        | INTEGER
        | LPAREN expr RPAREN
        | variable

variable: ID
 */
public class Parser
{
    private readonly Lexer _lexer;
    private Token _currentToken;

    public Parser(Lexer lexer)
    {
        _lexer = lexer;
        _currentToken = lexer.NextToken();
    }

    public IAST Parse()
    {
        return GetProgram();
    }

    // program : compound_statement DOT
    private IAST GetProgram()
    {
        var result = GetCompoundStatement();
        Eat(TokenType.Dot);

        return result;
    }

    // compound_statement : BEGIN statement_list END
    private IAST GetCompoundStatement()
    {
        Eat(TokenType.Begin);
        var nodes = GetStatementList();
        Eat(TokenType.End);

        return new CompoundNode(nodes);
    }

    // statement_list : statement
    // | statement SEMI statement_list
    private List<IAST> GetStatementList()
    {
        var result = new List<IAST> { GetStatement() };

        while (_currentToken.Type == TokenType.Semi)
        {
            Eat(TokenType.Semi);
            result.Add(GetStatement());
        }

        if (_currentToken.Type == TokenType.Id) throw new InvalidOperationException("Invalid syntax");

        return result;
    }

    // statement : compound_statement
    // | assignment_statement
    // | empty
    private IAST GetStatement()
    {
        return _currentToken.Type switch
        {
            TokenType.Begin => GetCompoundStatement(),
            TokenType.Id => GetAssignmentStatement(),
            _ => GetEmpty()
        };
    }

    // assignment_statement : variable ASSIGN expr
    private IAST GetAssignmentStatement()
    {
        var left = GetVariable();
        var op = _currentToken;
        Eat(TokenType.Assign);

        return new AssignNode(left, op, GetExpr());
    }

    // empty :
    private IAST GetEmpty()
    {
        return NoOpNode.Instance;
    }

    // expr : term ( (PLUS | MINUS) term)*
    private IAST GetExpr()
    {
        var node = GetTerm();

        while (_currentToken.Type is TokenType.Plus or TokenType.Minus)
        {
            var op = _currentToken;
            switch (_currentToken.Type)
            {
                case TokenType.Plus:
                    Eat(TokenType.Plus);
                    break;
                case TokenType.Minus:
                    Eat(TokenType.Minus);
                    break;
            }

            node = new BinaryOperator(node, op, GetTerm());
        }

        return node;
    }

    // term : factor ( (MUL | DIV) factor)*
    private IAST GetTerm()
    {
        var node = GetFactor();

        while (_currentToken.Type is TokenType.Divide or TokenType.Multiply)
        {
            var op = _currentToken;
            switch (_currentToken.Type)
            {
                case TokenType.Divide:
                    Eat(TokenType.Divide);
                    break;
                case TokenType.Multiply:
                    Eat(TokenType.Multiply);
                    break;
            }

            node = new BinaryOperator(node, op, GetFactor());
        }

        return node;
    }

    // factor : PLUS factor
    // | MINUS factor
    // | INTEGER
    // | LPAREN expr RPAREN
    // | variable
    private IAST GetFactor()
    {
        switch (_currentToken.Type)
        {
            case TokenType.Plus:
            {
                Eat(TokenType.Plus);
                return new UnaryOperator(new Token(TokenType.Plus), GetFactor());
            }
            case TokenType.Minus:
            {
                Eat(TokenType.Minus);
                return new UnaryOperator(new Token(TokenType.Minus), GetFactor());
            }
            case TokenType.Integer:
            {
                var current = _currentToken;
                Eat(TokenType.Integer);
                return new NumberNode(current);
            }
            case TokenType.LParen:
            {
                Eat(TokenType.LParen);
                var result = GetExpr();
                Eat(TokenType.RParen);
                return result;
            }
            case TokenType.Id:
                return GetVariable();
            default:
                throw new InvalidOperationException($"{_currentToken.Type} is not a valid Factor");
        }
    }

    // variable: ID
    private IAST GetVariable()
    {
        var result = new VariableNode(_currentToken);
        Eat(TokenType.Id);
        
        
        return result;
    }

    private void Eat(TokenType tokenType)
    {
        if (_currentToken.Type != tokenType) throw new InvalidOperationException("Invalid syntax.");

        _currentToken = _lexer.NextToken();
    }
}