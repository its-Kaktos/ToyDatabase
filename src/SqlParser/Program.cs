// See https://www.youtube.com/watch?v=5Pc18ge9ohI&ab_channel=TonySaro
// https://ruslanspivak.com/lsbasi-part1/
// https://forcedotcom.github.io/phoenix/index.html#select_expression

using SqlParser;
using SqlParser.BtreeImpl;
using SqlParser.Extensions;

var maxKeysCount = 4098;
var btree = new Btree(maxKeysCount);

foreach (var i in Enumerable.Range(1, 1_000_000))
{
    btree.Insert(i);
}

// var btreeStr = btree.ToPrettyString();
// File.WriteAllText("/home/kaktos/Desktop/" + Guid.NewGuid(), btreeStr);

btree.ThrowWhenInvalidBTree(maxKeysCount);

Console.WriteLine("Done.");

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