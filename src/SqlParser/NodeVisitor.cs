using System.Diagnostics;
using System.Reflection;
using SqlParser.Nodes;

namespace SqlParser;

public abstract class NodeVisitor
{
    private static readonly Dictionary<Type, MethodInfo[]> MethodInfosMap = new();

    protected object? Visit(IAST node)
    {
        var m = GetMethodEndingWith(node.GetType().Name);
        if (m is null) throw new InvalidOperationException($"No Visit{node.GetType()} method");
        var result = m.Invoke(this, [node]);

        return result;
    }

    private MethodInfo? GetMethodEndingWith(string endsWith)
    {
        MethodInfosMap.TryGetValue(GetType(), out var methodInfos);
        if (methodInfos is null)
        {
            var type = GetType();
            methodInfos = type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            MethodInfosMap.TryAdd(GetType(), methodInfos);
        }

        return methodInfos.FirstOrDefault(m => m.Name.EndsWith(endsWith));
    }
}