namespace SqlParser.PageImpl;

/*
 * Inspiration: https://github.com/postgres/postgres/blob/master/src/include/storage/itemptr.h
 * Comments are from the link above.
 * ItemPointer:
 *
 * This is a pointer to an item within a disk page of a known file
 * (for example, a cross-link from an index to its parent table).
 * ip_blkid tells us which block, ip_posid tells us which entry in
 * the linp (ItemIdData) array we want.
 *
 * Note: because there is an item pointer in each tuple header and index
 * tuple header on disk, it's very important not to waste space with
 * structure padding bytes.  The struct is designed to be six bytes long
 * (it contains three int16 fields) but a few compilers will pad it to
 * eight bytes unless coerced.  We apply appropriate persuasion where
 * possible.  If your compiler can't be made to play along, you'll waste
 * lots of space.
 */
public record ItemPointerData
{
    public required BlockIdData BlockIdData { get; set; }

    /*
     * OffsetNumber:
     *
     * this is a 1-based index into the linp (ItemIdData) array in the
     * header of each disk page.
     */
    public required ushort OffsetNumber { get; set; }
}