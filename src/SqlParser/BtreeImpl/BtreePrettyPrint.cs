using System.Resources;
using System.Xml;

namespace SqlParser.BtreeImpl;

public static class BtreePrettyPrint
{
    private static readonly string SpaceBetweenKeys = new string('\0', 3);
    private static readonly string SpaceBetweenChildNodes = new string('\0', 3);

    public static void PrettyPrint(this Btree tree)
    {
        // var result = new List<List<char>>();
        // X(tree.Root, result);

        var result = new List<List<char>>();
        var childRow = new List<char>();
        GetChild(tree.Root.Child, result, childRow);

        var c = TreeToCharList(tree.Root);

        TrimEnd(c);
        result.Insert(0, c);

        NormalizeRowLengthAndCenterContent(result);
        BoxEveryNode(result);

        foreach (var row in result)
        {
            Console.WriteLine(row.ToArray());
        }
    }

    private static void BoxEveryNode(List<List<char>> result)
    {
        foreach (var row in result)
        {
            var searchStartNodePosition = 0;
            var startNodePosition = GetStartNodePosition(row, searchStartNodePosition);

            while (startNodePosition is not -1)
            {
                var endNodePosition = GetEndNodePosition(row, startNodePosition);
                searchStartNodePosition = endNodePosition;
                row.Insert(startNodePosition, '│');
                row.Insert(endNodePosition + 1, '│');
                
                startNodePosition = GetStartNodePosition(row, searchStartNodePosition);
            }
        }
    }

    private static int GetStartNodePosition(List<char> row, int startFrom)
    {
        for (var i = startFrom; i < row.Count; i++)
        {
            if (row[i] is '\0' || row[i] is default(char)) continue;

            return i;
        }

        return -1;
    }

    private static int GetEndNodePosition(List<char> row, int startFrom)
    {
        for (var i = startFrom; i < row.Count; i++)
        {
            if (row[i] is '\0')
            {
                var peekPosition = i + 1;
                if (peekPosition < row.Count && (row[peekPosition] is '\0' || row[i] is default(char))) return i;
            }
        }

        return row.Count - 1;
    }

    private static void NormalizeRowLengthAndCenterContent(List<List<char>> result)
    {
        var lastRowLength = result[^1].Count;
        for (var i = 0; i < result.Count - 1; i++)
        {
            var length = result[0].Count;
            var spacesToInsert = (lastRowLength - length) / 2;
            for (var j = 0; j < spacesToInsert + 1; j++)
            {
                result[0].Insert(0, ' ');
            }
        }
    }

    private static void GetChild(List<BtreeNode> nodes, List<List<char>> output, List<char> row)
    {
        if (nodes.Count == 0) return;

        var childRow = new List<char>();
        foreach (var node in nodes)
        {
            foreach (var key in node.Keys)
            {
                row.AddRange(key.ToString().AsSpan().TrimEnd());
                row.AddRange(SpaceBetweenKeys.AsSpan());
            }

            row.AddRange(SpaceBetweenChildNodes.AsSpan());

            GetChild(node.Child, output, childRow);
        }

        if (childRow.Count != 0)
        {
            TrimEnd(childRow);
            output.Insert(0, childRow);
        }

        if (row.Count != 0)
        {
            TrimEnd(row);
            output.Insert(0, row);
        }
    }

    private static List<char> TreeToCharList(BtreeNode node)
    {
        var c = new List<char>();
        foreach (var key in node.Keys)
        {
            c.AddRange(key.ToString().AsSpan().TrimEnd());
            c.AddRange(SpaceBetweenKeys.AsSpan());
        }

        TrimEnd(c);
        return c;
    }

    private static void TrimEnd(List<char> chars)
    {
        for (var i = chars.Count - 1; i >= 0; i--)
        {
            if (char.IsWhiteSpace(chars[i]) || chars[i] is '\0')
            {
                chars.RemoveAt(i);
                continue;
            }

            break;
        }
    }
}