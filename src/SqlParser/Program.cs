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

// var leftLeaf = new BtreeNode()
// {
//     Keys = [1]
// };
//
// var middleLeaf = new BtreeNode()
// {
//     Keys = [2]
// };
//
// var rightLeaf = new BtreeNode()
// {
//     Keys = [3, 4]
// };
//
// var root = new BtreeNode()
// {
//     Keys = [2, 2],
//     Child = [leftLeaf, middleLeaf, rightLeaf]
// };
//
// var btree = new Btree()
// {
//     Root = root
// };

// var btree = CreateDefaultBtree();
var btree = new Btree(3);

foreach (var i in Enumerable.Range(1, 25))
{
    InsertAndPrint(btree, i);
}

// InsertAndPrint(btree, 1);
// InsertAndPrint(btree, 2);
// InsertAndPrint(btree, 3);
// InsertAndPrint(btree, 4);
// InsertAndPrint(btree, 5);


return;

void InsertAndPrint(Btree bt, int key)
{
    Console.WriteLine("Adding key: " + key);
    bt.Insert(key);
    bt.PrettyPrint(Color.Black);
    Console.WriteLine("****************************************");
}

// Btree CreateDefaultBtree()
// {
//     const int maxKeys = 4;
//     return new Btree(maxKeys)
//     {
//         Root = new BtreeNode(maxKeys)
//         {
//             Keys = [4],
//             Child =
//             [
//                 new BtreeNode(maxKeys)
//                 {
//                     Keys = [2],
//                     Child =
//                     [
//                         new BtreeNode(maxKeys) { Keys = [1] },
//                         new BtreeNode(maxKeys) { Keys = [3, 4] },
//                     ]
//                 },
//                 new BtreeNode(maxKeys)
//                 {
//                     Keys = [5, 6],
//                     Child =
//                     [
//                         new BtreeNode(maxKeys) { Keys = [5] },
//                         new BtreeNode(maxKeys) { Keys = [5, 6] },
//                         new BtreeNode(maxKeys) { Keys = [7, 8] },
//                     ]
//                 },
//             ]
//         }
//     };
// }