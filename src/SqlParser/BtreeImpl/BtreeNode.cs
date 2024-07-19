namespace SqlParser.BtreeImpl;

public record BtreeNode
{
    private readonly int _maxKeysCount;

    public BtreeNode(int maxKeysCount, BtreeNode? parentNode = null)
    {
        _maxKeysCount = maxKeysCount;
        Keys = [];
        Child = [];
        ParentNode = parentNode;
    }

    // TODO Add methods to CRUD key and child(?). remove set accessor?
    public List<int> Keys { get; set; }
    public List<BtreeNode> Child { get; set; }
    public BtreeNode? ParentNode { get; set; }

    public bool IsLeaf
    {
        get => Child.Count == 0;
    }

    public bool IsKeysFull
    {
        get => Keys.Count >= _maxKeysCount;
    }

    // TODO is there any better method to search? e.g binary search.
    public void AddKey(int key)
    {
        AddKeyInternal(key);
    }
    
    public void AddKeyAndChildren(int key, BtreeNode leftChild, BtreeNode rightChild)
    {
        var keyIndex = AddKeyInternal(key);
        
        if (keyIndex >= Child.Count)
        {
            Child.Add(leftChild);
        }
        else
        {
            Child.Insert(keyIndex, leftChild);
        }

        var rightChildIndex = keyIndex + 1;
        if (key >= Child.Count)
        {
            Child.Add(rightChild);
            return;
        }
        
        Child.Insert(rightChildIndex, rightChild);
    }

    public void RemoveChildByReference(BtreeNode node)
    {
        for (var i = 0; i < Child.Count; i++)
        {
            if (!ReferenceEquals(Child[i], node)) continue;

            Child.RemoveAt(i);
            break;
        }
    }
    
    private int AddKeyInternal(int key)
    {
        for (var i = 0; i < Keys.Count; i++)
        {
            if (key > Keys[i]) continue;

            Keys.Insert(i, key);
            return i;
        }

        Keys.Add(key);
        return Keys.Count - 1;
    }
}