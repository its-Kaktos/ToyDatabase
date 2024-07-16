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