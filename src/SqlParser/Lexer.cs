using System.Collections.Frozen;
using SqlParser.Extensions;

namespace SqlParser;

public class Lexer
{
    private readonly string _src;
    private int _position;
    private char? _currentCharacter;
    private const string MathOperations = "+-*/";

    private static readonly FrozenDictionary<string, Token> ReservedKeywords = new Dictionary<string, Token>
    {
        { "begin", new Token(TokenType.Begin) },
        { "end", new Token(TokenType.End) },
        { "div", new Token(TokenType.RealDivide) },
        { "program", new Token(TokenType.Program) },
        { "var", new Token(TokenType.VarDecl) },
        { "integer", new Token(TokenType.Integer) },
        { "real", new Token(TokenType.Real) }
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

            if (_currentCharacter.Value is '{')
            {
                Advance();
                SkipComment();
                continue;
            }

            if (_currentCharacter.Value.IsAlphabetical()) return ParseIdentifier();
            if (MathOperations.Contains(_currentCharacter.Value)) return ParseMathOperator();
            if (char.IsDigit(_currentCharacter.Value)) return ParseNumber();

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
                case ':':
                    if (Peek() == '=')
                    {
                        Advance();
                        Advance();
                        return new Token(TokenType.Assign);
                    }
                    
                    Advance();
                    return new Token(TokenType.Colon);
                case ',':
                    Advance();
                    return new Token(TokenType.Comma);
                default:
                    throw new InvalidOperationException($"{_currentCharacter} is not a valid token.");
            }
        }

        return Token.EofToken;
    }

    private void SkipComment()
    {
        while (_currentCharacter is not '}')
        {
            Advance();
        }

        Advance();
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
            '+' => new Token(TokenType.Plus),
            '-' => new Token(TokenType.Minus),
            '/' => new Token(TokenType.IntegerDivide),
            _ => throw new InvalidOperationException($"{_currentCharacter} is not a valid math operator.")
        };

        Advance();

        return token;
    }

    private Token ParseNumber()
    {
        var isFloat = false;
        var startPosition = _position;
        var endPosition = -1;
        while (_currentCharacter is not null && char.IsDigit(_currentCharacter.Value))
        {
            endPosition = _position;
            Advance();
        }

        if (_currentCharacter is '.')
        {
            isFloat = true;
            Advance();
        }

        while (_currentCharacter is not null && char.IsDigit(_currentCharacter.Value))
        {
            endPosition = _position;
            Advance();
        }

        var length = endPosition is -1 ? _src.Length - 1 - startPosition : endPosition - startPosition + 1;
        var tokenValue = _src.Substring(startPosition, length);
        return isFloat ? new Token(TokenType.RealConst, tokenValue) : new Token(TokenType.IntegerConst, tokenValue);
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
        var tokenValue = _src.Substring(startPosition, length).ToLower();

        return ReservedKeywords.TryGetValue(tokenValue, out var result) ? result : new Token(TokenType.Id, tokenValue);
    }
}