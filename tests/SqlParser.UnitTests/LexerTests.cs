using System.Text.Json;
using FluentAssertions;
using SqlParser.BtreeImpl;

namespace SqlParser.UnitTests;

public class LexerTests
{
    public static IEnumerable<object[]> NextShouldBeValidBtree => new List<object[]>
    {
        new object[]
        {
            3,
            new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 },
            """
            {"Root":{"Keys":[4],"Children":[{"Keys":[2],"Children":[{"Keys":[1],"Children":[]},{"Keys":[3],"Children":[]}]},{"Keys":[6,8],"Children":[{"Keys":[5],"Children":[]},{"Keys":[7],"Children":[]},{"Keys":[9,10],"Children":[]}]}]}}
            """,
            
        }
    };

    [Theory]
    [MemberData(nameof(NextShouldBeValidBtree))]
    public void Next_should_be_valid_btree(int maxKeysCount, List<int> keys, string validBtreeJson)
    {
        var sut = new Btree(maxKeysCount);

        foreach (var key in keys)
        {
            sut.Insert(key);
        }

        var actual = JsonSerializer.Serialize(validBtreeJson);
        actual.Should().BeEquivalentTo(validBtreeJson);
    }
}