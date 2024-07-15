// See https://www.youtube.com/watch?v=5Pc18ge9ohI&ab_channel=TonySaro
// https://ruslanspivak.com/lsbasi-part1/

using SqlParser;
using SqlParser.Extensions;

var input = """
            BEGIN
            
                BEGIN
                    number := 2;
                    a := number;
                    b := 10 * a + 10 * number / 4;
                    c := a - - b
                END;
            
                x := 11;
            END.
            """;
var lexer = new Lexer(input);
var parser = new Parser(lexer);
var interpreter = new Interpreter(parser);

try
{
    parser.Parse().PrettyPrint();
    // Console.WriteLine(interpreter.Evaluate());
}
catch (Exception e)
{
    Console.WriteLine(e);
    throw;
}