using FluentAssertions;

namespace SqlParser.UnitTests;

public class LexerTests
{
    public static IEnumerable<object[]> NextShouldReturnValidTokenData => new List<object[]>
    {
        new object[]
        {
            "SELECT", new[]
            {
                new Token(TokenType.Select),
            }
        },
        new object[]
        {
            "SELECT * FROM TABLENAME", new[]
            {
                new Token(TokenType.Select),
                new Token(TokenType.Name, "*"),
                new Token(TokenType.From),
                new Token(TokenType.Name, "TABLENAME"),
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
        while (next.Type != TokenType.EOF)
        {
            actual.Add(next);
            next = sut.NextToken();
        }

        actual.Should().BeEquivalentTo(expected);
    }
}