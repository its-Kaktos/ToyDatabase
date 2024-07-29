# Btree disk implementation details

## 73.6. Database Page Layout

[Source](https://www.postgresql.org/docs/16/storage-page-layout.html "Source")
[GitHub page of postgres Btree implementation](https://github.com/postgres/postgres/tree/master/src/backend/access/nbtree "GitHub page of postgres Btree implementation")

In the following explanation, a *byte* is assumed to contain 8 bits. In addition, the term *item* refers to an individual data value that is stored on a page. In a table, an item is a row; in an index, an item is an index entry.

Every table and index is stored as an array of pages of a fixed size (usually 8 kB). In indexes, the first page is generally reserved as a metapage holding control information, and there can be different types of pages within the index, depending on
the index access method.

[Table 73.2](#table-732-overall-page-layout) shows the overall layout of a page. There are five parts to each page.

#### Table 73.2. Overall Page Layout

| Item           | Description                                                                                                      |
|:---------------|:-----------------------------------------------------------------------------------------------------------------|
| PageHeaderData | 24 bytes long. Contains general information about the page, including free space pointers.                       |
| ItemIdData     | Array of item identifiers pointing to the actual items. Each entry is an (offset,length) pair. 4 bytes per item. |
| Free space     | The unallocated space. New item identifiers are allocated from the start of this area, new items from the end.   |
| Items	         | The actual items themselves.                                                                                     |
| Special space	 | Index access method specific data. Different methods store different data. Empty in ordinary tables.             |

The first 24 bytes (**16 bytes in my implementation**) of each page consists of a page header (PageHeaderData). Its format is detailed in [Table 73.3](#table-733-pageheaderdata-layout). The first field tracks the most recent WAL entry related to this page. The
second field contains the page checksum if data checksums are enabled. Next is a 2-byte field containing flag bits. This is followed by three 2-byte integer fields (pd_lower, pd_upper, and pd_special). These contain byte offsets from the page start
to the start of unallocated space, to the end of unallocated space, and to the start of the special space. The next 2 bytes of the page header, pd_pagesize_version, store both the page size and a version indicator. Beginning with PostgreSQL 8.3 the
version number is 4; PostgreSQL 8.1 and 8.2 used version number 3; PostgreSQL 8.0 used version number 2; PostgreSQL 7.3 and 7.4 used version number 1; prior releases used version number 0. (The basic page layout and header format has not changed in
most of these versions, but the layout of heap row headers has.) The page size is basically only present as a cross-check; there is no support for having more than one page size in an installation. The last field is a hint that shows whether pruning
the page is likely to be profitable: it tracks the oldest un-pruned XMAX on the page.

**Note:** I have removed *pd_checksum* and *pd_pagesize_version* and *pd_prune_xid* because in the implementation of my simple database, we dont need them.
So with my changes the header section is 16 bytes.

#### Table 73.3. PageHeaderData Layout

| Field      | Type           | Length  | Description                                                                                                                                                                                   |
|:-----------|:---------------|:--------|:----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| pd_lsn     | PageXLogRecPtr | 8 bytes | LSN: next byte after last byte of WAL record for last change to this page (IDK what this is, but lets keep it for now incase we need it. we can always remove it later if we dont need this.) |
| pd_flags   | uint16         | 2 bytes | Flag bits                                                                                                                                                                                     |
| pd_lower   | LocationIndex  | 2 bytes | Offset to start of free space                                                                                                                                                                 |
| pd_upper   | LocationIndex  | 2 bytes | Offset to end of free space                                                                                                                                                                   |
| pd_special | LocationIndex  | 2 bytes | Offset to start of special space                                                                                                                                                              |

All the details can be found in [src/include/storage/bufpage.h](https://github.com/postgres/postgres/blob/master/src/include/storage/bufpage.h).

Following the page header are item identifiers (ItemIdData), each requiring four bytes. An item identifier contains a byte-offset to the start of an item, its length in bytes, and a few attribute bits which affect its interpretation. New item identifiers are allocated as needed from the beginning of the unallocated space. The number of item identifiers present can be determined by looking at pd_lower, which is increased to allocate a new identifier. Because an item identifier is never moved until it is freed, its index can be used on a long-term basis to reference an item, even when the item itself is moved around on the page to compact free space. In fact, every pointer to an item (ItemPointer, also known as CTID) created by PostgreSQL consists of a page number and the index of an item identifier.

The items themselves are stored in space allocated backwards from the end of unallocated space. The exact structure varies depending on what the table is to contain. Tables and sequences both use a structure named HeapTupleHeaderData, described below.

The final section is the “special section” which can contain anything the access method wishes to store. For example, b-tree indexes store links to the page's left and right siblings, as well as some other data relevant to the index structure. Ordinary tables do not use a special section at all (indicated by setting pd_special to equal the page size).

[Figure 73.1](#figure-731-page-layout) illustrates how these parts are laid out in a page.

#### Figure 73.1. Page Layout

![Figure 73.1.](./Images/Figure-73.1.png "Figure 73.1.")
