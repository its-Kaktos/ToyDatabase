using System.Collections;
using System.Text;

namespace SqlParser.BtreeImpl;

// These are the data from
public class BtreeDisk
{
    public long GetSizeForFile(BtreeNode root)
    {
        return GetSizeForFileInternal(root);
    }

    // 0 in files always means NULL
    public void WriteToFile(string path, BtreeNode root)
    {
        var intAsBinary = Convert.ToString(99_823, 2);
        var test = Convert.ToString(int.MaxValue, 2);
        var test2 = Convert.ToString(int.MinValue, 2);

        using var stream = new FileStream(path, FileMode.Create, FileAccess.Write);
        using var writer = new BinaryWriter(stream);

        // for parent node of root is null
        writer.Write(0);

        var lastChildOffset = root.Children.Count - 1;
        writer.Write(lastChildOffset);

        for (var i = 0; i < root.Keys.Count; i++)
        {
            // child pointer
            writer.Write(i);
            writer.Write(root.Keys[i]);
        }

        intAsBinary = Convert.ToString(99_823, 2);

        // The page header contains metadata about the page. 
        void WritePageHeader()
        {
            // TODO Add parent pointer as well?

            // | Page Header (24 bytes) |
            // | pd_linp | pd_special | pd_pagesize_version | pd_lower | pd_upper | pd_flags | pd_level | pd_fillfactor |

            // pd_linp: Offset of item pointers (2 bytes).
            writer.Write((short)0);

            // pd_special: Offset of the special space area (2 bytes).
            writer.Write((short)0);

            // pd_pagesize_version: Page size and version information (2 bytes).
            writer.Write((short)0);

            // pd_lower: Offset to the start of the item pointers (2 bytes).
            writer.Write((short)0);

            // pd_upper: Offset to the end of the free space (2 bytes).
            writer.Write((short)0);

            // pd_flags: Flags indicating page type (1 byte).
            writer.Write((short)0);

            // pd_level: Level of the B-tree (1 byte, for internal nodes).
            writer.Write((short)0);

            // pd_fillfactor: Fill factor for the page (1 byte).
            writer.Write((short)0);
        }
        
        // Item pointers point to the actual data (index tuples) or to child pages in the case of internal nodes.
        // Each item pointer is a short integer (2 bytes) representing the offset of the tuple within the page.
        // Size: 4 bytes per item pointer (2 bytes for offset, 2 bytes for additional information).
        void WriteItemPointer()
        {
            // | Item Pointer 1 (4 bytes) | Item Pointer 2 (4 bytes) | ... |

            // pd_linp: Offset of item pointers (2 bytes).
            writer.Write((short)0);
        }
    }

    public void ReadFile(string path)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
        using var reader = new BinaryReader(stream);

        var buffer = new byte[16];
        stream.Read(buffer);
        var bitArray = new BitArray(buffer);

        var x111 = buffer.Select(Convert.ToChar).ToArray();
        var x23423 = new string(x111);
        foreach (var b in buffer)
        {
            Console.Write(b);
        }

        Console.WriteLine();
        Console.WriteLine("bewlo is bit");

        foreach (var b in bitArray)
        {
            Console.Write((bool)b ? "1" : "0");
        }

        Console.WriteLine(x23423);
        stream.Seek(0, SeekOrigin.Begin);

        // 
        var parentPointer = reader.ReadInt32();
        var lastChildPointer = reader.ReadInt32();

        var keyChild = new List<(int, int)>();
        while (stream.Position < stream.Length)
        {
            keyChild.Add((reader.ReadInt32(), reader.ReadInt32()));
        }

        var sb = new StringBuilder();
        sb.Append($"parentPointer: {parentPointer}, lastChildPointer: {lastChildPointer},");
        sb.Append(" keys: { ");
        foreach (var valueTuple in keyChild)
        {
            sb.Append($"(children pointer: {valueTuple.Item1}, key: {valueTuple.Item2}), ");
        }

        sb.Append(" }");

        Console.WriteLine(sb.ToString());
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