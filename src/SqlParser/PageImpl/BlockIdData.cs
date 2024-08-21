namespace SqlParser.PageImpl;

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