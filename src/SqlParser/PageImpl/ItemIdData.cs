namespace SqlParser.PageImpl;

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

    /// <summary>
    /// Creates a new ItemIdData with the value of 0
    /// </summary>
    public ItemIdData()
    {
        _value = 0;
    }

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

    public static ushort SizeInBytes()
    {
        return sizeof(int);
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write(_value);
    }

    public static ItemIdData Read(BinaryReader reader)
    {
        return FromInt32(reader.ReadInt32());
    }
}