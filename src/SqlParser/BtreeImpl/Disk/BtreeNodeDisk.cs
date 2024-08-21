using System.Diagnostics;
using System.Text.Json.Serialization;
using Range = System.Range;

namespace SqlParser.BtreeImpl.Disk;

public record BtreeNodeDisk
{
    private readonly int _maxKeysCount;
    private readonly int _minKeysCount;
    private List<int> _keys;
    private List<BtreeNodeDisk> _children;

    public BtreeNodeDisk(int maxKeysCount, BtreeNodeDisk? parentNode = null)
    {
        _maxKeysCount = maxKeysCount;
        _minKeysCount = maxKeysCount / 2;
        _keys = [];
        _children = [];
        ParentNode = parentNode;
    }

    public IReadOnlyList<int> Keys
    {
        get => _keys;
    }

    public IReadOnlyList<BtreeNodeDisk> Children
    {
        get => _children;
    }

    [JsonIgnore]
    public BtreeNodeDisk? ParentNode { get; set; }

    [JsonIgnore]
    public bool IsLeaf
    {
        get => Children.Count == 0;
    }

    [JsonIgnore]
    public bool IsKeysFull
    {
        get => Keys.Count > _maxKeysCount;
    }

    [JsonIgnore]
    public bool IsKeysLessThanMinimum
    {
        get => Keys.Count < _minKeysCount;
    }

    [JsonIgnore]
    public bool IsKeysGreaterThanMinimum
    {
        get => Keys.Count > _minKeysCount;
    }

    public void AddKey(int key)
    {
        AddKeyInternal(key);
    }

    public void AddKeyAndChildren(int key, BtreeNodeDisk leftChild, BtreeNodeDisk rightChild)
    {
        var keyIndex = AddKeyInternal(key);

        if (keyIndex >= Children.Count)
        {
            _children.Add(leftChild);
        }
        else
        {
            _children.Insert(keyIndex, leftChild);
        }

        var rightChildIndex = keyIndex + 1;
        if (rightChildIndex >= Children.Count)
        {
            _children.Add(rightChild);
            return;
        }

        _children.Insert(rightChildIndex, rightChild);
    }

    public void RemoveChildByReference(BtreeNodeDisk node)
    {
        var searchForKey = node.Keys[0];
        var start = 0;
        var end = Keys.Count - 1;
        while (start <= end)
        {
            var middle = (start + end) / 2;

            if (Keys[middle] == searchForKey) throw new UnreachableException();

            if (Keys[middle] > searchForKey)
            {
                end = middle - 1; // Search in the left half
                continue;
            }

            start = middle + 1; // Search in the right half
        }

        var childIndex = start;
        if (!ReferenceEquals(_children[childIndex], node)) throw new InvalidOperationException("Did not find the correct children.");
        
        _children.RemoveAt(childIndex);
    }

    public void SetKeys(List<int> keys)
    {
        _keys = keys;
    }

    public void SetChildren(List<BtreeNodeDisk> nodes)
    {
        _children = nodes;

        foreach (var child in _children)
        {
            child.ParentNode = this;
        }
    }

    public List<BtreeNodeDisk> GetRangeChildren(Range range)
    {
        return _children[range];
    }

    public List<int> GetRangeKeys(Range range)
    {
        return _keys[range];
    }

    public void DeleteKey(int key)
    {
        if (!IsLeaf) throw new InvalidOperationException("Can not simply remove key when node is NOT a leaf.");

        var start = 0;
        var end = Keys.Count - 1;
        while (start <= end)
        {
            var middle = (start + end) / 2;

            if (Keys[middle] == key)
            {
                _keys.RemoveAt(middle);
                return;
            }

            if (Keys[middle] > key)
            {
                end = middle - 1; // Search in the left half
                continue;
            }

            start = middle + 1; // Search in the right half
        }

        throw new InvalidOperationException("Key not found");
    }

    /// <summary>
    /// Borrows a key from left sibling if available else borrows from right sibling.
    /// </summary>
    /// <returns><c>true</c> if borrowing was successful else <c>false</c></returns>
    public bool TryAddKeyToCurrentNodeFromSibling()
    {
        return ParentNode is not null && (ParentNode.TryAddKeyToNodeFromLeftSibling(this) ||
                                          ParentNode.TryAddKeyToNodeFromRightSibling(this));
    }

