using System.Collections.Frozen;
using SqlParser.Extensions;

namespace SqlParser;

public class Lexer
{
    private readonly string _src;
    private int _position;
    private char? _currentCharacter;
    private const string MathOperations = "+-/*";

    private static readonly FrozenDictionary<string, Token> ReservedKeywords = new Dictionary<string, Token>
    {
        { "BEGIN", new Token(TokenType.Begin) }, { "END", new Token(TokenType.End) }
    }.ToFrozenDictionary();

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

            if (_currentCharacter.Value.IsAlphabetical()) return ParseIdentifier();
            if (MathOperations.Contains(_currentCharacter.Value)) return ParseMathOperator();
            if (char.IsDigit(_currentCharacter.Value)) return ParseInteger();

            switch (_currentCharacter)
            {
                case '(':
                    Advance();
                    return new Token(TokenType.LParen);
                case ')':
                    Advance();
                    return new Token(TokenType.RParen);
                case '.':
                    Advance();
                    return new Token(TokenType.Dot);
                case ';':
                    Advance();
                    return new Token(TokenType.Semi);
                case ':' when Peek() == '=':
                    Advance();
                    Advance();
                    return new Token(TokenType.Assign);
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

    private char? Peek()
    {
        var position = _position + 1;

        if (position >= _src.Length) return null;

        return _src[position];
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
        var token = _currentCharacter switch
        {
            '*' => new Token(TokenType.Multiply),
            '/' => new Token(TokenType.Divide),
            '+' => new Token(TokenType.Plus),
            '-' => new Token(TokenType.Minus),
            _ => throw new InvalidOperationException($"{_currentCharacter} is not a valid math operator.")
        };

        Advance();

        return token;
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

    private Token ParseIdentifier()
    {
        var startPosition = _position;
        var endPosition = -1;
        while (_currentCharacter is not null && _currentCharacter.Value.IsAlphanumeric())
        {
            endPosition = _position;
            Advance();
        }
        var length = endPosition is -1 ? _src.Length - 1 - startPosition : endPosition - startPosition + 1;
        var tokenValue = _src.Substring(startPosition, length);

        return ReservedKeywords.TryGetValue(tokenValue, out var result) ? result : new Token(TokenType.Id, tokenValue);
    }
}