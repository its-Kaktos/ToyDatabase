namespace SqlParser.PageImpl;

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