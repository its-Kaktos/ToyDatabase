using System.Runtime.Intrinsics.Arm;

namespace SqlParser;

public class Interpreter
{
    private readonly Lexer _lexer;
    private Token _currentToken;

    public Interpreter(Lexer lexer)
    {
        _lexer = lexer;
        _currentToken = lexer.NextToken();
    }

    public int Evaluate()
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

    private int GetExpr()
    {
        var result = GetTerm();

        while (_currentToken.Type is TokenType.Plus or TokenType.Minus)
        {
            switch (_currentToken.Type)
            {
                case TokenType.Plus:
                    Eat(TokenType.Plus);
                    result += GetTerm();
                    break;
                case TokenType.Minus:
                    Eat(TokenType.Minus);
                    result -= GetTerm();
                    break;
            }
        }

        return result;
    }

    private int GetTerm()
    {
        var result = GetFactor();

        while (_currentToken.Type is TokenType.Divide or TokenType.Multiply)
        {
            switch (_currentToken.Type)
            {
                case TokenType.Divide:
                    Eat(TokenType.Divide);
                    result /= GetFactor();
                    break;
                case TokenType.Multiply:
                    Eat(TokenType.Multiply);
                    result *= GetFactor();
                    break;
            }
        }

        return result;
    }

    private int GetFactor()
    {
        switch (_currentToken.Type)
        {
            case TokenType.Integer:
            {
                var current = _currentToken;
                Eat(TokenType.Integer);
                return current.ValueAsInt!.Value;
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