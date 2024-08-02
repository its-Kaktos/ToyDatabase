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

/*
 * From: https://github.com/postgres/postgres/blob/9fb855fe1ae04a147bd4cdaa571a1c9de5f03682/src/include/storage/block.h#L53
 * BlockId:
 *
 * this is a storage type for BlockNumber.  in other words, this type
 * is used for on-disk structures (e.g., in HeapTupleData) whereas
 * BlockNumber is the type on which calculations are performed (e.g.,
 * in access method code).
 *
 * there doesn't appear to be any reason to have separate types except
 * for the fact that BlockIds can be SHORTALIGN'd (and therefore any
 * structures that contains them, such as ItemPointerData, can also be
 * SHORTALIGN'd).  this is an important consideration for reducing the
 * space requirements of the line pointer (ItemIdData) array on each
 * page and the header of each heap or index tuple, so it doesn't seem
 * wise to change this without good reason.
 */
public record BlockIdData
{
    public BlockIdData(uint blockNumber)
    {
        // set BlockIdData fields from a BlockNumber
        BiHi = (ushort)(blockNumber >> 16);
        BiLo = (ushort)(blockNumber & 0xffff);
    }

    public ushort BiHi { get; }
    public ushort BiLo { get; }

    /// <summary>
    /// get BlockNumber from BlockIdData fields
    /// </summary>
    /// <returns>BlockNumber</returns>
    public uint GetBlockNumber()
    {
        return ((uint)BiHi << 16) | (uint)BiLo;
    }
}