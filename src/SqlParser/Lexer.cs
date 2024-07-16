using System.Collections.Frozen;
using SqlParser.Extensions;

namespace SqlParser;

public class Lexer
{
    private readonly string _src;
    private int _position;
    private char? _currentCharacter;

    private FrozenDictionary<string, Token> _reservedKeywords = new Dictionary<string, Token>
    {
        { "select", new Token(TokenType.Select) },
        { "from", new Token(TokenType.From) }
    }.ToFrozenDictionary();

    public Lexer(string src)
    {
        if (src.Length <= 0) throw new ArgumentException("Length can not be equal or less than 0", nameof(src));

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

            if (_currentCharacter is '*')
            {
                Advance();
                return new Token(TokenType.Name, "*");
            }
            if (!_currentCharacter.IsAlphanumeric()) throw new InvalidOperationException($"{_currentCharacter} is not a valid character");

            return GetIdentifier();
        }

        return new Token(TokenType.EOF);
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

    private char? PeekPrevious()
    {
        var position = _position - 1;
        if (position <= _src.Length)
        {
            return _src[position];
        }

        return null;
    }

    private void SkipWhiteSpace()
    {
        while (_currentCharacter is not null && char.IsWhiteSpace(_currentCharacter.Value))
        {
            Advance();
        }
    }

    private Token GetIdentifier()
    {
        var startPosition = _position;
        var endPosition = -1;
        while (_currentCharacter is not null && _currentCharacter.IsAlphanumeric())
        {
            endPosition = _position;
            Advance();
        }

        var length = endPosition is -1 ? _src.Length - 1 - startPosition : endPosition - startPosition + 1;
        var tokenValue = _src.Substring(startPosition, length);

        return _reservedKeywords.TryGetValue(tokenValue.ToLower(), out var result) ? result : 
                new Token(TokenType.Name, tokenValue);
    }
}