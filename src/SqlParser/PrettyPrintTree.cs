using System.Text;
using System.Text.RegularExpressions;
using SqlParser.Nodes;

namespace SqlParser;

public enum Color
{
    Red,
    Green,
    Yellow,
    Blue,
    Pink,
    LightBlue,
    White,
    None
}

public static class PrettyPrintAst
{
    public static void PrettyPrint(this IAST parentNode, Color color = Color.None)
    {
        var pt = new PrettyPrintTree<IAST>(
            getChildren: node => node.GetChildren(),
            getVal: node => node.GetValue(),
            color
        );

        pt.Display(parentNode);
    }
}

// https://github.com/AharonSambol/PrettyPrintTreeCSharp
internal partial class PrettyPrintTree<TNode>
{
    private static readonly Regex SlashNRegex = MyRegex();

    private static readonly Dictionary<Color, string> ColorToNum = new()
    {
        { Color.Red, "41" },
        { Color.Green, "42" },
        { Color.Yellow, "43" },
        { Color.Blue, "44" },
        { Color.Pink, "45" },
        { Color.LightBlue, "46" },
        { Color.White, "47" },
    };

    private readonly Func<TNode, IEnumerable<TNode?>> _getChildren;
    private readonly Func<TNode, string> _getNodeVal;
    private readonly int _maxDepth;
    private readonly int _trim;
    private readonly Color _color;
    private readonly bool _border;
    private readonly bool _escapeNewline;

    public PrettyPrintTree(Func<TNode, IEnumerable<TNode?>> getChildren,
        Func<TNode, string> getVal,
        Color color = Color.Blue,
        bool border = false,
        bool escapeNewline = false,
        int trim = -1,
        int maxDepth = -1)
    {
        _getChildren = getChildren;
        _getNodeVal = getVal;
        _color = color;
        _border = border;
        _maxDepth = maxDepth;
        _trim = trim;
        _escapeNewline = escapeNewline;
    }

    public void Display(TNode node, int depth = 0)
    {
        Console.WriteLine(ToStr(node, depth: depth));
    }

    private string ToStr(TNode node, int depth = 0)
    {
        var res = TreeToStr(node, depth: depth);
        var str = new StringBuilder();
        foreach (var line in res)
        {
            foreach (var x in line)
            {
                str.Append(IsNode(x) ? ColorTxt(x) : x);
            }

            str.Append('\n');
        }

        str.Length -= 1;
        return str.ToString();
    }

    private string?[][] GetVal(TNode node)
    {
        var stVal = _getNodeVal(node);
        if (_trim != -1 && _trim < stVal.Length)
        {
            stVal = string.Concat(stVal.AsSpan(0, _trim), "...");
        }

        if (_escapeNewline)
        {
            stVal = SlashNRegex.Replace(stVal, (x) => x.Value.Equals("\n") ? "\\n" : @"\\n");
        }

        if (!stVal.Contains('\n'))
        {
            return
            [
                [stVal]
            ];
        }

        var lstVal = stVal.Split("\n");
        var longest = 0;
        foreach (var item in lstVal)
        {
            longest = item.Length > longest ? item.Length : longest;
        }

        var res = new string?[lstVal.Length][];
        for (var i = 0; i < lstVal.Length; i++)
        {
            res[i] = [lstVal[i] + new string(' ', longest - lstVal[i].Length)];
        }

        return res;
    }

