using SqlParser.Exceptions;

namespace SqlParser.BtreeImpl;

/*
 * Source: https://en.wikipedia.org/wiki/B-tree#Definition
 * Note: Rule NO 7 and 8 are applied ONLY IF NO duplicate keys are allowed.
 * Rules:
 * 1- Keys should be stored in increasing order.
 * 2- Nodes max children count is M
 * 3- Root node have at least 2 children unless it's a leaf node
 * 4- Internal node should have at least M/2 child
 * 5- Internal node with K children should contain K-1 keys.
 * 6- All leaves should be at the same level (same depth)
 * 7- Left child of a key should be less than or equal to the key
 * 8- Right child of a key should be greater than the key
 */
public class BtreeValidator
{
    public void ThrowWhenInvalidBTree(Btree tree, int maxKeysCount)
    {
        ThrowWhenRuleNumberOneIsViolated(tree.Root);

        //calculated based on of rule NO 5.
        var maxChildCount = maxKeysCount + 1;
        ThrowWhenRuleNumberTwoIsViolated(tree.Root, maxChildCount);
        ThrowWhenRuleNumberThreeIsViolated(tree.Root);
    }

    private void ThrowWhenRuleNumberOneIsViolated(BtreeNode node)
    {
        if (!node.Keys.SequenceEqual(node.Keys.Order()))
        {
            throw new InvalidBtreeException("Keys are not in ascending order.");
        }

        if (node.Children.Count == 0) return;

        foreach (var child in node.Children)
        {
            ThrowWhenRuleNumberOneIsViolated(child);
        }
    }

    private void ThrowWhenRuleNumberTwoIsViolated(BtreeNode node, int maxChildCount)
    {
        if (node.Children.Count > maxChildCount)
        {
            throw new InvalidBtreeException("Node has more that max amount of children.");
        }

        foreach (var child in node.Children)
        {
            ThrowWhenRuleNumberTwoIsViolated(child, maxChildCount);
        }
    }
    
    private void ThrowWhenRuleNumberThreeIsViolated(BtreeNode rootNode)
    {
        if (rootNode.IsLeaf) return;

        if (rootNode.Children.Count < 2)
        {
            throw new InvalidBtreeException("Root node should at least have 2 children unless its a leaf.");
        }
    }
}