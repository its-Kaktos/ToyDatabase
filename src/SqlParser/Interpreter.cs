namespace SqlParser;

public class Interpreter
{
    private readonly string _src;
    private int _position;
    private Token? _currentToken;
    private const string MathOperations = "+-/*";

    public Interpreter(string src)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(src);
        _src = src;
        _position = 0;
        _currentToken = null;
    }

    public int Evaluate()
    {
        _currentToken = GetNextToken();
        
        var left = _currentToken!;
        Eat(TokenType.Integer);
        
        var operation = _currentToken!;
        Eat(TokenType.Plus);
        
        var right = _currentToken;
        Eat(TokenType.Integer);
        
        var result = int.Parse(left.Value!) + int.Parse(right.Value!);
        return result;
    }

    /// <summary>
    /// Lexical analyzer (also known as scanner or tokenizer).
    /// This method is responsible for breaking a sentence apart into tokens. One token at a time.
    /// </summary>
    /// <returns>Token</returns>
    public Token GetNextToken()
    {
        var current = NextTokenAsString();
        if (current is null) return Token.EofToken;

        if (current.Length == 1 && MathOperations.Contains(current[0])) return GetMathOperationToken(current);
        if (IsDigit(current)) return new Token(TokenType.Integer, current);

        throw new InvalidOperationException($"{current} is a invalid token");
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

    /// <summary>
    /// Retrieves the next token from the input string.
    /// </summary>
    /// <returns>
    /// Returns <c>null</c> if there are no tokens left in input string.
    /// Returns an empty string (" ") if the current token is a space character.
    /// Otherwise, returns the next token found in input string.
    /// </returns>
    private string? NextTokenAsString()
    {
        if (_position >= _src.Length) return null;

        while (_src[_position] == ' ')
        {
            _position++;
            if (_position >= _src.Length) return null;
        }

        if (MathOperations.Contains(_src[_position]))
        {
            var result = _src[_position].ToString();
            _position++;
            return result;
        }

        var startFrom = _position;
        for (var i = startFrom; i < _src.Length; i++)
        {
            if (_src[i] != ' ' && !MathOperations.Contains(_src[i])) continue;

            _position = i;
            return _src[startFrom..i];
        }

        _position = _src.Length;
        return _src[startFrom..];
    }

    private static bool IsDigit(string input)
    {
        for (var i = 0; i < input.Length; i++)
        {
            if (!char.IsDigit(input[i])) return false;
        }

        return true;
    }

    private static Token GetMathOperationToken(string input)
    {
        if (input.Length is not 1) throw new InvalidOperationException("Math operator can only have 1 character.");

        return input[0] switch
        {
            '+' => new Token(TokenType.Plus),
            '-' => new Token(TokenType.Minus),
            '/' => new Token(TokenType.Divide),
            '*' => new Token(TokenType.Multiply),
            _ => throw new InvalidOperationException($"{input} is not a valid math operator.")
        };
    }
}