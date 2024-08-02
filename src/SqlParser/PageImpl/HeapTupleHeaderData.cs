namespace SqlParser.PageImpl;

// README: Table 73.4. HeapTupleHeaderData Layout
// Thanks to: https://fritshoogland.wordpress.com/2017/07/01/postgresql-block-internals/
// TODO Implementation is not complete
public record HeapTupleHeaderData
{
    /// <summary>
    /// t_xmin	TransactionId	4 bytes	insert XID stamp
    /// </summary>
    public int XMin { get; set; }
    
    /// <summary>
    /// t_xmax	TransactionId	4 bytes	delete XID stamp
    /// </summary>
    public int XMax { get; set; }
    
    /// <summary>
    /// t_cid	CommandId	4 bytes	insert and/or delete CID stamp (overlays with t_xvac)
    /// </summary>
    public int Cid { get; set; }
    
    /// <summary>
    /// t_xvac	TransactionId	4 bytes	XID for VACUUM operation moving a row version
    /// </summary>
    public int XVac { get; set; }
    
    /// <summary>
    /// t_ctid	ItemPointerData	6 bytes	current TID of this or newer row version
    /// </summary>
    public ItemPointerData CTID { get; set; }
    
    /// <summary>
    /// t_infomask2	uint16	2 bytes	number of attributes, plus various flag bits
    /// </summary>
    public short InfoMask2 { get; set; }
    
    /// <summary>
    /// t_infomask	uint16	2 bytes	various flag bits
    /// </summary>
    public short InfoMax { get; set; }
    
    /// <summary>
    /// t_hoff	uint8	1 byte	offset to user data
    /// </summary>
    public byte Hoff { get; set; }
}