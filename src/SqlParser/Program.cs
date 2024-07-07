// See https://www.youtube.com/watch?v=5Pc18ge9ohI&ab_channel=TonySaro
// https://ruslanspivak.com/lsbasi-part1/

using SqlParser;

// var sql = "123456 + 555.9826 - 11123.234";
// var sql = "123456+555.9826-11123.234";

// var x = Lexer.Tokenize(sql).ToList();

// var x = Lexer.(sql);
// var input = "123+1 - 523 / 234 * 234  ";
// var input = "123 + 1  - 4";
var input = "123+1";

var interpreter = new Interpreter(input);

// var x = new List<Token2>();
// while (true)
// {
//     var currentToken = interpreter.GetNextToken();
//     x.Add(currentToken);
//     if(currentToken == Token2.EofToken) break;
// }

Console.WriteLine(interpreter.Evaluate());
Console.WriteLine("xds");