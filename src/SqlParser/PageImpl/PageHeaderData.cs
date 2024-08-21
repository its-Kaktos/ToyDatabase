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

    public static int SizeInBytesExcludingLinp()
    {
        var size = PageXlogRecPtr.SizeInBytes() // Lsn
                   + sizeof(ushort) // CheckSum
                   + sizeof(PageHeaderDataFlags) // Flags
                   + sizeof(ushort) // Lower
                   + sizeof(ushort) // Upper
                   + sizeof(ushort) // Special
                   + sizeof(ushort) // PageSizeAndVersion
                   + sizeof(uint); // PruneXid

        return size;
    }

    public void Write(BinaryWriter writer)
    {
        Lsn.Write(writer);
        writer.Write(CheckSum);
        writer.Write((ushort)Flags);
        writer.Write(Lower);
        writer.Write(Upper);
        writer.Write(Special);
        writer.Write(PageSizeAndVersion);
        writer.Write(PruneXid);

        foreach (var itemIdData in Linp)
        {
            itemIdData.Write(writer);
        }
    }

    public static PageHeaderData Read(BinaryReader reader)
    {
        var output = new PageHeaderData
        {
            Lsn = PageXlogRecPtr.Read(reader),
            CheckSum = reader.ReadUInt16(),
            Flags = (PageHeaderDataFlags)reader.ReadUInt16(),
            Lower = reader.ReadUInt16(),
            Upper = reader.ReadUInt16(),
            Special = reader.ReadUInt16(),
            PageSizeAndVersion = reader.ReadUInt16(),
            PruneXid = reader.ReadUInt32()
        };

        var linp = new List<ItemIdData>();
        var endOfLinp = output.Lower - PageHeaderData.SizeInBytesExcludingLinp();
        var readUntil = reader.BaseStream.Position + endOfLinp;

        var i = reader.BaseStream.Position;
        while (i < readUntil)
        {
            linp.Add(ItemIdData.Read(reader));

            i += ItemIdData.SizeInBytes();
        }

        output.Linp = linp;

        return output;
    }
}