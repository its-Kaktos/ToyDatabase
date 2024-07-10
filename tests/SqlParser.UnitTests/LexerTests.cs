using FluentAssertions;

namespace SqlParser.UnitTests;

public class LexerTests
{
    public static IEnumerable<object[]> NextShouldReturnValidTokenData => new List<object[]>
    {
        new object[]
        {
            "1", new[]
            {
                new Token(TokenType.Integer, "1")
            }
        },
        new object[]
        {
            "12", new[]
            {
                new Token(TokenType.Integer, "12")
            }
        },
        new object[]
        {
            "12+", new[]
            {
                new Token(TokenType.Integer, "12"),
                new Token(TokenType.Plus)
            }
        },
        new object[]
        {
            "12+3", new[]
            {
                new Token(TokenType.Integer, "12"),
                new Token(TokenType.Plus),
                new Token(TokenType.Integer, "3")
            }
        },
        new object[]
        {
            "12+3-", new[]
            {
                new Token(TokenType.Integer, "12"),
                new Token(TokenType.Plus),
                new Token(TokenType.Integer, "3"),
                new Token(TokenType.Minus)
            }
        },
        new object[]
        {
            "12+3-4", new[]
            {
                new Token(TokenType.Integer, "12"),
                new Token(TokenType.Plus),
                new Token(TokenType.Integer, "3"),
                new Token(TokenType.Minus),
                new Token(TokenType.Integer, "4")
            }
        },
        new object[]
        {
            "12+3-4*", new[]
            {
                new Token(TokenType.Integer, "12"),
                new Token(TokenType.Plus),
                new Token(TokenType.Integer, "3"),
                new Token(TokenType.Minus),
                new Token(TokenType.Integer, "4"),
                new Token(TokenType.Multiply)
            }
        },
        new object[]
        {
            "12+3-4*8", new[]
            {
                new Token(TokenType.Integer, "12"),
                new Token(TokenType.Plus),
                new Token(TokenType.Integer, "3"),
                new Token(TokenType.Minus),
                new Token(TokenType.Integer, "4"),
                new Token(TokenType.Multiply),
                new Token(TokenType.Integer, "8")
            }
        },
        new object[]
        {
            "12+3-4*8/", new[]
            {
                new Token(TokenType.Integer, "12"),
                new Token(TokenType.Plus),
                new Token(TokenType.Integer, "3"),
                new Token(TokenType.Minus),
                new Token(TokenType.Integer, "4"),
                new Token(TokenType.Multiply),
                new Token(TokenType.Integer, "8"),
                new Token(TokenType.Divide)
            }
        },
        new object[]
        {
            "12+3-4*8/9", new[]
            {
                new Token(TokenType.Integer, "12"),
                new Token(TokenType.Plus),
                new Token(TokenType.Integer, "3"),
                new Token(TokenType.Minus),
                new Token(TokenType.Integer, "4"),
                new Token(TokenType.Multiply),
                new Token(TokenType.Integer, "8"),
                new Token(TokenType.Divide),
                new Token(TokenType.Integer, "9")
            }
        },
        new object[]
        {
            "12+3 -4*8/ 9", new[]
            {
                new Token(TokenType.Integer, "12"),
                new Token(TokenType.Plus),
                new Token(TokenType.Integer, "3"),
                new Token(TokenType.Minus),
                new Token(TokenType.Integer, "4"),
                new Token(TokenType.Multiply),
                new Token(TokenType.Integer, "8"),
                new Token(TokenType.Divide),
                new Token(TokenType.Integer, "9")
            }
        },
        new object[]
        {
            "12 + 3 -4*8/ 9", new[]
            {
                new Token(TokenType.Integer, "12"),
                new Token(TokenType.Plus),
                new Token(TokenType.Integer, "3"),
                new Token(TokenType.Minus),
                new Token(TokenType.Integer, "4"),
                new Token(TokenType.Multiply),
                new Token(TokenType.Integer, "8"),
                new Token(TokenType.Divide),
                new Token(TokenType.Integer, "9")
            }
        },
        new object[]
        {
            "12 + 3 -4* 8/ 9", new[]
            {
                new Token(TokenType.Integer, "12"),
                new Token(TokenType.Plus),
                new Token(TokenType.Integer, "3"),
                new Token(TokenType.Minus),
                new Token(TokenType.Integer, "4"),
                new Token(TokenType.Multiply),
                new Token(TokenType.Integer, "8"),
                new Token(TokenType.Divide),
                new Token(TokenType.Integer, "9")
            }
        }
    };

    [Theory]
    [MemberData(nameof(NextShouldReturnValidTokenData))]
    public void Next_should_return_valid_token(string input, Token[] expected)
    {
        var sut = new Lexer(input);

        var actual = new List<Token>();
        var next = sut.NextToken();
        while (next != Token.EofToken)
        {
            actual.Add(next);
            next = sut.NextToken();
        }

        actual.Should().BeEquivalentTo(expected);
    }
}