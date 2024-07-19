using System.Collections.ObjectModel;

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

    public BtreeNode? ParentNode { get; set; }

    public bool IsLeaf
    {
        get => Children.Count == 0;
    }

    public bool IsKeysFull
    {
        get => Keys.Count >= _maxKeysCount;
    }

    public bool IsKeysLessThanMinimum
    {
        get => _keys.Count < _minKeysCount;
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
        if (key >= Children.Count)
        {
            _children.Add(rightChild);
            return;
        }
        
        _children.Insert(rightChildIndex, rightChild);
    }

    public void RemoveChildByReference(BtreeNode node)
    {
        for (var i = 0; i < Children.Count; i++)
        {
            if (!ReferenceEquals(Children[i], node)) continue;

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
}