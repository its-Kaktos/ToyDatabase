// See https://www.youtube.com/watch?v=5Pc18ge9ohI&ab_channel=TonySaro
// https://ruslanspivak.com/lsbasi-part1/
// https://forcedotcom.github.io/phoenix/index.html#select_expression

using SqlParser;

var input = """
            SELECT * FROM tableNameHere
            """;
var lexer = new Lexer(input);
var parser = new Parser(lexer);
var ast = parser.Parse();

try
{
    ast.PrettyPrint();
}
catch (Exception e)
{
    Console.WriteLine(e);
    throw;
}