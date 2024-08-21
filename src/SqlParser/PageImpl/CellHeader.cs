namespace SqlParser.PageImpl;

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
    /// Page number of the BTree page that contains values less than this cell.
    /// </summary>
    public uint LeftChild { get; set; }

    public ushort SizeInBytes()
    {
        const int size = sizeof(ushort) + sizeof(uint);

        return size;
    }
}