    /// <summary>
    /// Merges current node with parent key and left sibling if available else merges with right sibling.
    /// </summary>
    /// <returns><c>true</c> if merging was successful else <c>false</c></returns>
    public bool TryMergeCurrentNodeWithParentKeyAndSibling()
    {
        return ParentNode is not null && (TryMergeCurrentNodeWithParentKeyAndLeftSibling() ||
                                          TryMergeCurrentNodeWithParentKeyAndRightSibling());
    }

    /// <summary>
    /// Replaces the <paramref name="key"/> with the greatest key of the right most leaf.
    /// </summary>
    /// <param name="key">Key to find and replace with the greatest key of the right most leaf.</param>
    /// <returns>Right most leaf that the new key was removed from.</returns>
    /// <exception cref="InvalidOperationException">When <paramref name="key"/> is not found.</exception>
    public BtreeNodeDisk ReplaceKeyWithRightMostKeyOfLeaf(int key)
    {
        // TODO use binary serach
        for (var i = 0; i < Keys.Count; i++)
        {
            if (Keys[i] != key) continue;

            var leftChild = _children[i];
            var rightMostLeaf = leftChild.GetRightMostLeaf();
            var greatestKeyOfRightMostLeaf = rightMostLeaf.RemoveLastKey();

            _keys[i] = greatestKeyOfRightMostLeaf;

            return rightMostLeaf;
        }

        throw new InvalidOperationException("Did not find the key.");
    }

    private int AddKeyInternal(int key)
    {
        var start = 0;
        var end = Keys.Count - 1;
        while (start <= end)
        {
            var middle = (start + end) / 2;

            if (Keys[middle] == key) throw new InvalidOperationException("Duplicate key is not allowed.");

            if (Keys[middle] > key)
            {
                end = middle - 1; // Search in the left half
                continue;
            }

            start = middle + 1; // Search in the right half
        }

        _keys.Insert(start, key);
        return start;
    }

    /// <summary>
    /// Prepends a key from left sibling of <paramref name="node"/> into <paramref name="node"/>.
    /// </summary>
    /// <param name="node">Node to prepend a key from left sibling into.</param>
    /// <returns><c>true</c> if a left sibling that have greater than minimum number of allowed keys exists,
    /// else returns <c>false</c>.</returns>
    private bool TryAddKeyToNodeFromLeftSibling(BtreeNodeDisk node)
    {
        var searchForKey = node.Keys[0];
        var start = 0;
        var end = _keys.Count - 1;
        while (start <= end)
        {
            var middle = (start + end) / 2;

            if (_keys[middle] == searchForKey) throw new UnreachableException();

            if (_keys[middle] > searchForKey)
            {
                end = middle - 1; // Search in the left half
                continue;
            }

            start = middle + 1; // Search in the right half
        }

        var nodeIndex = start;

        if (!ReferenceEquals(_children[nodeIndex], node)) throw new InvalidOperationException("Did not find the correct children.");

        var leftSiblingIndex = nodeIndex - 1;
        if (leftSiblingIndex < 0) return false;
        if (!_children[leftSiblingIndex].IsKeysGreaterThanMinimum) return false;

        var leftSiblingParentKey = _keys[leftSiblingIndex];
        var newKey = _children[leftSiblingIndex].RemoveLastKey();
        var lastChildOfLeftSibling = _children[leftSiblingIndex]._children.Count > 0 ? _children[leftSiblingIndex]._children[^1] : null;

        _keys[leftSiblingIndex] = newKey;
        _children[nodeIndex]._keys.Insert(0, leftSiblingParentKey);

        if (lastChildOfLeftSibling is not null)
        {
            // Remove this child from left sibling
            // and add it to this node's children.
            _children[nodeIndex]._children.Insert(0, lastChildOfLeftSibling);
            lastChildOfLeftSibling.ParentNode = _children[nodeIndex];
            _children[leftSiblingIndex]._children.RemoveAt(_children[leftSiblingIndex]._children.Count - 1);
        }

        return true;
    }

