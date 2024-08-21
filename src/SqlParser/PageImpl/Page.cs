namespace SqlParser.PageImpl;

/*
 * Source: https://github.com/postgres/postgres/blob/7da1bdc2c2f17038f2ae1900be90a0d7b5e361e0/src/include/storage/bufpage.h#L25
 *
 * A postgres disk page is an abstraction layered on top of a postgres
 * disk block (which is simply a unit of i/o, see block.h).
 *
 * specifically, while a disk block can be unformatted, a postgres
 * disk page is always a slotted page of the form:
 *
 * +----------------+---------------------------------+
 * | PageHeaderData | linp1 linp2 linp3 ...           |
 * +-----------+----+---------------------------------+
 * | ... linpN |									  |
 * +-----------+--------------------------------------+
 * |		   ^ pd_lower		                      |
 * |					                              |
 * |		     v pd_upper							  |
 * +-------------+------------------------------------+
 * |			 | tupleN ...                         |
 * +-------------+------------------+-----------------+
 * |    ... tuple3 tuple2 tuple1    | "special space" |
 * +--------------------------------+-----------------+
 *		                            ^ pd_special
 *
 * a page is full when nothing can be added between pd_lower and
 * pd_upper.
 *
 * all blocks written out by an access method must be disk pages.
 *
 * EXCEPTIONS:
 *
 * obviously, a page is not formatted before it is initialized by
 * a call to PageInit.
 *
 * NOTES:
 *
 * linp1..N form an ItemId (line pointer) array.  ItemPointers point
 * to a physical block number and a logical offset (line pointer
 * number) within that block/page.  Note that OffsetNumbers
 * conventionally start at 1, not 0.
 *
 * tuple1..N are added "backwards" on the page.  Since an ItemPointer
 * offset is used to access an ItemId entry rather than an actual
 * byte-offset position, tuples can be physically shuffled on a page
 * whenever the need arises.  This indirection also keeps crash recovery
 * relatively simple, because the low-level details of page space
 * management can be controlled by standard buffer page code during
 * logging, and during recovery.
 *
 * AM-generic per-page information is kept in PageHeaderData.
 *
 * AM-specific per-page data (if any) is kept in the area marked "special
 * space"; each AM has an "opaque" structure defined somewhere that is
 * stored as the page trailer.  An access method should always
 * initialize its pages with PageInit and then set its own opaque
 * fields.
 */
public class BtreePage
{
    public PageHeaderData PageHeaderData { get; set; }
}

// Dummy BufferWithHeader class for the example