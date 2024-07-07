using FluentAssertions;

namespace SqlParser.UnitTests;

public class LexerTests
{
    public static IEnumerable<object[]> NextShouldReturnTokenData =>
        new List<object[]>
        {
            new object[] { "1", new[] { "1" } },
            new object[] { "12", new[] { "12" } },
            new object[] { "12+", new[] { "12", "+" } },
            new object[] { "12+3", new[] { "12", "+", "3" } },
            new object[] { "12+3-", new[] { "12", "+", "3", "-" } },
            new object[] { "12+3-4", new[] { "12", "+", "3", "-", "4" } },
            new object[] { "12+3-4*", new[] { "12", "+", "3", "-", "4", "*" } },
            new object[] { "12+3-4*8", new[] { "12", "+", "3", "-", "4", "*", "8" } },
            new object[] { "12+3-4*8/", new[] { "12", "+", "3", "-", "4", "*", "8", "/" } },
            new object[] { "12+3-4*8/9", new[] { "12", "+", "3", "-", "4", "*", "8", "/", "9" } },
            new object[] { "12+3 -4*8/ 9", new[] { "12", "+", "3", "-", "4", "*", "8", "/", "9" } },
            new object[] { "12 + 3 -4*8/ 9", new[] { "12", "+", "3", "-", "4", "*", "8", "/", "9" } },
            new object[] { "12 + 3 -4* 8/ 9", new[] { "12", "+", "3", "-", "4", "*", "8", "/", "9" } },
            new object[] { "1 2", new[] { "1", "2"} },
        };

    // [Theory]
    // [MemberData(nameof(NextShouldReturnTokenData))]
    // public void Next_should_return_token(string input, string[] expected)
    // {
    //     var sut = new Lexer(input);
    //
    //     var actual = new List<string>();
    //     var next = sut.Next();
    //     while (next is not null)
    //     {
    //         actual.Add(next);
    //         next = sut.Next();
    //     }
    //
    //     expected.Should().BeEquivalentTo(actual);
    // }
}