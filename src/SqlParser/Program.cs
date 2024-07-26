// See https://www.youtube.com/watch?v=5Pc18ge9ohI&ab_channel=TonySaro
// https://ruslanspivak.com/lsbasi-part1/
// https://forcedotcom.github.io/phoenix/index.html#select_expression

using SqlParser.BtreeImpl;
using SqlParser.Extensions;


var maxKeysCount = 4;
var btree = new Btree(maxKeysCount);
var numbers = Enumerable.Range(1, 5).ToList();
foreach (var i in numbers)
{
    InsertAndPrint(btree, i);
    btree.ThrowWhenInvalidBTree(maxKeysCount);
}

DeleteAndPrint(btree, 2);

btree.StressTestRandomTreesInfinitely();

// var btreeStr = btree.ToPrettyString();
// File.WriteAllText("/home/kaktos/Desktop/" + Guid.NewGuid(), btreeStr);

btree.ThrowWhenInvalidBTree(maxKeysCount);

Console.WriteLine("Done.");

return;

void DeleteAndPrint(Btree bt, int key)
{
    Console.WriteLine("------------ Deleted key: " + key);
    bt.Delete(key);
    bt.PrettyPrint(Color.Black);
    Console.WriteLine("****************************************");
}

void InsertAndPrint(Btree bt, int key)
{
    Console.WriteLine("++++++++++++ Added key: " + key);
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