    /// <summary>
    /// Attempts to borrow a key from the right sibling of the given node and redistribute it to the current node.
    /// 
    /// This method performs the following steps:
    /// 1. Finds the index of the specified <paramref name="node"/> within the current node's list of children.
    /// 2. Checks if there is a right sibling of the current node and whether the right sibling has enough keys to lend.
    /// 3. If the right sibling has enough keys:
    ///    - Retrieves the first key from the right sibling and the corresponding parent key that separates the current node and its right sibling.
    ///    - Updates the current node by adding the parent key to its keys and moving the borrowed key from the right sibling to the current node.
    ///    - Transfers the first child of the right sibling (if any) to the current node’s children and updates its parent reference.
    ///    - Removes the borrowed key and transferred child from the right sibling.
    /// 4. Returns <c>true</c> if the operation was successful; otherwise, <c>false</c> if the right sibling is not available or does not have sufficient keys.
    /// 
    /// This method is used during B-tree rebalancing operations to ensure that nodes have sufficient keys and that the tree remains balanced.
    /// </summary>
    /// <param name="node">The child node from which the key will be borrowed.</param>
    /// <returns><c>true</c> if the key was successfully borrowed and redistributed; <c>false</c> otherwise.</returns>
    private bool TryAddKeyToNodeFromRightSibling(BtreeNodeDisk node)
    {
        var searchForKey = node.Keys[^1];
        var start = 0;
        var end = _keys.Count - 1;
        while (start <= end)
        {
            var middle = (start + end) / 2;

            if (_keys[middle] == searchForKey) throw new UnreachableException();

            if (_keys[middle] > searchForKey)
            {
                end = middle - 1; // Search in the left half
                continue;
            }

            start = middle + 1; // Search in the right half
        }

        var nodeIndex = start;

        if (!ReferenceEquals(_children[nodeIndex], node)) throw new InvalidOperationException("Did not find the correct children.");

        var rightSiblingIndex = nodeIndex + 1;
        if (rightSiblingIndex >= _children.Count) return false;
        if (!_children[rightSiblingIndex].IsKeysGreaterThanMinimum) return false;

        var rightSiblingParentKey = _keys[rightSiblingIndex - 1];
        var newKey = _children[rightSiblingIndex]._keys[0];
        _children[rightSiblingIndex]._keys.RemoveAt(0);

        var firstChildOfRightSibling = _children[rightSiblingIndex]._children.Count > 0 ? _children[rightSiblingIndex]._children[0] : null;

        _keys[rightSiblingIndex - 1] = newKey;
        _children[nodeIndex]._keys.Add(rightSiblingParentKey);

        if (firstChildOfRightSibling is not null)
        {
            // Remove this child from right sibling
            // and add it to this node's children.
            _children[nodeIndex]._children.Add(firstChildOfRightSibling);
            firstChildOfRightSibling.ParentNode = _children[nodeIndex];
            _children[rightSiblingIndex]._children.RemoveAt(0);
        }

        return true;
    }

    private int RemoveLastKey()
    {
        var key = _keys[^1];
        _keys.RemoveAt(_keys.Count - 1);

        return key;
    }

    /// <summary>
    /// Attempts to merge the current node with its left sibling, using the parent key as a bridge.
    /// 
    /// This method performs the following steps:
    /// 1. Identifies the index of the current node within its parent's list of children and determines the index of its left sibling.
    /// 2. If there is no left sibling, the method returns false to indicate that the merge operation cannot be performed.
    /// 3. Retrieves the parent key that separates the current node and its left sibling.
    /// 4. Merges the left sibling's keys and the parent key with the current node's keys and children, placing the merged keys and children at the start of the current node.
    /// 5. Updates the parent references of the current node's new children to point to the current node.
    /// 6. Removes the left sibling from the parent node's list of children as it has been merged.
    /// 
    /// This method is typically used during tree balancing operations where nodes need to be merged to maintain B-tree properties.
    /// 
    /// </summary>
    /// <returns>
    /// <c>true</c> if the merge operation was successful; <c>false</c> if the current node has no left sibling to merge with.
    /// </returns>
    private bool TryMergeCurrentNodeWithParentKeyAndLeftSibling()
    {
        var searchForKey = Keys[0];
        var start = 0;
        var end = ParentNode!._keys.Count - 1;
        while (start <= end)
        {
            var middle = (start + end) / 2;

            if (ParentNode._keys[middle] == searchForKey) throw new UnreachableException();

            if (ParentNode._keys[middle] > searchForKey)
            {
                end = middle - 1; // Search in the left half
                continue;
            }

            start = middle + 1; // Search in the right half
        }

        var nodeIndex = start;

        if (!ReferenceEquals(ParentNode._children[nodeIndex], this)) throw new InvalidOperationException("Did not find the correct children.");
        
        var leftSiblingIndex = nodeIndex - 1;

        // There is no left sibling
        if (leftSiblingIndex < 0) return false;

        // if left sibling index + 1 is greater than keys count,
        // it means this is the last child, so get the last parent key.
        var parentKeyIndex = leftSiblingIndex;
        var parentKey = ParentNode._keys[parentKeyIndex];

        ParentNode._keys.RemoveAt(parentKeyIndex);
        var leftSibling = ParentNode!._children[leftSiblingIndex];

        // Merge left sibling keys and parent key and current node keys and children.
        _keys.Insert(0, parentKey);
        _keys.InsertRange(0, leftSibling._keys);
        _children.InsertRange(0, leftSibling._children);
        UpdateParentOfChildrenTo(leftSibling._children, this);

        // Remove left sibling node form parent children
        // because left sibling keys are moved to this node
        ParentNode._children.RemoveAt(leftSiblingIndex);

        return true;
    }

