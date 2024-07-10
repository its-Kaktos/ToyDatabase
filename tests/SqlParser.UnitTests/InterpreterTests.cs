using FluentAssertions;

namespace SqlParser.UnitTests;

public class InterpreterTests
{
    public static IEnumerable<object[]> ShouldEvaluateCorrectlyData => new List<object[]>
    {
        new object[] { "1", 1 },
        new object[] { "1+2", 3 },
        new object[] { "1+2-3", 0 },
        new object[] { "1+2-4", -1 },
        new object[] { "6 / 2", 3 },
        new object[] { "10 / 2 * 5", 25 },
    };

    [Theory]
    [MemberData(nameof(ShouldEvaluateCorrectlyData))]
    public void Should_evaluate_correctly(string input, int expected)
    {
        var lexer = new Lexer(input);
        var sut = new Interpreter(lexer);

        var actual = sut.Evaluate();

        actual.Should().Be(expected);
    }
}