using System.Net.Http.Headers;

namespace SqlParser.BtreeImpl;

public class Btree
{
    private readonly int _maxNumberOfKeys;

    public Btree(int maxNumberOfKeys)
    {
        _maxNumberOfKeys = maxNumberOfKeys;
        Root = new BtreeNode(_maxNumberOfKeys);
    }

    public BtreeNode Root { get; private set; }

    /// <summary>
    /// Searches the btree for the <paramref name="value"/>.
    /// </summary>
    /// <param name="value">Value to search for.</param>
    /// <returns>The value if btree contains the <paramref name="value"/>, else <c>null</c></returns>
    public int? Search(int value)
    {
        return Search(value, Root);
    }

    public void Insert(int value)
    {
        var node = SearchNodeForInsert(value, Root);
        node.AddKey(value);

        if (node.IsKeysFull)
        {
            BalanceTree(node);
            Root = GetParentNode(Root);
        }
    }

    private void BalanceTree(BtreeNode node)
    {
        var medianIndex = _maxNumberOfKeys / 2;
        var median = node.Keys[medianIndex];
        var leftNodeKeys = node.GetRangeKeys(..medianIndex);
        var rightNodeKeys = node.GetRangeKeys((medianIndex + 1)..);

        var parentNode = node.ParentNode ?? new BtreeNode(_maxNumberOfKeys);

        // From now on, node is the left child.
        node.SetKeys(leftNodeKeys);
        node.ParentNode = parentNode;

        var rightNode = new BtreeNode(_maxNumberOfKeys, parentNode);
        rightNode.SetKeys(rightNodeKeys);

        if (node.Children.Count != 0)
        {
            var leftNodeChild = node.GetRangeChildren(..(medianIndex + 1));
            var rightNodeChild = node.GetRangeChildren((medianIndex + 1)..);

            node.SetChildren(leftNodeChild);
            rightNode.SetChildren(rightNodeChild);
        }

        // Remove current node from children to add it again as left child.
        parentNode.RemoveChildByReference(node);
        parentNode.AddKeyAndChildren(median, node, rightNode);

        if (parentNode.IsKeysFull)
        {
           BalanceTree(parentNode);
        }
    }

    private BtreeNode GetParentNode(BtreeNode node)
    {
        while (node.ParentNode is not null)
        {
            node = node.ParentNode;
        }

        return node;
    }
    
    // TODO Use binary search? https://en.wikipedia.org/wiki/B-tree#Search
    // TODO Use the search method if possible.
    private BtreeNode SearchNodeForInsert(int value, BtreeNode node)
    {
        // If node is a leaf, it means we have found the node to insert our value into.
        if (node.IsLeaf) return node;

        for (var i = 0; i < node.Keys.Count; i++)
        {
            if (value > node.Keys[i]) continue;

            // Value is less that current key, search its child.
            return SearchNodeForInsert(value, node.Children[i]);
        }

        // Value is greater than all keys, search right most child
        return SearchNodeForInsert(value, node.Children[node.Keys.Count]);
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
            return Search(value, node.Children[i]);
        }

        // Value is greater than all keys, and there is no
        // child to search for the Value in its Keys.
        if (node.IsLeaf) return -1;

        // Value is greater than all keys, search right most child
        return Search(value, node.Children[node.Keys.Count]);
    }
}