using FluentAssertions;

namespace SqlParser.UnitTests;

public class ParserTests
{
    public static IEnumerable<object[]> ShouldEvaluateCorrectlyData => new List<object[]>
    {
        new object[] { "1", 1 },
        new object[] { "1+2", 3 },
        new object[] { "1+2-3", 0 },
        new object[] { "1+2-4", -1 },
        new object[] { "6 / 2", 3 },
        new object[] { "10 / 2 * 5", 25 },
        new object[] { "8 + 2 * 5", 18 },
        new object[] { "8 + 2 * 5 / 5", 10 }
    };
    
    public static IEnumerable<object[]> ShouldEvaluateParenthesesCorrectlyData => new List<object[]>
    {
        new object[] { "(1 + 1)", 2 },
        new object[] { "(1+1)", 2 },
        new object[] { "(1+1  )", 2 },
        new object[] { "(   1+1  )", 2 },
        new object[] { "(   1 +   1  )", 2 },
        new object[] { "2 * (1 + 1)", 4 },
        new object[] { "2 * (1 + 1) + 3", 7 },
        new object[] { "2 * (1 + 1) + 3 * (5 - 3)", 10 },
    };

    public static IEnumerable<object[]> ShouldEvaluatePowerOfCorrectlyData => new List<object[]>
    {
        new object[] { "(1 + 1) ^ 2", 4 },
        new object[] { "(1 + 2 * 3 ^ 2) ^ 2", 361 },
        new object[] { "(1 + 1) ^ 2 * 6", 24 },
    };
    
    [Theory]
    [MemberData(nameof(ShouldEvaluateCorrectlyData))]
    [MemberData(nameof(ShouldEvaluateParenthesesCorrectlyData))]
    [MemberData(nameof(ShouldEvaluatePowerOfCorrectlyData))]
    public void Should_evaluate_correctly(string input, int expected)
    {
        var lexer = new Lexer(input);
        var parser = new Parser(lexer);
        var sut = new Interpreter(parser);

        var actual = sut.Evaluate();

        actual.Should().Be(expected);
    }
}