using SqlParser.Nodes;

namespace SqlParser;

// Grammar:
// expr :   term ( (PLUS | MINUS) term)*
// term :   power ( (MUL | DIV) power)*
// power :  factor ( (power) factor)*
// factor : (PLUS | MINUS) factor | INTEGER | LParen factor RParen 
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
        // expr   : term ((PLUS | MINUS) term)*
        // term   : factor ((MUL | DIV) factor)*
        // factor : INTEGER
        return GetExpr();
    }

    private void Eat(TokenType tokenType)
    {
        if (_currentToken.Type != tokenType) throw new InvalidOperationException("Invalid syntax.");

        _currentToken = _lexer.NextToken();
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

    // term : power ( (MUL | DIV) power)*
    private IAST GetTerm()
    {
        var node = GetPower();

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

            node = new BinaryOperator(node, op, GetPower());
        }

        return node;
    }

    // power : factor ( (power) factor)*
    private IAST GetPower()
    {
        var node = GetFactor();
        
        while (_currentToken.Type is TokenType.Power)
        {
            var op = _currentToken;
            Eat(TokenType.Power);
            node = new BinaryOperator(node, op, GetFactor());
        }

        return node;
    }

    // factor : (PLUS | MINUS) factor | INTEGER | LParen factor RParen 
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
            default:
                throw new InvalidOperationException($"{_currentToken.Type} is not a valid Factor");
        }
    }
}