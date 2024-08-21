using System.Buffers.Binary;
using SqlParser.PageImpl;

namespace SqlParser.BtreeImpl;

// These are the data from
public class BtreeDisk
{
    private const ushort PageSize = 8 * 1024;

    public long GetSizeForFile(BtreeNode root)
    {
        return GetSizeForFileInternal(root);
    }

    // 0 in files always means NULL
    public (PageHeaderData pageHeaderData, List<Cell> cells) WriteToFile(string path, BtreeNode root)
    {
        using var stream = new FileStream(path, FileMode.Create, FileAccess.Write);
        using var writer = new BinaryWriter(stream);

        var startOfCells = PageSize;
        Span<byte> content = stackalloc byte[4];
        var linePointers = new List<ItemIdData>();
        var cells = new List<Cell>();
        for (var i = 0; i < root.Keys.Count; i++)
        {
            BinaryPrimitives.WriteInt32LittleEndian(content, root.Keys[i]);

            var cell = new Cell()
            {
                Header = new CellHeader
                {
                    Size = (ushort)content.Length,
                    LeftChild = 0
                },
                Content = content.ToArray()
            };

            cells.Insert(0, cell);

            startOfCells -= cell.SizeInBytes();
            linePointers.Add(new ItemIdData(0)
            {
                Flags = ItemIdDataFlags.Normal,
                Length = cell.SizeInBytes(),
                Offset = startOfCells
            });
        }

        var lower = PageHeaderData.SizeInBytesExcludingLinp()
                    + linePointers.Count * ItemIdData.SizeInBytes();

        var upper = startOfCells;

        var pageHeaderData = new PageHeaderData
        {
            Flags = PageHeaderDataFlags.AllVisible,
            Lsn = new PageXlogRecPtr
            {
                XLogId = 1,
                XRecOff = 1
            },
            CheckSum = 1,
            Linp = linePointers,
            Lower = (ushort)lower,
            Upper = upper,
            Special = PageSize,
            PruneXid = 0,
            PageSizeAndVersion = PageSize // TODO fix?
        };

        pageHeaderData.Write(writer);

        var insertNullData = upper - writer.BaseStream.Position;
        const byte nullData = 0;
        for (var i = writer.BaseStream.Position; i < upper; i++)
        {
            writer.Write(nullData);
        }

        foreach (var cell in cells)
        {
            cell.Write(writer);
        }

        return (pageHeaderData, cells);
    }

    public Btree ReadFile(string path, int maxKeysCount, PageHeaderData eP, List<Cell> eC)
    {
        const ushort pageSize = 8 * 1024; // 8 KB

        using var streamReader = new FileStream(path, FileMode.Open, FileAccess.Read);
        using var reader = new BinaryReader(streamReader);

        Span<byte> s = stackalloc byte[pageSize];
        reader.Read(s);
        reader.BaseStream.Seek(0, SeekOrigin.Begin);

        Console.WriteLine("---------START => Binary file-----------");
        foreach (var b in s)
        {
            Console.Write(b);
        }

        Console.WriteLine();
        Console.WriteLine("+++++++++++++++END => Binary file+++++++++++");

        var (pageHeaderData, cells) = ReadPageHeaderDataAndCells(reader);

        var btree = new Btree(maxKeysCount);
        var node = new BtreeNode(maxKeysCount);

        var keys = cells.Select(x => BinaryPrimitives.ReadInt32LittleEndian(x.Content)).ToList();
        node.SetKeys(keys);
        btree.SetRoot(node);

        return btree;
    }

    private (PageHeaderData pageHeaderData, List<Cell> cells) ReadPageHeaderDataAndCells(BinaryReader reader)
    {
        var pageHeaderData = PageHeaderData.Read(reader);

        var cells = new List<Cell>();
        var bytesCount = PageSize - pageHeaderData.Upper;
        var moveTo = Math.Abs(reader.BaseStream.Position - pageHeaderData.Upper);
        reader.BaseStream.Seek(moveTo, SeekOrigin.Current);

        var readUntil = reader.BaseStream.Position + bytesCount;

        var i = reader.BaseStream.Position;
        while (i < readUntil)
        {
            var cell = Cell.Read(reader);
            cells.Insert(0, cell);

            i += cell.SizeInBytes();
        }

        return (pageHeaderData, cells);
    }

    private long GetSizeForFileInternal(BtreeNode node)
    {
        long singleKeySize = sizeof(int);

        var sumOfKeysSize = node.Keys.Count * singleKeySize;

        foreach (var child in node.Children)
        {
            sumOfKeysSize += GetSizeForFileInternal(child);
        }

        return sumOfKeysSize;
    }
}