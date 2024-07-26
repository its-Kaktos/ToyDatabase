using System.Text.Json.Serialization;
using Range = System.Range;

namespace SqlParser.BtreeImpl;

public record BtreeNode
{
    private readonly int _maxKeysCount;
    private readonly int _minKeysCount;
    private List<int> _keys;
    private List<BtreeNode> _children;

    public BtreeNode(int maxKeysCount, BtreeNode? parentNode = null)
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

    public IReadOnlyList<BtreeNode> Children
    {
        get => _children;
    }

    [JsonIgnore]
    public BtreeNode? ParentNode { get; set; }

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
    public bool IsKeysAtMinimum
    {
        get => _keys.Count == _minKeysCount;
    }

    [JsonIgnore]
    public bool IsKeysLessThanMinimum
    {
        get => _keys.Count < _minKeysCount;
    }

    [JsonIgnore]
    public bool IsKeysGreaterThanMinimum
    {
        get => _keys.Count > _minKeysCount;
    }

    // TODO is there any better method to search? e.g binary search.
    public void AddKey(int key)
    {
        AddKeyInternal(key);
    }

    public void AddKeyAndChildren(int key, BtreeNode leftChild, BtreeNode rightChild)
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

    public void RemoveChildByReference(BtreeNode node)
    {
        for (var i = 0; i < _children.Count; i++)
        {
            if (!ReferenceEquals(_children[i], node)) continue;

            _children.RemoveAt(i);
            break;
        }
    }

    public void SetKeys(List<int> keys)
    {
        _keys = keys;
    }

    public void SetChildren(List<BtreeNode> nodes)
    {
        _children = nodes;

        foreach (var child in _children)
        {
            child.ParentNode = this;
        }
    }

    public List<BtreeNode> GetRangeChildren(Range range)
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

        for (var i = 0; i < _keys.Count; i++)
        {
            if (_keys[i] != key) continue;

            _keys.RemoveAt(i);
            return;
        }
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
    public BtreeNode ReplaceKeyWithRightMostKeyOfLeaf(int key)
    {
        for (var i = 0; i < _keys.Count; i++)
        {
            if (_keys[i] != key) continue;

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
        for (var i = 0; i < _keys.Count; i++)
        {
            if (key > _keys[i]) continue;
            if (key == _keys[i]) throw new InvalidOperationException("Duplicate key is not allowed.");

            _keys.Insert(i, key);
            return i;
        }

        _keys.Add(key);
        return _keys.Count - 1;
    }

    /// <summary>
    /// Prepends a key from left sibling of <paramref name="node"/> into <paramref name="node"/>.
    /// </summary>
    /// <param name="node">Node to prepend a key from left sibling into.</param>
    /// <returns><c>true</c> if a left sibling that have greater than minimum number of allowed keys exists,
    /// else returns <c>false</c>.</returns>
    private bool TryAddKeyToNodeFromLeftSibling(BtreeNode node)
    {
        for (var i = 0; i < _children.Count; i++)
        {
            if (!ReferenceEquals(_children[i], node)) continue;

            var leftSiblingIndex = i - 1;
            if (leftSiblingIndex < 0) return false;
            if (!_children[leftSiblingIndex].IsKeysGreaterThanMinimum) return false;

            var leftSiblingParentKey = _keys[leftSiblingIndex];
            var newKey = _children[leftSiblingIndex].RemoveLastKey();
            var lastChildOfLeftSibling = _children[leftSiblingIndex]._children.Count > 0 ? _children[leftSiblingIndex]._children[^1] : null;

            _keys[leftSiblingIndex] = newKey;
            _children[i]._keys.Insert(0, leftSiblingParentKey);

            if (lastChildOfLeftSibling is not null)
            {
                // Remove this child from left sibling
                // and add it to this node's children.
                _children[i]._children.Insert(0, lastChildOfLeftSibling);
                lastChildOfLeftSibling.ParentNode = _children[i];
                _children[leftSiblingIndex]._children.RemoveAt(_children[leftSiblingIndex]._children.Count - 1);
            }
            
            return true;
        }

        return false;
    }

    /// <summary>
    /// Appends a key from right sibling of <paramref name="node"/> into <paramref name="node"/>.
    /// </summary>
    /// <param name="node">Node to prepend a key from right sibling into.</param>
    /// <returns><c>true</c> if a right sibling that have greater than minimum number of allowed keys exists,
    /// else returns <c>false</c>.</returns>
    private bool TryAddKeyToNodeFromRightSibling(BtreeNode node)
    {
        for (var i = 0; i < _children.Count; i++)
        {
            if (!ReferenceEquals(_children[i], node)) continue;

            var rightSiblingIndex = i + 1;
            if (rightSiblingIndex >= _children.Count) return false;
            if (!_children[rightSiblingIndex].IsKeysGreaterThanMinimum) return false;

            var rightSiblingParentKey = _keys[rightSiblingIndex - 1];
            var newKey = _children[rightSiblingIndex]._keys[0];
            _children[rightSiblingIndex]._keys.RemoveAt(0);

            var firstChildOfRightSibling = _children[rightSiblingIndex]._children.Count > 0 ? _children[rightSiblingIndex]._children[0] : null;
            
            _keys[rightSiblingIndex - 1] = newKey;
            _children[i]._keys.Add(rightSiblingParentKey);

            if (firstChildOfRightSibling is not null)
            {
                // Remove this child from right sibling
                // and add it to this node's children.
                _children[i]._children.Add(firstChildOfRightSibling);
                firstChildOfRightSibling.ParentNode = _children[i];
                _children[rightSiblingIndex]._children.RemoveAt(0);
            }

            return true;
        }

        return false;
    }

    private int RemoveLastKey()
    {
        var key = _keys[^1];
        _keys.RemoveAt(_keys.Count - 1);

        return key;
    }

    private bool TryMergeCurrentNodeWithParentKeyAndLeftSibling()
    {
        var leftSiblingIndex = -1;
        for (var i = 0; i < ParentNode!._children.Count; i++)
        {
            if (!ReferenceEquals(this, ParentNode!._children[i])) continue;

            leftSiblingIndex = i - 1;
            break;
        }

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
    
    private bool TryMergeCurrentNodeWithParentKeyAndRightSibling()
    {
        var rightSiblingIndex = -1;
        for (var i = 0; i < ParentNode!._children.Count; i++)
        {
            if (!ReferenceEquals(this, ParentNode!._children[i])) continue;

            rightSiblingIndex = i + 1;
            break;
        }

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

    private void UpdateParentOfChildrenTo(List<BtreeNode> nodes, BtreeNode parentNode)
    {
        foreach (var node in nodes)
        {
            node.ParentNode = parentNode;
        }
    }
    
    /// <summary>
    /// Returns the last (right most) leaf.
    /// </summary>
    /// <returns>The last (right most) leaf.</returns>
    private BtreeNode GetRightMostLeaf()
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