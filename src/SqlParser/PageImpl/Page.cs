namespace SqlParser.PageImpl;

/*
 * Source: https://github.com/postgres/postgres/blob/7da1bdc2c2f17038f2ae1900be90a0d7b5e361e0/src/include/storage/bufpage.h#L25
 *
 * A postgres disk page is an abstraction layered on top of a postgres
 * disk block (which is simply a unit of i/o, see block.h).
 *
 * specifically, while a disk block can be unformatted, a postgres
 * disk page is always a slotted page of the form:
 *
 * +----------------+---------------------------------+
 * | PageHeaderData | linp1 linp2 linp3 ...           |
 * +-----------+----+---------------------------------+
 * | ... linpN |									  |
 * +-----------+--------------------------------------+
 * |		   ^ pd_lower		                      |
 * |					                              |
 * |		     v pd_upper							  |
 * +-------------+------------------------------------+
 * |			 | tupleN ...                         |
 * +-------------+------------------+-----------------+
 * |    ... tuple3 tuple2 tuple1    | "special space" |
 * +--------------------------------+-----------------+
 *		                            ^ pd_special
 *
 * a page is full when nothing can be added between pd_lower and
 * pd_upper.
 *
 * all blocks written out by an access method must be disk pages.
 *
 * EXCEPTIONS:
 *
 * obviously, a page is not formatted before it is initialized by
 * a call to PageInit.
 *
 * NOTES:
 *
 * linp1..N form an ItemId (line pointer) array.  ItemPointers point
 * to a physical block number and a logical offset (line pointer
 * number) within that block/page.  Note that OffsetNumbers
 * conventionally start at 1, not 0.
 *
 * tuple1..N are added "backwards" on the page.  Since an ItemPointer
 * offset is used to access an ItemId entry rather than an actual
 * byte-offset position, tuples can be physically shuffled on a page
 * whenever the need arises.  This indirection also keeps crash recovery
 * relatively simple, because the low-level details of page space
 * management can be controlled by standard buffer page code during
 * logging, and during recovery.
 *
 * AM-generic per-page information is kept in PageHeaderData.
 *
 * AM-specific per-page data (if any) is kept in the area marked "special
 * space"; each AM has an "opaque" structure defined somewhere that is
 * stored as the page trailer.  An access method should always
 * initialize its pages with PageInit and then set its own opaque
 * fields.
 */
public class Page
{
    public PageHeaderData PageHeaderData { get; set; }
}

/*
 * Source: https://github.com/antoniosarosi/mkdb/blob/bf1341bc4da70971fc6c340f3a5e9c6bbc55da37/src/storage/page.rs#L550
 *
 * Cell header located at the beginning of each cell.
 *
 * The header stores the size of the cell without including its own size and it
 * also stores a pointer to the BTree page the contains entries "less than"
 * this one.
 *
 * ```text
 *           HEADER                               CONTENT
 * +-----------------------+-----------------------------------------------+
 * | +------+------------+ |                                               |
 * | | size | left_child | |                                               |
 * | +------+------------+ |                                               |
 * +-----------------------+-----------------------------------------------+
 *                         ^                                               ^
 *                         |                                               |
 *                         +-----------------------------------------------+
 *                                            size bytes
 * ```
 *
 * # Alignment
 *
 * Cells are 64 bit aligned. See [`Page`] for more details.
 *
 * # Overflow
 *
 * The maximum size of a cell is defined by [`Page::ideal_max_payload_size`].
 * If a cell needs to hold more data than its maximum size allows, then we'll
 * set [`CellHeader::is_overflow`] to `true` and make the last 4 bytes of the
 * content point to an [`OverflowPage`].
 */
public class CellHeader
{
    /// <summary>
    ///  Size of the cell content.
    /// </summary>
    public ushort Size { get; set; }

    /// <summary>
    /// True if this cell points to an overflow page.
    /// Since we need to add
    /// padding to align the fields of this struct anyway, we're not wasting
    /// space here.
    /// </summary>
    public bool IsOverFlow { get; set; }

    /// <summary>
    /// Add padding manually to avoid uninitialized bytes
    /// </summary>
    public byte Padding { get; set; }

    /// <summary>
    /// Page number of the BTree page that contains values less than this cell.
    /// </summary>
    public uint LeftChild { get; set; }
}

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
}

public class PageAllocator
{
    private const int CellHeaderSize = 16; // Example size; replace with actual size
    private const int SlotSize = 8; // Example size; replace with actual size
    private const int CellAlignment = 8; // Example alignment; replace with actual alignment
    private const int MaxPageSize = 64 * 1024; // 64 KiB

    // Allocates a new page of `size` bytes in memory.
    public static PageAllocator Alloc(int size)
    {
        // Create a new instance with the given size
        return new PageAllocator(size);
    }

    // Amount of space that can be used in a page to store Cells.
    public static ushort UsableSpace(int pageSize)
    {
        return (ushort)BufferWithHeader<uint>.UsableSpace(pageSize);
    }

    // The maximum payload size that can be stored in the given usable space.
    private static ushort MaxPayloadSizeIn(ushort usableSpace)
    {
        // Calculate max payload size considering header and slot sizes, and alignment
        return (ushort)((usableSpace - CellHeaderSize - SlotSize) & ~(CellAlignment - 1));
    }

    // Maximum size that the payload of a single Cell should take on the page to allow the page to store at least `minCells`.
    public static ushort IdealMaxPayloadSize(int pageSize, int minCells)
    {
        if (minCells <= 0)
        {
            throw new ArgumentException("Number of cells must be greater than zero.", nameof(minCells));
        }

        ushort usableSpace = (ushort)(UsableSpace(pageSize) / minCells);
        ushort idealSize = MaxPayloadSizeIn(usableSpace);

        if (idealSize <= 0)
        {
            throw new InvalidOperationException($"Page size {pageSize} is too small to store {minCells} cells.");
        }

        return idealSize;
    }

    // Internal storage for the page size, just as an example
    private int PageSize;

    // Private constructor to simulate page allocation
    private PageAllocator(int size)
    {
        PageSize = size;
    }
}

// Dummy BufferWithHeader class for the example
public static class BufferWithHeader<T>
{
    public static int UsableSpace(int pageSize)
    {
        // Just an example implementation
        return pageSize - 128; // Assume some header size
    }
}