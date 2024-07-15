// See https://www.youtube.com/watch?v=5Pc18ge9ohI&ab_channel=TonySaro
// https://ruslanspivak.com/lsbasi-part1/

using SqlParser;
using SqlParser.Extensions;

var input = """
            BEGIN
            
                BEGIN
                    number := 2;
                    _a := NumBer;
                    B := 10 * _a + 10 * NUMBER DiV 4;
                    c := _a - - b
                end;
            
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