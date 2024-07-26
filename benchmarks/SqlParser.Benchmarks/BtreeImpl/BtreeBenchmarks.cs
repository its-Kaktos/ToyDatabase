using BenchmarkDotNet.Attributes;
using SqlParser.BtreeImpl;

namespace SqlParser.Benchmarks.BtreeImpl;

[MemoryDiagnoser]
public class BtreeBenchmarks
{
    private const int MaxChildCount = 32;
    private const int BenchmarkTreeSize = 10_000;
    // private int _i = 0;
    // private int _size = 0;
    private List<int> _itemsToAdd;
    private Btree _btree;

    [GlobalSetup(Target = nameof(BenchmarkInsert))]
    public void SetupBenchmarkInsert()
    {
        _itemsToAdd = Random.Shared.Perm(BenchmarkTreeSize).ToList();
    }

    [Benchmark]
    public Btree BenchmarkInsert()
    {
        var tr = new Btree(MaxChildCount);
        foreach (var item in _itemsToAdd)
        {
            tr.Add(item);
        }

        return tr;
    }

    // private void InitBenchmark(int size)
    // {
    //     _itemsToAdd = Random.Shared.Perm(size).ToList();
    //     _btree = new Btree(MaxChildCount);
    //     foreach (var item in _itemsToAdd)
    //     {
    //         _btree.Insert(item);
    //     }
    //
    //     _i = 0;
    // }
    //
    // [GlobalSetup(Target = nameof(BenchmarkSeek))]
    // public void SetupBenchmarkSeek()
    // {
    //     _size = 100000;
    //     InitBenchmark(_size);
    // }
    //
    // [Benchmark]
    // public int BenchmarkSeek()
    // {
    //     _tr.AscendGreaterOrEqual(new Int(_i % _size), (Int i) => { return false; });
    //     return _i++;
    // }
    //
    // [GlobalSetup(Target = nameof(BenchmarkDeleteInsert))]
    // public void SetupBenchmarkDeleteInsert()
    // {
    //     InitBenchmark(BenchmarkTreeSize);
    // }
    //
    // [Benchmark]
    // public int BenchmarkDeleteInsert()
    // {
    //     _tr.Delete(_insertP[_i % BenchmarkTreeSize]);
    //     _tr.ReplaceOrInsert(_insertP[_i % BenchmarkTreeSize]);
    //     return _i++;
    // }
    //
    // [GlobalSetup(Target = nameof(BenchmarkDeleteInsertCloneOnce))]
    // public void SetupBenchmarkDeleteInsertCloneOnce()
    // {
    //     InitBenchmark(BenchmarkTreeSize);
    //     _tr = _tr.Clone();
    // }
    //
    // [Benchmark]
    // public int BenchmarkDeleteInsertCloneOnce()
    // {
    //     _tr.Delete(_insertP[_i % BenchmarkTreeSize]);
    //     _tr.ReplaceOrInsert(_insertP[_i % BenchmarkTreeSize]);
    //     return _i++;
    // }
    //
    // [GlobalSetup(Target = nameof(BenchmarkDeleteInsertCloneEachTime))]
    // public void SetupBenchmarkDeleteInsertCloneEachTime()
    // {
    //     InitBenchmark(BenchmarkTreeSize);
    // }
    //
    // [Benchmark]
    // public int BenchmarkDeleteInsertCloneEachTime()
    // {
    //     _tr = _tr.Clone();
    //     _tr.Delete(_insertP[_i % BenchmarkTreeSize]);
    //     _tr.ReplaceOrInsert(_insertP[_i % BenchmarkTreeSize]);
    //     return _i++;
    // }
}