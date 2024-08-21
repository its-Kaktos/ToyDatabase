namespace SqlParser.PageImpl;

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

    public static ushort SizeInBytes()
    {
        const ushort size = sizeof(uint) + sizeof(uint);

        return size;
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write(XLogId);
        writer.Write(XRecOff);
    }

    public static PageXlogRecPtr Read(BinaryReader reader)
    {
        return new PageXlogRecPtr
        {
            XLogId = reader.ReadUInt32(),
            XRecOff = reader.ReadUInt32()
        };
    }
}