    private string?[][] TreeToStr(TNode node, int depth = 0)
    {
        var val = GetVal(node);
        var children = _getChildren(node).Where(x => x != null).Cast<TNode>().ToList();
        if (children.Count == 0)
        {
            if (val.Length == 1)
            {
                string?[][] res = new[] { new[] { $"[{val[0][0]}]" } };
                return res;
            }
            else
            {
                var res = FormatBox("", val);
                return res;
            }
        }

        var toPrint = new List<List<string?>>() { new() };
        var spacingCount = 0;
        var spacing = "";
        if (depth + 1 != _maxDepth)
        {
            foreach (var childPrint in children.Select(child => TreeToStr(child, depth + 1)))
            {
                for (var l = 0; l < childPrint.Length; l++)
                {
                    var line = childPrint[l];
                    if (l + 1 >= toPrint.Count)
                    {
                        toPrint.Add([]);
                    }

                    if (l == 0)
                    {
                        var lineLen = LenJoin(line);
                        var middleOfChild = lineLen - (int)Math.Ceiling(line[^1]!.Length / 2d);
                        var toPrint0Len = LenJoin(toPrint[0]);
                        toPrint[0].Add(new string(' ', spacingCount - toPrint0Len + middleOfChild) + "┬");
                    }

                    var toPrintNxtLen = LenJoin(toPrint[l + 1]);
                    toPrint[l + 1].Add(new string(' ', spacingCount - toPrintNxtLen));
                    toPrint[l + 1].AddRange(line);
                }

                spacingCount = toPrint.Select(LenJoin).Prepend(0).Max();

                spacingCount++;
            }

            int pipePos;
            if (toPrint[0].Count != 1)
            {
                var newLines = string.Join("", toPrint[0]);
                var spaceBefore = newLines.Length - (newLines = newLines.Trim()).Length;
                var lenOfTrimmed = newLines.Length;
                newLines = new string(' ', spaceBefore) +
                           "┌" + newLines.Substring(1, newLines.Length - 2).Replace(' ', '─') + "┐";
                var middle = newLines.Length - (int)Math.Ceiling(lenOfTrimmed / 2d);
                pipePos = middle;
                var newCh = new Dictionary<char, char> { { '─', '┴' }, { '┬', '┼' }, { '┌', '├' }, { '┐', '┤' } }[newLines[middle]];
                newLines = newLines.Substring(0, middle) + newCh + newLines.Substring(middle + 1);
                toPrint[0] = new List<string?> { newLines };
            }
            else
            {
                toPrint[0][0] = string.Concat(toPrint[0][0]!.AsSpan(0, toPrint[0][0]!.Length - 1), "│");
                pipePos = toPrint[0][0]!.Length - 1;
            }

            if (val[0][0]!.Length < pipePos * 2)
            {
                spacing = new string(' ', pipePos - (int)Math.Ceiling(val[0][0]!.Length / 2d));
            }
        }

        val = val.Length == 1 ? [[spacing, $"[{val[0][0]}]"]] : FormatBox(spacing, val);

        var asArr = new string?[val.Length + toPrint.Count][];
        var row = 0;
        foreach (var item in val)
        {
            asArr[row] = new string[item.Length];
            for (var i = 0; i < item.Length; i++)
            {
                asArr[row][i] = item[i];
            }

            row++;
        }

        foreach (var item in toPrint)
        {
            asArr[row] = new string[item.Count];
            var i = 0;
            foreach (var x in item)
            {
                asArr[row][i] = x;
                i++;
            }

            row++;
        }

        return asArr;
    }

    private static bool IsNode(string? x)
    {
        if (string.IsNullOrEmpty(x))
        {
            return false;
        }

        if (x[0] == '[' || x[0] == '|' || (x[0] == '│' && x.TrimEnd().Length > 1))
        {
            return true;
        }

        if (x.Length < 2)
        {
            return false;
        }

        var middle = new string('─', x.Length - 2);
        return x.Equals($"┌{middle}┐") || x.Equals($"└{middle}┘");
    }

    private string ColorTxt(string? txt)
    {
        ArgumentException.ThrowIfNullOrEmpty(txt);
        
        var spaces = new string(' ', txt.Length - (txt = txt.TrimStart()).Length);
        var isLabel = txt.StartsWith('|');
        if (isLabel)
        {
            throw new InvalidOperationException();
        }

        txt = _border ? txt : $" {txt.Substring(1, txt.Length - 2)} ";
        txt = _color == Color.None ? txt : $"\u001b[{ColorToNum[_color]}m{txt}\u001b[0m";
        return spaces + txt;
    }

    private static int LenJoin(IEnumerable<string?> lst) => string.Join("", lst).Length;

    private string?[][] FormatBox(string? spacing, string?[][] val)
    {
        string?[][] res;
        var start = 0;
        if (_border)
        {
            res = new string?[val.Length + 2][];
            start = 1;
            var middle = new string('─', val[0][0]!.Length);
            res[0] = [spacing, '┌' + middle + '┐'];
            res[^1] = [spacing, '└' + middle + '┘'];
        }
        else
        {
            res = new string?[val.Length][];
        }

        for (var r = 0; r < val.Length; r++)
        {
            res[r + start] = [spacing, $"│{val[r][0]}│"];
        }

        return res;
    }

    [GeneratedRegex("(\\\\n|\n)", RegexOptions.Compiled)]
    private static partial Regex MyRegex();
}