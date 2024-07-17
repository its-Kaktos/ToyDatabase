// See https://www.youtube.com/watch?v=5Pc18ge9ohI&ab_channel=TonySaro
// https://ruslanspivak.com/lsbasi-part1/
// https://forcedotcom.github.io/phoenix/index.html#select_expression

using SqlParser;
using SqlParser.BtreeImpl;

// var input = """
//             SELECT * FROM tableNameHere
//             """;
// var lexer = new Lexer(input);
// var parser = new Parser(lexer);
// var ast = parser.Parse();
// try
// {
//     ast.PrettyPrint();
//     
// }
// catch (Exception e)
// {
//     Console.WriteLine(e);
//     throw;
// }

var leftLeaf = new BtreeNode()
{
    Keys = [1]
};

var middleLeaf = new BtreeNode()
{
    Keys = [2]
};

var rightLeaf = new BtreeNode()
{
    Keys = [3, 4]
};

var root = new BtreeNode()
{
    Keys = [2, 2],
    Child = [leftLeaf, middleLeaf, rightLeaf]
};

var btree = new Btree()
{
    Root = root
};

btree.PrettyPrint();