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
        var result = GetFactor();

        while (_currentToken.Type is TokenType.Divide or TokenType.Multiply or TokenType.Plus or TokenType.Minus)
        {
            int right;
            switch (_currentToken.Type)
            {
                case TokenType.Plus:
                    Eat(TokenType.Plus);
                    right = GetFactor();
                    result += right;
                    continue;
                case TokenType.Minus:
                    Eat(TokenType.Minus);
                    right = GetFactor();
                    result -= right;
                    continue;
                case TokenType.Multiply:
                    Eat(TokenType.Multiply);
                    right = GetFactor();
                    result *= right;
                    continue;
                case TokenType.Divide:
                    Eat(TokenType.Divide);
                    right = GetFactor();
                    result /= right;
                    continue;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        return result;
    }

    private void Eat(TokenType tokenType)
    {
        if (_currentToken.Type != tokenType) throw new InvalidOperationException("Invalid syntax.");

        _currentToken = _lexer.NextToken();
    }
    
    private int GetFactor()
    {
        var current = _currentToken;
        Eat(TokenType.Integer);

        return current.ValueAsInt!.Value;
    }
}