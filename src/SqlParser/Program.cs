// See https://www.youtube.com/watch?v=5Pc18ge9ohI&ab_channel=TonySaro
// https://ruslanspivak.com/lsbasi-part1/
// https://forcedotcom.github.io/phoenix/index.html#select_expression

using System.Text.Json;
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
var maxKeysCount = Random.Shared.Next(3, 6);
var btree = new Btree(maxKeysCount);

var keysAddedInOrder = new List<int>();
var max = Random.Shared.Next(10, 50);
for (int i = 0; i < max; i++)
{
    var key = Random.Shared.Next(0, 100);
    keysAddedInOrder.Add(key);
    InsertAndPrint(btree, key);
}

Console.WriteLine(JsonSerializer.Serialize(btree));
Console.WriteLine();
Console.WriteLine("MAX KEYS " + maxKeysCount);
Console.WriteLine();
Console.WriteLine(JsonSerializer.Serialize(keysAddedInOrder));


// TODO Add unit tests, and a method to check B-tree is valid,
// TODO using the rules provided on wikipedia and other websites.
return;

void DeleteAndPrint(Btree bt, int key)
{
    Console.WriteLine("------------ Deleting key: " + key);
    bt.Delete(key);
    bt.PrettyPrint(Color.Black);
    Console.WriteLine("****************************************");
}

void InsertAndPrint(Btree bt, int key)
{
    Console.WriteLine("++++++++++++ Adding key: " + key);
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