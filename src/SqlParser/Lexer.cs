namespace SqlParser;

public class Lexer
{
    private readonly string _src;
    private int _position;
    private char? _currentCharacter;
    private const string MathOperations = "+-/*";

    public Lexer(string src)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(src);
        _src = src;
        _position = 0;
        _currentCharacter = _src[_position];
    }

    public Token NextToken()
    {
        while (_currentCharacter is not null)
        {
            if (char.IsWhiteSpace(_currentCharacter.Value))
            {
                SkipWhiteSpace();
                continue;
            }

            if (MathOperations.Contains(_currentCharacter.Value))
            {
                // Parse and return math operation
                var mathOperator = ParseMathOperator();
                Advance();
                return mathOperator;
            }

            if (char.IsDigit(_currentCharacter.Value))
            {
                return ParseInteger();
            }

            switch (_currentCharacter)
            {
                case '(':
                    Advance();
                    return new Token(TokenType.LParen);
                case ')':
                    Advance();
                    return new Token(TokenType.RParen);
                default:
                    throw new InvalidOperationException($"{_currentCharacter} is not a valid token.");
            }
        }

        return Token.EofToken;
    }

    private void Advance()
    {
        _position++;

        if (_position >= _src.Length)
        {
            _currentCharacter = null;
            return;
        }

        _currentCharacter = _src[_position];
    }

    private void SkipWhiteSpace()
    {
        while (_currentCharacter is not null && char.IsWhiteSpace(_currentCharacter.Value))
        {
            Advance();
        }
    }

    private Token ParseMathOperator()
    {
        return _currentCharacter switch
        {
            '*' => new Token(TokenType.Multiply),
            '/' => new Token(TokenType.Divide),
            '+' => new Token(TokenType.Plus),
            '-' => new Token(TokenType.Minus),
            _ => throw new InvalidOperationException($"{_currentCharacter} is not a valid math operator.")
        };
    }

    private Token ParseInteger()
    {
        var startPosition = _position;
        var endPosition = -1;
        while (_currentCharacter is not null && char.IsDigit(_currentCharacter.Value))
        {
            endPosition = _position;
            Advance();
        }

        var length = endPosition is -1 ? _src.Length - 1 - startPosition : endPosition - startPosition + 1;

        return new Token(TokenType.Integer, _src.Substring(startPosition, length));
    }
}