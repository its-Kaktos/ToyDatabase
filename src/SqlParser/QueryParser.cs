using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace SqlParser;

public class QueryParser
{
    public List<Token> Tokenizer(string sql)
    {
        ArgumentNullException.ThrowIfNull(sql);

        var tokens = new List<Token>();
        for (var i = 0; i < sql.Length; i++)
        {
            switch (sql[i])
            {
                case ' ':
                case '\n':
                case '\r':
                case '\t':
                    tokens.Add(new Token { TokenType = TokenType.WhiteSpace });
                    break;
                case '-':
                    tokens.Add(new Token { TokenType = TokenType.Minus });
                    break;
                case '+':
                    tokens.Add(new Token { TokenType = TokenType.Plus });
                    break;
                case '*':
                    tokens.Add(new Token { TokenType = TokenType.Multiply });
                    break;
                case '/':
                    tokens.Add(new Token { TokenType = TokenType.Divide });
                    break;
                case '^':
                    tokens.Add(new Token { TokenType = TokenType.Power });
                    break;
                case >= '0' and <= '9':
                    tokens.Add(GetNumberToken(ref i, sql));
                    break;
                default:
                    tokens.Add(new Token { TokenType = TokenType.String, ValueChar = sql[i] });
                    break;
            }
        }

        return tokens;
    }

    private Token GetNumberToken(ref int startFrom, string sql)
    {
        var isDouble = false;
        var stopIndex = -1;
        for (var j = startFrom; j <= sql.Length; j++)
        {
            if (j == sql.Length)
            {
                stopIndex = j;
                break;
            }

            switch (sql[j])
            {
                case '.':
                    isDouble = true;
                    continue;
                case >= '0' and <= '9':
                    continue;
            }

            stopIndex = j;
            break;
        }

        var stopIndexIsEmpty = stopIndex == -1;
        var length = stopIndexIsEmpty ? 1 : stopIndex - startFrom;
        var token = isDouble
            ? new Token { TokenType = TokenType.Number, ValueDouble = decimal.Parse(sql.AsSpan(startFrom, length)) } 
            : new Token { TokenType = TokenType.Number, ValueInt = int.Parse(sql.AsSpan(startFrom, length)) };
        
        if (!stopIndexIsEmpty) startFrom = stopIndex - 1;

        return token;
    }

    public void CreateExpression(List<Token> tokens)
    {
        Expression rootExpression;
        foreach (var token in tokens)
        {
            switch (token.TokenType)
            {
                case TokenType.Number:
                    break;
                case TokenType.String:
                    break;
                case TokenType.Plus:
                    break;
                case TokenType.Minus:
                    break;
                case TokenType.Multiply:
                    break;
                case TokenType.Divide:
                    break;
                case TokenType.Power:
                    break;
                case TokenType.WhiteSpace:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}