namespace SqlParser.PageImpl;

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