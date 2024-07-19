using System.Net.Http.Headers;

namespace SqlParser.BtreeImpl;

public class Btree
{
    public Btree(BtreeNode root)
    {
        Root = root;
    }

    public BtreeNode Root { get; set; }

    /// <summary>
    /// Searches the btree for the <paramref name="value"/>.
    /// </summary>
    /// <param name="value">Value to search for.</param>
    /// <returns>The value if btree contains the <paramref name="value"/>, else <c>null</c></returns>
    public int? Search(int value)
    {
        return Search(value, Root);
    }

    // TODO Use binary search? https://en.wikipedia.org/wiki/B-tree#Search
    private int? Search(int value, BtreeNode node)
    {
        for (var i = 0; i < node.Keys.Count; i++)
        {
            if (value > node.Keys[i]) continue;
            if (value == node.Keys[i]) return node.Keys[i];

            // Value is less than current key, and there is no
            // child to search for the Value in its Keys.
            if (node.IsLeaf) return -1;

            // Value is less that current key, search its child.
            return Search(value, node.Child[i]);
        }

        // Value is greater than all keys, and there is no
        // child to search for the Value in its Keys.
        if (node.IsLeaf) return -1;

        // Value is greater than all keys, search right most child
        return Search(value, node.Child[node.Keys.Count]);
    }
}