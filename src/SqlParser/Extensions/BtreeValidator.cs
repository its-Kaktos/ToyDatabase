using SqlParser.BtreeImpl;
using SqlParser.Exceptions;

namespace SqlParser.Extensions;

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
public static class BtreeValidator
{
    public static void StressTestRandomTreesInfinitly(this Btree tree)
    {
        long numbersOfPasses = 2;
        var random = new Random();
        while (true)
        {
            var maxKeysCount = random.Next(4, 150);
            var btree = new Btree(maxKeysCount);
            var listNumbers = Enumerable.Range(1, random.Next(50, 500)).ToList();

            Console.WriteLine($"++++START new random check, max keys: {maxKeysCount}, numbers of items added: {listNumbers.Count}");
            foreach (var i in listNumbers)
            {
                btree.Insert(i);
                btree.ThrowWhenInvalidBTree(maxKeysCount);
            }

            var guid = Guid.NewGuid();
            listNumbers = listNumbers.OrderBy(x => guid).ToList();
            while (true)
            {
                var numberToRemove = listNumbers[0];
                btree.Delete(numberToRemove);
                btree.ThrowWhenInvalidBTree(maxKeysCount);
                listNumbers.RemoveAt(0);
                if (listNumbers.Count <= 0) break;
            }

            numbersOfPasses++;
            Console.WriteLine($"Number of passes: {numbersOfPasses}");
            Thread.Sleep(10);
        }
    }

    /// <summary>
    /// Validate B-tree based on these rules.
    /// <para>Note: this validation is not optimized and will traverse the tree multiple times.</para>
    /// <para>Note: Rule NO 7 and 8 are applied ONLY IF NO duplicate keys are allowed.</para>
    /// <para>Rules:</para>    
    /// <para>1- Keys should be stored in increasing order.</para>    
    /// <para>2- Nodes max children count is M</para>    
    /// <para>3- Root node have at least 2 children unless it's a leaf node</para>    
    /// <para>4- Internal node should have at least M/2 child</para>    
    /// <para>5- Internal node with K children should contain K-1 keys.</para>    
    /// <para>6- All leaves should be at the same level (same depth)</para>    
    /// <para>7- Left child of a key should be less than or equal to the key</para>    
    /// <para>8- Right child of a key should be greater than the key</para>
    /// </summary>
    /// <param name="tree">Three to validate</param>
    /// <param name="maxKeysCount">Max keys count provided to the tree.</param>
    public static void ThrowWhenInvalidBTree(this Btree tree, int maxKeysCount)
    {
        ThrowWhenRuleNumberOneIsViolated(tree.Root);

        //calculated based on of rule NO 5.
        var maxChildCount = maxKeysCount + 1;
        ThrowWhenRuleNumberTwoIsViolated(tree.Root, maxChildCount);
        ThrowWhenRuleNumberThreeIsViolated(tree.Root);
        foreach (var child in tree.Root.Children)
        {
            ThrowWhenRuleNumberFourIsViolated(child, maxChildCount);
            ThrowWhenRuleNumberFiveIsViolated(child);
        }

        ThrowWhenRuleNumberSixIsViolated(tree.Root);
        ThrowWhenRuleNumberSevenAndEightIsViolated(tree.Root);
    }

    private static void ThrowWhenRuleNumberOneIsViolated(BtreeNode node)
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

    private static void ThrowWhenRuleNumberTwoIsViolated(BtreeNode node, int maxChildCount)
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

    private static void ThrowWhenRuleNumberThreeIsViolated(BtreeNode rootNode)
    {
        if (rootNode.IsLeaf) return;

        if (rootNode.Children.Count < 2)
        {
            throw new InvalidBtreeException("Root node should at least have 2 children unless its a leaf.");
        }
    }

    private static void ThrowWhenRuleNumberFourIsViolated(BtreeNode node, int maxChildCount)
    {
        // This rule only apply to internal nodes
        if (node.IsLeaf) return;

        var minChildCount = maxChildCount / 2;
        if (node.Children.Count < minChildCount)
        {
            throw new InvalidBtreeException("Internal node should have at least (max child count / 2) child");
        }

        foreach (var child in node.Children)
        {
            ThrowWhenRuleNumberFourIsViolated(child, maxChildCount);
        }
    }

    private static void ThrowWhenRuleNumberFiveIsViolated(BtreeNode node)
    {
        // This rule only apply to internal nodes
        if (node.IsLeaf) return;

        if (node.Children.Count - 1 != node.Keys.Count)
        {
            throw new InvalidBtreeException("Internal node with K children should contain K-1 keys.");
        }

        foreach (var child in node.Children)
        {
            ThrowWhenRuleNumberFiveIsViolated(child);
        }
    }

    private static void ThrowWhenRuleNumberSixIsViolated(BtreeNode rootNode)
    {
        var leafLevel = 0;
        ThrowWhenRuleNumberSixIsViolatedInternal(rootNode, 0, ref leafLevel);
    }

    private static void ThrowWhenRuleNumberSixIsViolatedInternal(BtreeNode node, int level, ref int leafLevel)
    {
        if (node.IsLeaf)
        {
            if (leafLevel == 0)
            {
                leafLevel = level; // Set the level of the first leaf node
                return;
            }

            // Compare subsequent leaf levels
            if (level != leafLevel)
            {
                throw new InvalidBtreeException("All leaves should be at the same level (same depth)");
            }
        }

        foreach (var child in node.Children)
        {
            ThrowWhenRuleNumberSixIsViolatedInternal(child, level + 1, ref leafLevel);
        }
    }

    private static void ThrowWhenRuleNumberSevenAndEightIsViolated(BtreeNode node)
    {
        if (node.IsLeaf) return;

        for (var i = 0; i < node.Keys.Count; i++)
        {
            // Last key of the child should be less than or equal to the node's first key
            if (node.Children[i].Keys[^1] > node.Keys[i])
                throw new InvalidBtreeException("Left child of a key should be less than or equal to the key and right child of a key should be greater than the key.");
        }

        // First key of the last child should be greater than the node's last key
        if (node.Children[^1].Keys[0] <= node.Keys[^1])
            throw new InvalidBtreeException("Left child of a key should be less than or equal to the key and right child of a key should be greater than the key.");

        foreach (var child in node.Children)
        {
            ThrowWhenRuleNumberSevenAndEightIsViolated(child);
        }
    }
}