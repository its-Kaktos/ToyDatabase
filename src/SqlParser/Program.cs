// See https://www.youtube.com/watch?v=5Pc18ge9ohI&ab_channel=TonySaro
// https://ruslanspivak.com/lsbasi-part1/

using SqlParser;

// var sql = "123456 + 555.9826 - 11123.234";
// var sql = "123456+555.9826-11123.234";

// var x = Lexer.Tokenize(sql).ToList();

// var x = Lexer.(sql);
// var input = "123+1 - 523 / 234 * 234  ";
// var input = "123+1";

// var input = "100 + 200 - 50 + 50";
// var input = "100 / (10 * 20";
// var input = "100 / 10 * 20";
var input = "--100 / (10 * 20) + 10 ^ (5 * -3 ^ -10) + 10 * -(3 - (1 + (123 - 64)))";
// var input = "1 + -----2";
var lexer = new Lexer(input);
var parser = new Parser(lexer);
var interpreter = new Interpreter(parser);


try
{
    // parser.Parse().PrettyPrint();
    Console.WriteLine(interpreter.Evaluate());
    // Console.WriteLine("xds");
}
catch (Exception e)
{
    Console.WriteLine(e);
    throw;
}