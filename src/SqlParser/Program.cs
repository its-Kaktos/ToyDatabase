// See https://www.youtube.com/watch?v=5Pc18ge9ohI&ab_channel=TonySaro


using SqlParser;

var sql = "1 + 5";

var sqlParser = new QueryParser();

var tokens = sqlParser.Tokenizer(sql);

Console.WriteLine("Hello, World!");