namespace SqlParser.PageImpl;

/*
 * Source: https://github.com/postgres/postgres/blob/7da1bdc2c2f17038f2ae1900be90a0d7b5e361e0/src/include/storage/bufpage.h#L112
 * Note: for some reason PSQL dealered a type for only single values.
 * for example the Lower type is `LocationIndex` which is just a uint16.
 * I have no idea why this is.
 *
 * disk page organization
 *
 * space management information generic to any page
 *
 *		pd_lsn		- identifies xlog record for last change to this page.
 *		pd_checksum - page checksum, if set.
 *		pd_flags	- flag bits.
 *		pd_lower	- offset to start of free space.
 *		pd_upper	- offset to end of free space.
 *		pd_special	- offset to start of special space.
 *		pd_pagesize_version - size in bytes and page layout version number.
 *		pd_prune_xid - oldest XID among potentially prunable tuples on page.
 *
 * The LSN is used by the buffer manager to enforce the basic rule of WAL:
 * "thou shalt write xlog before data".  A dirty buffer cannot be dumped
 * to disk until xlog has been flushed at least as far as the page's LSN.
 *
 * pd_checksum stores the page checksum, if it has been set for this page;
 * zero is a valid value for a checksum. If a checksum is not in use then
 * we leave the field unset. This will typically mean the field is zero
 * though non-zero values may also be present if databases have been
 * pg_upgraded from releases prior to 9.3, when the same byte offset was
 * used to store the current timelineid when the page was last updated.
 * Note that there is no indication on a page as to whether the checksum
 * is valid or not, a deliberate design choice which avoids the problem
 * of relying on the page contents to decide whether to verify it. Hence
 * there are no flag bits relating to checksums.
 *
 * pd_prune_xid is a hint field that helps determine whether pruning will be
 * useful.  It is currently unused in index pages.
 *
 * The page version number and page size are packed together into a single
 * uint16 field.  This is for historical reasons: before PostgreSQL 7.3,
 * there was no concept of a page version number, and doing it this way
 * lets us pretend that pre-7.3 databases have page version number zero.
 * We constrain page sizes to be multiples of 256, leaving the low eight
 * bits available for a version number.
 *
 * Minimum possible page size is perhaps 64B to fit page header, opaque space
 * and a minimal tuple; of course, in reality you want it much bigger, so
 * the constraint on pagesize mod 256 is not an important restriction.
 * On the high end, we can only support pages up to 32KB because lp_off/lp_len
 * are 15 bits.
 */
public class PageHeaderData
{
    /// <summary>
    /// XXX LSN is member of *any* block, not only page-organized ones.
    /// LSN: next byte after last byte of xlog record for last change to this page.
    /// </summary>
    public PageXlogRecPtr Lsn { get; set; }

    /// <summary>
    /// Check sum
    /// </summary>
    public ushort CheckSum { get; set; }

    /// <summary>
    /// Flag bits.
    /// </summary>
    public PageHeaderDataFlags Flags { get; set; }

    /// <summary>
    /// offset to start of free space
    /// </summary>
    public ushort Lower { get; set; }

    /// <summary>
    /// offset to end of free space
    /// </summary>
    public ushort Upper { get; set; }

    /// <summary>
    /// offset to start of special space
    /// </summary>
    public ushort Special { get; set; }

    /// <summary>
    /// This field can be seperated into two fields.
    /// There is nothing stopping this implementation to separate
    /// Page size from version.
    /// </summary>
    public ushort PageSizeAndVersion { get; set; }

    /// <summary>
    /// oldest prunable XID, or zero if none
    /// </summary>
    public uint PruneXid { get; set; }

    /// <summary>
    /// line pointer array
    /// </summary>
    public List<ItemIdData> Linp { get; set; }
}

