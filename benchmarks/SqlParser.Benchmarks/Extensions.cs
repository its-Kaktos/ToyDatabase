namespace SqlParser.Benchmarks;

internal static class Extensions
{
    public static int[] Perm(this Random r, int n)
    {
        var m = new int[n];
        for (var i = 0; i < n; i++)
        {
            var j = r.Next(i + 1);
            (m[i], m[j]) = (m[j], i);
        }

        return m;
    }
}