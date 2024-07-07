namespace SqlParser;

public class Interpreter
{
    private readonly string _src;
    private int _position;
    private Token? _currentToken;
    private char? _currentCharacter;
    private const string MathOperations = "+-/*";

    public Interpreter(string src)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(src);
        _src = src;
        _position = 0;
        _currentCharacter = _src[_position];
        _currentToken = null;
    }

    public int Evaluate()
    {
        var left = GetNextToken();
        var mathOp = GetNextToken();
        var currentToken = GetNextToken();

        if (left == Token.EofToken) throw new InvalidOperationException("Left hand side token type can not be Eof.");
        if (mathOp == Token.EofToken) throw new InvalidOperationException("Math operator token type can not be Eof.");
        if (currentToken == Token.EofToken) throw new InvalidOperationException("Right hand side token type can not be Eof.");
        
        var leftValue = int.Parse(left.Value!);
        while (currentToken != Token.EofToken)
        {
            switch (mathOp.Type)
            {
                case TokenType.Plus:
                    leftValue += int.Parse(currentToken.Value!);
                    AdvanceMathOpAndCurrentToken();
                    if (IsTokensEof()) return leftValue;
                    break;
                case TokenType.Minus:
                    leftValue -= int.Parse(currentToken.Value!);
                    AdvanceMathOpAndCurrentToken();
                    if (IsTokensEof()) return leftValue;
                    break;
                case TokenType.Multiply:
                    leftValue *= int.Parse(currentToken.Value!);
                    AdvanceMathOpAndCurrentToken();
                    if (IsTokensEof()) return leftValue;
                    break;
                case TokenType.Divide:
                    leftValue /= int.Parse(currentToken.Value!);
                    AdvanceMathOpAndCurrentToken();
                    if (IsTokensEof()) return leftValue;
                    break;
                case TokenType.Integer:
                case TokenType.EOF:
                default:
                    throw new InvalidOperationException("Invalid math operator.");
            }
        }

        return leftValue;

        void AdvanceMathOpAndCurrentToken()
        {
            mathOp = GetNextToken();
            currentToken = GetNextToken();
        }

        bool IsTokensEof()
        {
            return left == Token.EofToken || mathOp == Token.EofToken || currentToken == Token.EofToken;
        }
    }

    /// <summary>
    /// Lexical analyzer (also known as scanner or tokenizer).
    /// This method is responsible for breaking a sentence apart into tokens. One token at a time.
    /// </summary>
    /// <returns>Token</returns>
    public Token GetNextToken()
    {
        SkipWhiteSpace();
        if (_currentCharacter is null) return Token.EofToken;

        if (TryParseMathOperator(_currentCharacter.Value, out var token))
        {
            Advance();
            return token!;
        }

        if (char.IsDigit(_currentCharacter.Value)) return GetInteger();

        throw new InvalidOperationException("Should not reach here");
    }

    /// <summary>
    /// compare the current token type with the passed token
    /// type and if they match then "eat" the current token
    /// and assign the next token to the _currentToken,
    /// otherwise raise an exception.
    /// </summary>
    /// <param name="type">current token type</param>
    /// <exception cref="InvalidOperationException">If _currentToken.Type does not match with provided type.</exception>
    private void Eat(TokenType type)
    {
        if (_currentToken?.Type != type) throw new InvalidOperationException("Error parsing input.");

        _currentToken = GetNextToken();
    }

    private static bool IsDigit(string input)
    {
        for (var i = 0; i < input.Length; i++)
        {
            if (!char.IsDigit(input[i])) return false;
        }

        return true;
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
        while (_currentCharacter is ' ')
        {
            Advance();
        }
    }

    private bool TryParseMathOperator(char input, out Token? token)
    {
        token = null;
        if (!MathOperations.Contains(input)) return false;

        token = input switch
        {
            '+' => new Token(TokenType.Plus),
            '-' => new Token(TokenType.Minus),
            '/' => new Token(TokenType.Divide),
            '*' => new Token(TokenType.Multiply),
            _ => throw new InvalidOperationException("Should not reach here.")
        };

        return true;
    }

    private Token GetInteger()
    {
        var startFrom = _position;
        while (_currentCharacter is not null)
        {
            if (char.IsDigit(_currentCharacter.Value))
            {
                Advance();
                continue;
            }

            var token = new Token(TokenType.Integer, _src[startFrom.._position]);
            return token;
        }

        _position = _src.Length;
        return new Token(TokenType.Integer, _src[startFrom..]);
    }
}