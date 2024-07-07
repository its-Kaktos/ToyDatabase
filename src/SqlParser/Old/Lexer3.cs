using System.Collections.Frozen;

namespace SqlParser;

public class Lexer3
{
    private static readonly FrozenSet<char> MathOperations = new[] { '*', '-', '+', '/' }.ToFrozenSet();
    
    /// <summary>
    /// Tokenizes the input string <paramref name="src"/> into individual tokens.
    /// </summary>
    /// <param name="src">The source string to tokenize.</param>
    /// <returns>An enumerable collection of tokens.</returns>
    public static IEnumerable<string> Tokenize(string src)
    {
        var lastVisited = 0;
        var token = Next(src, ref lastVisited);
        while (token != null)
        {
            if (token.Length != 0) yield return token;
            token = Next(src, ref lastVisited);
        }
    }

    /// <summary>
    /// Retrieves the next token from the input string <paramref name="src"/>.
    /// </summary>
    /// <param name="src">The source string from which to extract the token.</param>
    /// <param name="lastVisited">The index of the last visited character in <paramref name="src"/>.</param>
    /// <returns>
    /// Returns <c>null</c> if there are no tokens left in <paramref name="src"/>.
    /// Returns an empty string (" ") if the current token is a space character.
    /// Otherwise, returns the next token found in <paramref name="src"/>.
    /// </returns>
    private static string? Next(string src, ref int lastVisited)
    {
        if (string.IsNullOrWhiteSpace(src) || lastVisited >= src.Length) return null;
        if (src[lastVisited] == ' ')
        {
            lastVisited++;
            return "";
        }

        if (MathOperations.Contains(src[lastVisited]))
        {
            var result = src[lastVisited].ToString();
            lastVisited++;
            return result;
        }

        var startFrom = lastVisited;
        for (var i = startFrom; i < src.Length; i++)
        {
            if (src[i] != ' ' && !MathOperations.Contains(src[i])) continue;

            lastVisited = i;
            return src[startFrom..(i + 1)];
        }

        lastVisited = src.Length;
        return src[startFrom..];
    }
}