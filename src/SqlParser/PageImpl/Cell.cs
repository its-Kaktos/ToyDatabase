namespace SqlParser.PageImpl;

/// <summary>
/// Source: https://github.com/antoniosarosi/mkdb/blob/bf1341bc4da70971fc6c340f3a5e9c6bbc55da37/src/storage/page.rs#L615
/// 
/// A cell is a structure that stores a single BTree entry.
///
/// Each cell stores the binary entry (AKA payload or key) and a pointer to the
/// BTree node that contains cells with keys smaller than that stored in the
/// cell itself.
///
/// The [`super::BTree`] structure reorders cells around different sibling pages
/// when an overflow or underflow occurs, so instead of hiding the low level
/// details we provide some API that can be used by upper levels.
///
/// # About DSTs
///
/// Note that this struct is a DST (Dynamically Sized Type), which is hard to
/// construct in Rust and is considered a "half-baked feature" (as of march 2024
/// at least, check the [nomicon]). Not that half-baked features are a problem,
/// we're using nightly and `#![feature()]` everywhere to see the latest and
/// greatest of Rust, but it would be nice to have a standard way of building
/// DSTs.
/// </summary>
public class Cell
{
    /// <summary>
    /// Cell header.
    /// </summary>
    public CellHeader Header { get; set; }

    /// <summary>
    /// Cell content.
    /// If Header.IsOverFlow is true, then the last 4 bytes of this array
    /// should point to an overflow page.
    /// </summary>
    public byte[] Content { get; set; }

    // Override ToString for debugging
    public override string ToString()
    {
        return $"Header: {Header}, Content Length: {Content.Length}";
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write(Header.Size);
        writer.Write(Header.LeftChild);
        writer.Write(Content);
    }

    public static Cell Read(BinaryReader reader)
    {
        var size = reader.ReadUInt16();
        var leftChild = reader.ReadUInt32();
        var content = reader.ReadBytes(size);

        return new Cell
        {
            Header = new CellHeader
            {
                Size = size,
                LeftChild = leftChild
            },
            Content = content
        };
    }

    public ushort SizeInBytes()
    {
        var size = Header.SizeInBytes() + Content.Length;
        
        return (ushort)size;
    }
}