using FluentAssertions;

namespace SqlParser.UnitTests;

public class InterpreterTests
{
    public static IEnumerable<object[]> ShouldEvaluateMathOperationsData => new List<object[]>
    {
        new object[] { "1+1", 2 },
        new object[] { "100+100 - 100", 100 },
        new object[] { "100+100 - 100-100", 0 },
        new object[] { "250-100", 150 }
    };
    
    [Theory]
    [MemberData(nameof(ShouldEvaluateMathOperationsData))]
    public void Should_evaluate_math_operation(string input, int expected)
    {
        var sut = new Interpreter(input);

        var actual = sut.Evaluate();

        actual.Should().Be(expected);
    }
}