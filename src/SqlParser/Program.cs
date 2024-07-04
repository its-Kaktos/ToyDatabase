// See https://www.youtube.com/watch?v=5Pc18ge9ohI&ab_channel=TonySaro


using SqlParser;

// var sql = "123456 + 555.9826 - 11123.234";
var sql = "123456+555.9826-11123.234";

var x = Lexer.Tokenize(sql).ToList();

Console.WriteLine("Hello, World!");