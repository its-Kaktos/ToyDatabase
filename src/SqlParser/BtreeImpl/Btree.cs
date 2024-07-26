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

    public void Add(int key)
    {
        var node = GetNodeToInsertKey(key, Root);
        node.AddKey(key);

        if (node.IsKeysFull)
        {
            BalanceTree(node);
            Root = GetRootNode(Root);
        }
    }

    public void Delete(int key)
    {
        var node = GetNodeToDeleteKey(key, Root);
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
        if (parentNode.Children.Count != 0) parentNode.RemoveChildByReference(node);
        parentNode.AddKeyAndChildren(median, node, rightNode);

        if (parentNode.IsKeysFull)
        {
            BalanceTree(parentNode);
        }
    }

    private BtreeNode GetRootNode(BtreeNode node)
    {
        while (node.ParentNode is not null)
        {
            node = node.ParentNode;
        }

        return node;
    }

    private BtreeNode GetNodeToInsertKey(int key, BtreeNode node)
    {
        while (true)
        {
            var (keyIndex, childIndex) = BinarySearchKey(node, key);
            if (keyIndex is not null && node.Keys[keyIndex.Value] == key) throw new InvalidOperationException("Duplicate key is not allowed.");

            // If node is a leaf, it means we have found the node to insert our value into.
            if (node.IsLeaf) return node;

            if (childIndex is null) throw new InvalidOperationException("Node is not a leaf, child index must not be null");

            // Search for key in the child.
            node = node.Children[childIndex.Value];
        }
    }

    private BtreeNode? GetNodeToDeleteKey(int key, BtreeNode node)
    {
        while (true)
        {
            var (keyIndex, childIndex) = BinarySearchKey(node, key);
            if (keyIndex is not null && node.Keys[keyIndex.Value] == key) return node; // Node is found.

            // Key is not found and there is no other child left to search for.
            if (node.IsLeaf) return null;

            if (childIndex is null) throw new InvalidOperationException("Node is not a leaf, child index must not be null");

            // Search for key in the child.
            node = node.Children[childIndex.Value];
        }
    }

    private int? Search(int key, BtreeNode node)
    {
        var (keyIndex, childIndex) = BinarySearchKey(node, key);
        if (keyIndex is not null && node.Keys[keyIndex.Value] == key) return key;

        // Key is not found and there is no other child left to search for.
        if (node.IsLeaf) return null;

        if (childIndex is null) throw new InvalidOperationException("Node is not a leaf, child index must not be null");

        // Search for key in the child.
        return Search(key, node.Children[childIndex.Value]);
    }

    /// <summary>
    /// Performs a binary search for a specified <paramref name="target"/> key in the <paramref name="node"/>'s keys.
    /// If the <paramref name="target"/> key is found, the method returns a tuple with the index of the key and a null value
    /// for the child index. If the key is not found, the method returns a tuple where the key index is null and the child index
    /// indicates where the <paramref name="target"/> might be located if the search continues in the child nodes. If <paramref name="node"/>
    /// is a leaf node and the key is not found, the child index will be null.
    /// The method uses a binary search algorithm to efficiently locate the key or determine the appropriate child index for further searching.
    /// </summary>
    /// <param name="node">The B-tree node in which to search for the <paramref name="target"/> key.</param>
    /// <param name="target">The key value to search for within the <paramref name="node"/> keys.</param>
    /// <returns>A tuple where:
    ///     - <c>keyIndex</c> is the index of the <paramref name="target"/> key if found, otherwise null.
    ///     - <c>childIndex</c> is the index of the child node to search if the <paramref name="target"/> is not found and <paramref name="node"/> is not a leaf; otherwise, null.
    /// </returns>
    private (int? keyIndex, int? childIndex) BinarySearchKey(BtreeNode node, int target)
    {
        var start = 0;
        var end = node.Keys.Count - 1;
        while (start <= end)
        {
            var middle = (start + end) / 2;

            if (node.Keys[middle] == target)
            {
                return (middle, null); // Key found at index `middle`
            }

            if (node.Keys[middle] > target)
            {
                end = middle - 1; // Search in the left half
                continue;
            }

            start = middle + 1; // Search in the right half
        }

        // Key not found, determine the child index to search next
        return (null, GetChildIndex());

        int? GetChildIndex()
        {
            // The key is not found, and there is no child left.
            if (node.IsLeaf) return null;

            var indexOfLastCheckedKey = start - 1 < 0 ? 0 : start - 1;
            var leftChildIndex = indexOfLastCheckedKey - 1 < 0 ? 0 : indexOfLastCheckedKey - 1;
            var isLastCheckedKeyGreaterThanTarget = node.Keys[indexOfLastCheckedKey] > target;

            return isLastCheckedKeyGreaterThanTarget ? leftChildIndex : indexOfLastCheckedKey + 1;
        }
    }
}