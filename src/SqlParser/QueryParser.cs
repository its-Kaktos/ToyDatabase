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
                case < '9' and > '0':
                    tokens.Add(new Token { TokenType = TokenType.Number, ValueByte = byte.Parse(sql.AsSpan(i, 1)) });
                    break;
                default:
                    tokens.Add(new Token { TokenType = TokenType.String, ValueChar = sql[i] });
                    break;
            }
        }

        return tokens;
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