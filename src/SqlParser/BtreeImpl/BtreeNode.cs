using System.Text.Json.Serialization;

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

    // TODO Add comments
    public bool AddKeyToCurrentNodeFromSibling()
    {
        return ParentNode is not null && (ParentNode.AddKeyToNodeFromLeftSibling(this) ||
                                          ParentNode.AddKeyToNodeFromRightSibling(this));
    }

    private int AddKeyInternal(int key)
    {
        for (var i = 0; i < _keys.Count; i++)
        {
            if (key > _keys[i]) continue;

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
    private bool AddKeyToNodeFromLeftSibling(BtreeNode node)
    {
        for (var i = 0; i < _children.Count; i++)
        {
            if (!ReferenceEquals(_children[i], node)) continue;

            var leftSiblingIndex = i - 1;
            if (leftSiblingIndex < 0) return false;
            if (!_children[leftSiblingIndex].IsKeysGreaterThanMinimum) return false;

            var leftSiblingParentKey = _keys[leftSiblingIndex];
            var newKey = _children[leftSiblingIndex].RemoveLastKey();

            _keys[leftSiblingIndex] = newKey;
            _children[i]._keys.Insert(0, leftSiblingParentKey);

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
    private bool AddKeyToNodeFromRightSibling(BtreeNode node)
    {
        for (var i = 0; i < _children.Count; i++)
        {
            if (!ReferenceEquals(_children[i], node)) continue;

            var rightSiblingIndex = i + 1;
            if (rightSiblingIndex >= _children.Count) return false;
            if (!_children[rightSiblingIndex].IsKeysGreaterThanMinimum) return false;

            var rightSiblingParentKey = _keys[rightSiblingIndex - 1];
            var newKey = _children[rightSiblingIndex].RemoveFirstKey();

            _keys[rightSiblingIndex - 1] = newKey;
            _children[i]._keys.Add(rightSiblingParentKey);

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

    private int RemoveFirstKey()
    {
        var key = _keys[0];
        _keys.RemoveAt(0);

        return key;
    }
}