    /// <summary>
    /// Attempts to merge the current node with its right sibling, using the parent key as a bridge.
    /// 
    /// This method performs the following steps:
    /// 1. Identifies the index of the current node within its parent's list of children and determines the index of its right sibling.
    /// 2. If there is no right sibling, the method returns false to indicate that the merge operation cannot be performed.
    /// 3. Retrieves the parent key that separates the current node and its right sibling.
    /// 4. Merges the current node's keys with the right sibling's keys and the parent key.
    /// 5. Updates the current node's children with those of the right sibling and adjusts the parent references of these children.
    /// 6. Removes the right sibling from the parent node's list of children as it has been merged.
    /// 
    /// This method is typically used during tree balancing operations where nodes need to be merged to maintain B-tree properties.
    /// 
    /// </summary>
    /// <returns>
    /// <c>true</c> if the merge operation was successful; <c>false</c> if the current node has no right sibling to merge with.
    /// </returns>
    private bool TryMergeCurrentNodeWithParentKeyAndRightSibling()
    {
        var searchForKey = Keys[0];
        var start = 0;
        var end = ParentNode!._keys.Count - 1;
        while (start <= end)
        {
            var middle = (start + end) / 2;

            if (ParentNode._keys[middle] == searchForKey) throw new UnreachableException();

            if (ParentNode._keys[middle] > searchForKey)
            {
                end = middle - 1; // Search in the left half
                continue;
            }

            start = middle + 1; // Search in the right half
        }

        var nodeIndex = start;

        if (!ReferenceEquals(ParentNode._children[nodeIndex], this)) throw new InvalidOperationException("Did not find the correct children.");
        
        var rightSiblingIndex = nodeIndex + 1;

        // There is no right sibling
        if (rightSiblingIndex >= ParentNode._children.Count) return false;

        var parentKeyIndex = rightSiblingIndex - 1;
        var parentKey = ParentNode._keys[parentKeyIndex];

        ParentNode._keys.RemoveAt(parentKeyIndex);
        var rightSibling = ParentNode!._children[rightSiblingIndex];

        // Merge current keys and parent key with right sibling keys.
        _keys.Add(parentKey);
        _keys.AddRange(rightSibling._keys);
        _children.AddRange(rightSibling._children);
        UpdateParentOfChildrenTo(rightSibling._children, this);

        // Remove right sibling form parent because
        // its keys are moved to the current node.
        ParentNode._children.RemoveAt(rightSiblingIndex);

        return true;
    }

    private void UpdateParentOfChildrenTo(List<BtreeNodeDisk> nodes, BtreeNodeDisk parentNode)
    {
        foreach (var node in nodes)
        {
            node.ParentNode = parentNode;
        }
    }

    /// <summary>
    /// Traverses down the B-tree to find and return the rightmost leaf node.
    /// This method starts at the current node and continuously moves to the rightmost child until it reaches a leaf node.
    /// It assumes that the B-tree is well-formed and that every internal node has at least one child.
    /// </summary>
    /// <returns>The rightmost leaf node in the B-tree.</returns>
    private BtreeNodeDisk GetRightMostLeaf()
    {
        var node = this;

        while (true)
        {
            if (node.IsLeaf)
            {
                return node;
            }

            node = node._children[^1];
        }
    }
}