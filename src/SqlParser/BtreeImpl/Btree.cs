using System.Diagnostics;

namespace SqlParser.BtreeImpl;

// Disallow duplicate key insertion, because:
//  Firstly, databases do NOT allow duplicate indexes,
//  secondly, by the search definition of a BST, if the value is <>,
//      you traverse the data structure in one of two 'directions'.
//      So, in that sense, duplicate values don't make any sense at all.
//  thirdly having duplicate values adds more complexity
//      which I don't want to handle right now.
// TODO Use array instead of List to reduce memory usage.
/**
 * Sources:
 * https://en.wikipedia.org/wiki/B-tree
 * https://www.youtube.com/watch?v=K1a2Bk8NrYQ&ab_channel=SpanningTree
 * https://www.cs.cornell.edu/courses/cs211/2000fa/AccelStream/B-Trees.pdf
 */
public class Btree
{
    private readonly int _maxKeysCount;

    public Btree(int maxKeysCount)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(maxKeysCount, 2);

        _maxKeysCount = maxKeysCount;
        Root = new BtreeNode(_maxKeysCount);
    }

    public BtreeNode Root { get; private set; }

    /// <summary>
    /// Searches the btree for the <paramref name="key"/>.
    /// </summary>
    /// <param name="key">Value to search for.</param>
    /// <returns>The value if btree contains the <paramref name="key"/>, else <c>null</c></returns>
    public int? Search(int key)
    {
        return Search(key, Root);
    }

    public void Insert(int key)
    {
        var node = FindNodeToInsertKeyInto(key, Root);
        node.AddKey(key);

        if (node.IsKeysFull)
        {
            BalanceTree(node);
            Root = GetParentNode(Root);
        }
    }

    public void Delete(int key)
    {
        var node = SearchKey(key, Root);
        if (node is null) throw new InvalidOperationException("Key is not found.");

        if (!node.IsLeaf)
        {
            // Remove left child's biggest key (right most leaf of the left child) and return that key,
            // lets name the returned key 'new key'. Replace the key with the 'new key', and re-balance the tree from
            // the leaf node that the 'new key' was removed from.
            var leafNode = node.ReplaceKeyWithRightMostKeyOfLeaf(key);

            // To re-balance the tree if needed.
            node = leafNode;
        }
        else
        {
            node.DeleteKey(key);
        }

        BalanceTreeAfterDeletion(node);
    }

    // Tail recursive calls are never optimized in c#! : https://blog.objektkultur.de/about-tail-recursion-in-.net/
    // You can see it is not optimized in the IL Viewer.
    private void BalanceTreeAfterDeletion(BtreeNode node)
    {
        if (ReferenceEquals(Root, node)) return;

        // There is enough keys in node, simply return.
        if (!node.IsKeysLessThanMinimum) return;

        // If we get here it means there is not enough
        // keys in current node.

        // Try to get a key from siblings
        if (node.TryAddKeyToCurrentNodeFromSibling()) return;

        // Siblings only have the minimum amount of keys,
        // merge current node and its parent key with
        // left or right sibling
        if (!node.TryMergeCurrentNodeWithParentKeyAndSibling())
        {
            throw new InvalidOperationException("Can not remove current key, merging was not successful.");
        }

        var isNextNodeRoot = node.ParentNode?.ParentNode is null;
        if (isNextNodeRoot)
        {
            var rootNode = node.ParentNode!;
            if (rootNode.Keys.Count == 0)
            {
                // Current root node is empty,
                // Make current node the new root node.

                Root = node;
                node.ParentNode = null;
            }

            return;
        }

        if (node.ParentNode is not null) BalanceTreeAfterDeletion(node.ParentNode);
    }

    // Tail recursive calls are never optimized in c#! : https://blog.objektkultur.de/about-tail-recursion-in-.net/
    // You can see it is not optimized in the IL Viewer.
    private void BalanceTree(BtreeNode node)
    {
        var medianIndex = _maxKeysCount / 2;
        var median = node.Keys[medianIndex];
        var leftNodeKeys = node.GetRangeKeys(..medianIndex);
        var rightNodeKeys = node.GetRangeKeys((medianIndex + 1)..);

        var parentNode = node.ParentNode ?? new BtreeNode(_maxKeysCount);

        // From now on, node is the left child.
        node.SetKeys(leftNodeKeys);
        node.ParentNode = parentNode;

        var rightNode = new BtreeNode(_maxKeysCount, parentNode);
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

    private BtreeNode FindNodeToInsertKeyInto(int key, BtreeNode node)
    {
        return SearchKeyToInsert(key, node) ?? throw new UnreachableException();
    }

    // TODO Use binary search? https://en.wikipedia.org/wiki/B-tree#Search
    private BtreeNode? SearchKeyToInsert(int key, BtreeNode node)
    {
        // If node is a leaf, it means we have found the node to insert our value into.
        if (node.IsLeaf) return node;

        for (var i = 0; i < node.Keys.Count; i++)
        {
            if (key > node.Keys[i]) continue;
            if (key == node.Keys[i]) throw new InvalidOperationException("Duplicate key is not allowed.");

            // Value is less that current key, search its child.
            return SearchKeyToInsert(key, node.Children[i]);
        }

        // Value is greater than all keys, search right most child
        return SearchKeyToInsert(key, node.Children[node.Keys.Count]);
    }

    private BtreeNode? SearchKey(int key, BtreeNode node)
    {
        for (var i = 0; i < node.Keys.Count; i++)
        {
            if (key > node.Keys[i]) continue;
            if (key == node.Keys[i]) return node;

            // Value is less that current key, search its child.
            return node.IsLeaf ? null : SearchKey(key, node.Children[i]);
        }

        // Value is greater than all keys, search right most child
        return node.IsLeaf ? null : SearchKey(key, node.Children[node.Keys.Count]);
    }

    // TODO Use binary search? https://en.wikipedia.org/wiki/B-tree#Search
    private int? Search(int key, BtreeNode node)
    {
        for (var i = 0; i < node.Keys.Count; i++)
        {
            if (key > node.Keys[i]) continue;
            if (key == node.Keys[i]) return node.Keys[i];

            // Value is less than current key, and there is no
            // child to search for the Value in its Keys.
            if (node.IsLeaf) return -1;

            // Value is less that current key, search its child.
            return Search(key, node.Children[i]);
        }

        // Value is greater than all keys, and there is no
        // child to search for the Value in its Keys.
        if (node.IsLeaf) return -1;

        // Value is greater than all keys, search right most child
        return Search(key, node.Children[node.Keys.Count]);
    }
}