/*
 *Source: https://github.com/postgres/postgres/blob/7da1bdc2c2f17038f2ae1900be90a0d7b5e361e0/src/include/storage/bufpage.h#L187
 *
 * pd_flags contains the following flag bits.  Undefined bits are initialized
 * to zero and may be used in the future.
 *
 * PD_HAS_FREE_LINES is set if there are any LP_UNUSED line pointers before
 * pd_lower.  This should be considered a hint rather than the truth, since
 * changes to it are not WAL-logged.
 *
 * PD_PAGE_FULL is set if an UPDATE doesn't find enough free space in the
 * page for its new tuple version; this suggests that a prune is needed.
 * Again, this is just a hint.
 */
[Flags]
public enum PageHeaderDataFlags : ushort
{
    /// <summary>
    /// are there any unused line pointers?
    /// </summary>
    HasFreeLines = 1,

    /// <summary>
    /// not enough free space for new tuple?
    /// </summary>
    PageFull = 2,

    /// <summary>
    /// ll tuples on page are visible to everyone
    /// </summary>
    AllVisible = 4
}

/*
 * Source: https:/|/github.com/postgres/postgres/blob/7da1bdc2c2f17038f2ae1900be90a0d7b5e361e0/src/include/storage/bufpage.h#L97
 *  For historical reasons, the 64-bit LSN value is stored as two 32-bit
 * values.
 * Note: there is nothing stopping me to split these apart.
 */
public class PageXlogRecPtr
{
    /// <summary>
    /// High bits
    /// </summary>
    public uint XLogId { get; set; }

    /// <summary>
    /// Low bits
    /// </summary>
    public uint XRecOff { get; set; }
}

/*
 * Source: https://github.com/postgres/postgres/blob/b919a97a6cd204cbd9b77d12c9e60ad59eea04a4/src/include/storage/itemid.h#L25
 *
 * A line pointer on a buffer page.  See buffer page definitions and comments
 * for an explanation of how line pointers are used.
 *
 * In some cases a line pointer is "in use" but does not have any associated
 * storage on the page.  By convention, lp_len == 0 in every line pointer
 * that does not have storage, independently of its lp_flags state.
 */
public class ItemIdData
{
    private int _value;

    public ItemIdData(int value)
    {
        _value = value;
    }

    // Property to get and set the offset (15 bits)
    public ushort Offset
    {
        get => (ushort)(_value & 0x7FFF); // Extract 15 bits
        set => _value = (_value & ~0x7FFF) | (value & 0x7FFF); // Set 15 bits
    }

    // Property to get and set the flags (2 bits)
    public ItemIdDataFlags Flags
    {
        get => (ItemIdDataFlags)((_value >> 15) & 0x03); // Extract 2 bits
        set => _value = (_value & ~0x6000) | (((ushort)value & 0x03) << 15); // Set 2 bits
    }

    // Property to get and set the length (15 bits)
    public ushort Length
    {
        get => (ushort)((_value >> 17) & 0x7FFF); // Extract 15 bits
        set => _value = (_value & ~0x7FFF0000) | ((value & 0x7FFF) << 17); // Set 15 bits
    }

    // Method to combine bit fields into an int
    public int ToInt32()
    {
        return _value;
    }

    public static ItemIdData FromInt32(int value)
    {
        return new ItemIdData(value);
    }

    // Method to display the fields (for testing)
    public override string ToString()
    {
        return $"Offset: {Offset}, Flags: {Flags}, Length: {Length}";
    }
}

/*
 * Source: https://github.com/postgres/postgres/blob/b919a97a6cd204cbd9b77d12c9e60ad59eea04a4/src/include/storage/itemid.h#L38
 *
 * lp_flags has these possible states.  An UNUSED line pointer is available
 * for immediate re-use, the other states are not.
 */
[Flags]
public enum ItemIdDataFlags : ushort
{
    /// <summary>
    /// unused (should always have lp_len=0)
    /// </summary>
    Unused = 1,

    /// <summary>
    /// used (should always have lp_len>0)
    /// </summary>
    Normal = 2,

    /// <summary>
    /// HOT redirect (should have lp_len=0)
    /// </summary>
    Redirect = 4,

    /// <summary>
    /// dead, may or may not have storage
    /// </summary>
    Dead = 8
}