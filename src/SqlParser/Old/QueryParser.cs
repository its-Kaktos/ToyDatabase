using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace SqlParser;

public class QueryParser
{
    public List<Token3> Tokenizer(string sql)
    {
        ArgumentNullException.ThrowIfNull(sql);

        var tokens = new List<Token3>();
        for (var i = 0; i < sql.Length; i++)
        {
            switch (sql[i])
            {
                case ' ':
                case '\n':
                case '\r':
                case '\t':
                    tokens.Add(new Token3 { TokenType3 = TokenType3.WhiteSpace });
                    break;
                case '-':
                    tokens.Add(new Token3 { TokenType3 = TokenType3.Minus });
                    break;
                case '+':
                    tokens.Add(new Token3 { TokenType3 = TokenType3.Plus });
                    break;
                case '*':
                    tokens.Add(new Token3 { TokenType3 = TokenType3.Multiply });
                    break;
                case '/':
                    tokens.Add(new Token3 { TokenType3 = TokenType3.Divide });
                    break;
                case '^':
                    tokens.Add(new Token3 { TokenType3 = TokenType3.Power });
                    break;
                case >= '0' and <= '9':
                    tokens.Add(GetNumberToken(ref i, sql));
                    break;
                default:
                    tokens.Add(new Token3 { TokenType3 = TokenType3.String, ValueChar = sql[i] });
                    break;
            }
        }

        return tokens;
    }

    private Token3 GetNumberToken(ref int startFrom, string sql)
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
            ? new Token3 { TokenType3 = TokenType3.Number, ValueDouble = decimal.Parse(sql.AsSpan(startFrom, length)) } 
            : new Token3 { TokenType3 = TokenType3.Number, ValueInt = int.Parse(sql.AsSpan(startFrom, length)) };
        
        if (!stopIndexIsEmpty) startFrom = stopIndex - 1;

        return token;
    }

    public void CreateExpression(List<Token3> tokens)
    {
        Expression rootExpression;
        foreach (var token in tokens)
        {
            switch (token.TokenType3)
            {
                case TokenType3.Number:
                    break;
                case TokenType3.String:
                    break;
                case TokenType3.Plus:
                    break;
                case TokenType3.Minus:
                    break;
                case TokenType3.Multiply:
                    break;
                case TokenType3.Divide:
                    break;
                case TokenType3.Power:
                    break;
                case TokenType3.WhiteSpace:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}