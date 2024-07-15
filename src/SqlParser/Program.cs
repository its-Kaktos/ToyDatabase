// See https://www.youtube.com/watch?v=5Pc18ge9ohI&ab_channel=TonySaro
// https://ruslanspivak.com/lsbasi-part1/

using System.Text;
using SqlParser;
using SqlParser.Extensions;

var input = """
            PROGRAM Part10;
            VAR
               number     : INTEGER;
               a, b, c : INTEGER;
               y          : REAL;
            
            BEGIN {Part10}
               BEGIN
                  number := 2;
                  a := number;
                  b := 10 * a + 10 * number DIV 4;
                  c := a - - b
               END;
               y := 20 / 7 + 3.14;
               { writeln('a = ', a); }
               { writeln('b = ', b); }
               { writeln('c = ', c); }
               { writeln('number = ', number); }
               { writeln('x = ', x); }
               { writeln('y = ', y); }
            END.  {Part10}
            """;
var lexer = new Lexer(input);
var parser = new Parser(lexer);
var interpreter = new Interpreter(parser);

try
{
    // parser.Parse().PrettyPrint();

    var sb = new StringBuilder();
    foreach (var (key, value) in interpreter.Evaluate())
    {
        sb.Append(key);
        sb.Append(" = ");
        sb.Append(value);
    }

    Console.WriteLine(sb.ToString());
}
catch (Exception e)
{
    Console.WriteLine(e);
    throw;
}